using MassTransit;

namespace KVS.Installers;

public static class RabbitMqInstaller
{
    static public void AddRabbitMq(this IServiceCollection services, IConfiguration config)
    {
        var rabbitMqHost = GetRabbitMqHost(config);
        var rabbitMqUsername = GetRabbitMqUsername(config);
        var rabbitMqPassword = GetRabbitMqPassword(config);

        services.AddMassTransit(options =>
        {
            options.AddConsumers(typeof(RabbitMqInstaller).Assembly);

            options.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitMqHost, hostCfg =>
                {
                    hostCfg.Username(rabbitMqUsername);
                    hostCfg.Password(rabbitMqPassword);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });
    }

    static private string GetRabbitMqHost(IConfiguration config)
    {
        var host = config.GetValue<string>(RABBIT_MQ_HOST_VARIABLE);
        if (host is null)
        {
            // TODO: LogCritical when this happens
            throw new InvalidOperationException($"Unable to find host variable '{RABBIT_MQ_HOST_VARIABLE}' for RabbitMQ.");
        }

        return host;
    }

    static private string GetRabbitMqUsername(IConfiguration config)
    {
        var host = config.GetValue<string>(RABBIT_MQ_USERNAME_VARIABLE);
        if (host is null)
        {
            throw new InvalidOperationException($"Unable to find username variable '{RABBIT_MQ_USERNAME_VARIABLE}' for RabbitMQ.");
        }

        return host;
    }

    static private string GetRabbitMqPassword(IConfiguration config)
    {
        var host = config.GetValue<string>(RABBIT_MQ_PASSWORD_VARIABLE);
        if (host is null)
        {
            throw new InvalidOperationException($"Unable to find password variable '{RABBIT_MQ_PASSWORD_VARIABLE}' for RabbitMQ.");
        }

        return host;
    }

    const string RABBIT_MQ_HOST_VARIABLE = "rabbit-mq-host";
    const string RABBIT_MQ_USERNAME_VARIABLE = "rabbit-mq-username";
    const string RABBIT_MQ_PASSWORD_VARIABLE = "rabbit-mq-password";
}
