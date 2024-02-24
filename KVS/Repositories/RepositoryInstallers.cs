﻿namespace KVS.Repositories;

public static class RepositoryInstallers
{
    static public IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<IKeyValueRepository, KeyValueRepository>();

        return services;
    }
}
