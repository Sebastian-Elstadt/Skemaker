using System.Net.Http.Headers;
using App.Abstractions;
using Infra.Abstractions;
using Infra.Documents;
using Infra.FileStorage;
using Infra.RecordStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infra;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration config)
    {
        var fileStoreConfig = config.GetRequiredSection("FileStore").Get<FileStoreConfig>();
        if (fileStoreConfig is null) throw new InvalidOperationException("Missing FileStore configuration.");
        fileStoreConfig.EnsureValid();

        var recordStoreConfig = config.GetRequiredSection("RecordStore").Get<RecordStoreConfig>();
        if (recordStoreConfig is null) throw new InvalidOperationException("Missing RecordStore configuration.");
        recordStoreConfig.EnsureValid();

        var xAiConfig = config.GetRequiredSection("xAi").Get<xAiConfig>();
        if (xAiConfig is null) throw new InvalidOperationException("Missing xAi configuration.");
        xAiConfig.EnsureValid();

        PostgresMigrator.MigrateDatabase(recordStoreConfig);

        services.AddSingleton<IFileStore, VolumeFileStore>(sp => new VolumeFileStore(fileStoreConfig));

        services.AddScoped(sp => new PostgresRecordStore(recordStoreConfig));
        services.AddScoped<IRecordStore>(sp => sp.GetRequiredService<PostgresRecordStore>());
        services.AddScoped<IQueryExecutor>(sp => sp.GetRequiredService<PostgresRecordStore>()); // for infra-internal exposure

        services.AddScoped<xAiUploadStore>();
        services.AddHttpClient<IGdAndTAnalyzer, xAiGdAndTAnalyzer>(client =>
        {
            client.BaseAddress = new Uri(xAiConfig.BaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", xAiConfig.ApiKey);
        });

        return services;
    }
}