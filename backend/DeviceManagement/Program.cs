var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DeviceManagement.ExceptionHandling.GlobalExceptionHandler>();

builder.Services.Configure<DeviceManagement.MongoDb.MongoDbOptions>(
    builder.Configuration.GetSection(DeviceManagement.MongoDb.MongoDbOptions.SectionName));

builder.Services.AddSingleton<DeviceManagement.MongoDb.MongoDbContext>();

builder.Services.AddScoped<DeviceManagement.Repositories.IDeviceRepository, DeviceManagement.Repositories.DeviceRepository>();
builder.Services.AddScoped<DeviceManagement.Repositories.IUserRepository, DeviceManagement.Repositories.UserRepository>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
