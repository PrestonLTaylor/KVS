using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace KVS.IntegrationTests.Helpers;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
internal class KeyValueV1ApiFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("POSTGRESQLCONNSTR_DefaultConnection", psqlContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("rabbit-mq-host", $"amqp://{rabbitMqContainer.Hostname}:{rabbitMqContainer.GetMappedPublicPort(RabbitMqBuilder.RabbitMqPort)}");
        Environment.SetEnvironmentVariable("rabbit-mq-username", RABBIT_MQ_USERNAME);
        Environment.SetEnvironmentVariable("rabbit-mq-password", RABBIT_MQ_PASSWORD);

        return base.CreateHost(builder);
    }

    protected PostgreSqlContainer psqlContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("testusername")
        .WithPassword("testpassword")
        .WithCleanUp(true)
        .Build();

    const string RABBIT_MQ_USERNAME = "testusername";
    const string RABBIT_MQ_PASSWORD = "testpassword";

    protected readonly RabbitMqContainer rabbitMqContainer = new RabbitMqBuilder()
        .WithUsername(RABBIT_MQ_USERNAME)
        .WithPassword(RABBIT_MQ_PASSWORD)
        .WithCleanUp(true)
        .Build();
}
