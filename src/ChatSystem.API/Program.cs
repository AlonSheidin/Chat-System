using ChatSystem.API.Hubs;
using ChatSystem.API.Workers;
using ChatSystem.Infrastructure;
using ChatSystem.Infrastructure.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatSystem API", Version = "v1" });
});

builder.Services.AddSignalR();
builder.Services.AddHostedService<NotificationWorker>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500") // Common ports for VS Code Live Server or similar
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();

        // For testing, allow everything (NOT for production!)
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Ensure Kafka topics are created
using (var scope = app.Services.CreateScope())
{
    var kafkaInitializer = scope.ServiceProvider.GetRequiredService<IKafkaTopicInitializer>();
    await kafkaInitializer.EnsureTopicsCreatedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/ws");

app.Run();

public partial class Program { }
