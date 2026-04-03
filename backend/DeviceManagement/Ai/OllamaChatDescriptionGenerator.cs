using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DeviceManagement.Contracts.Devices;
using Microsoft.Extensions.Options;

namespace DeviceManagement.Ai;

/// <summary>
/// Uses Ollama’s OpenAI-compatible <c>POST /v1/chat/completions</c> API (default <c>http://127.0.0.1:11434/v1</c>).
/// </summary>
public sealed class OllamaChatDescriptionGenerator : IDeviceDescriptionGenerator
{
    private const int MaxDescriptionLength = 2000;

    private readonly HttpClient _http;
    private readonly LlmDescriptionOptions _opt;

    public OllamaChatDescriptionGenerator(
        IHttpClientFactory httpClientFactory,
        IOptions<LlmDescriptionOptions> options)
    {
        _http = httpClientFactory.CreateClient("LlmDescription");
        _opt = options.Value;
    }

    public async Task<string> GenerateAsync(GenerateDeviceDescriptionRequest specs, CancellationToken cancellationToken)
    {
        var model = string.IsNullOrWhiteSpace(_opt.Model) ? "llama3.2" : _opt.Model.Trim();

        var userContent = $"""
            Device specifications:
            - Name: {specs.Name}
            - Manufacturer: {specs.Manufacturer}
            - Type: {specs.Type}
            - OS: {specs.Os} {specs.OsVersion}
            - Processor: {specs.Processor}
            - RAM: {specs.RamGb} GB

            Write exactly one concise sentence (at most 40 words) for a company IT inventory.
            Do not read the specs back as a list; weave them into a short natural characterization (e.g. role, class, or tier).
            Add one plausible real-world suitability inferred from the hardware and software (e.g. office productivity, field work, development, meetings, or everyday business use)—not a second list of features.
            Tone: professional, plain language. No markdown, bullet lists, or quotation marks.
            """;

        var body = new ChatCompletionRequestDto
        {
            Model = model,
            Messages =
            [
                new ChatMessagePartDto("system",
                    "You write short device descriptions for corporate asset records. Synthesize specifications into a readable summary and who it is suited for; avoid feature-dump phrasing. Output only the sentence, nothing else."),
                new ChatMessagePartDto("user", userContent)
            ],
            MaxTokens = 180,
            Temperature = 0.5
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        if (!string.IsNullOrWhiteSpace(_opt.ApiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opt.ApiKey.Trim());

        request.Content = JsonContent.Create(body, options: WriteOptions);

        using var response = await _http.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"LLM API returned {(int)response.StatusCode}: {Truncate(json, 500)}");

        var parsed = JsonSerializer.Deserialize<ChatCompletionResponseDto>(json, ReadOptions);
        var text = parsed?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("The model returned an empty description.");

        if (text.Length > MaxDescriptionLength)
            text = text[..MaxDescriptionLength].TrimEnd();

        return text;
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class ChatCompletionRequestDto
    {
        [JsonPropertyName("model")]
        public required string Model { get; init; }

        [JsonPropertyName("messages")]
        public required List<ChatMessagePartDto> Messages { get; init; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; init; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; init; }
    }

    private sealed class ChatMessagePartDto
    {
        [JsonPropertyName("role")]
        public string Role { get; init; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; init; } = string.Empty;

        public ChatMessagePartDto(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }

    private sealed class ChatCompletionResponseDto
    {
        public List<ChatChoiceDto>? Choices { get; set; }
    }

    private sealed class ChatChoiceDto
    {
        public ChatMessageBodyDto? Message { get; set; }
    }

    private sealed class ChatMessageBodyDto
    {
        public string? Content { get; set; }
    }
}
