using System.Text;
using ChatSystem.Application.Interfaces;
using ChatSystem.Application.Interfaces.Repositories;
using ChatSystem.Application.Interfaces.Services;
using ChatSystem.Infrastructure.Authentication;
using ChatSystem.Infrastructure.Options;
using ChatSystem.Infrastructure.Persistence;
using ChatSystem.Infrastructure.Repositories;
using ChatSystem.Infrastructure.Services;
using ChatSystem.Infrastructure.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace ChatSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ChatDbContext>(options =>
        {
            if (configuration["UseInMemoryDatabase"] == "true")
            {
                options.UseInMemoryDatabase("ChatDb");
            }
            else
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            }
        });

        // Options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));

        // Authentication
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions?.Issuer,
                    ValidAudience = jwtOptions?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions?.Secret ?? ""))
                };
                
                // Allow JWT via query string for SignalR
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/ws"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        // Services
        services.AddSingleton<IConnectionTracker, ConnectionTracker>();
        services.AddSingleton<IPresenceService, PresenceService>();
        services.AddSingleton<IMessageCache, MessageCache>();
        services.AddSingleton<IEventProducer, KafkaEventProducer>();
        services.AddSingleton<IEventConsumer, KafkaEventConsumer>();
        services.AddSingleton<IKafkaTopicInitializer, KafkaTopicInitializer>();

        // Ingress Buffer
        services.AddSingleton<KafkaIngressWorker>();
        services.AddSingleton<IBackgroundEventPublisher>(sp => sp.GetRequiredService<KafkaIngressWorker>());
        services.AddHostedService(sp => sp.GetRequiredService<KafkaIngressWorker>());

        // Workers
        services.AddHostedService<MessageWorker>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtProvider, JwtProvider>();

        // Redis
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            try
            {
                var redis = ConnectionMultiplexer.Connect(redisConnectionString);
                services.AddSingleton<IConnectionMultiplexer>(redis);
                services.AddSingleton<IRedisService, RedisService>();
            }
            catch (Exception ex)
            {
                // Log or handle the failure - the app will still start but Redis features will fail gracefully
                Console.WriteLine($"Warning: Could not connect to Redis at startup: {ex.Message}");
                // We still register the service but it might not be fully functional
                // Alternatively, we could register a 'NoOp' Redis service
            }
        }

        return services;
    }
}
