namespace KVS.Repositories;

public static class RepositoryInstallers
{
    static public IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IKeyValueCache, KeyValueCache>(provider => new KeyValueCache(provider));

        services.AddTransient<IKeyValueDatabase, KeyValueDatabase>();
        services.AddTransient<IKeyValueRepository, KeyValueRepository>();

        return services;
    }
}
