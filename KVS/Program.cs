using Asp.Versioning;
using KVS.Data;
using KVS.Endpoints.v1;
using KVS.Installers;
using KVS.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
});

builder.Services.AddRabbitMq(builder.Configuration);

builder.Services.AddDbContext<DatabaseContext>();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    // This will format the version as v{MAJOR}{MINOR}{STATUS}
    options.GroupNameFormat = "'v'VVV";

    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddRepositories();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.EnsureDatabaseIsUpToDateAsync();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();

        // Adds the swagger endpoints for each of the versions
        foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }
    });
}

app.UseHttpsRedirection();

app.MapKeyValueV1Endpoints();

app.Run();

public partial class Program { }