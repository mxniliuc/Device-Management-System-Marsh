var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSwaggerGen();

builder.Services.Configure<DeviceManagement.MongoDb.MongoDbOptions>(
    builder.Configuration.GetSection(DeviceManagement.MongoDb.MongoDbOptions.SectionName));

builder.Services.AddSingleton<DeviceManagement.MongoDb.MongoDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
