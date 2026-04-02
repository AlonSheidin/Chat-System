using ChatSystem.Application.Interfaces;
using ChatSystem.IntegrationTests.Mocks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Moq;

namespace ChatSystem.IntegrationTests;

public class TestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly MockEventBus MockEventBus = new();

    public TestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Secret"] = "super_secret_key_that_is_long_enough_for_hmac_sha256",
                    ["Jwt:Issuer"] = "ChatSystem",
                    ["Jwt:Audience"] = "ChatSystemClients",
                    ["Jwt:ExpiryMinutes"] = "60",
                    ["UseInMemoryDatabase"] = "true",
                    // Disable real Kafka/Redis for tests
                    ["Kafka:BootstrapServers"] = "localhost:9092", 
                    ["Redis"] = "localhost:6379"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Override Kafka
                services.AddSingleton<IEventProducer>(MockEventBus);
                services.AddSingleton<IEventConsumer>(MockEventBus);

                // Mock Redis to avoid connection errors and null references
                var mockDatabase = new Mock<IDatabase>();
                var mockMultiplexer = new Mock<IConnectionMultiplexer>();
                mockMultiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);
                
                services.AddSingleton<IConnectionMultiplexer>(mockMultiplexer.Object);
            });
        });
    }
}
