namespace DeviceManagement.Ai;

public sealed class LlmDescriptionOptions
{
    public const string SectionName = "LlmDescription";

    /// <summary>When false, POST generate-description returns 503 without calling the LLM.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Bearer token for secured or cloud OpenAI-compatible APIs. Ollama on localhost usually needs this empty.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Ollama model tag, e.g. <c>llama3.2</c> (run <c>ollama pull llama3.2</c> once).</summary>
    public string Model { get; set; } = "llama3.2";

    /// <summary>OpenAI-compatible base URL including <c>/v1</c>. Default is local Ollama.</summary>
    public string BaseUrl { get; set; } = "http://127.0.0.1:11434/v1";
}
