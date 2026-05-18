using App.Abstractions;
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
        if (fileStoreConfig is null || string.IsNullOrWhiteSpace(fileStoreConfig.BasePath))
            throw new InvalidOperationException("Invalid FileStore configuration.");

        var recordStoreConfig = config.GetRequiredSection("RecordStore").Get<RecordStoreConfig>();
        if (recordStoreConfig is null || string.IsNullOrWhiteSpace(recordStoreConfig.ConnectionString))
            throw new InvalidOperationException("Invalid RecordStore configuration.");

        PostgresMigrator.MigrateDatabase(recordStoreConfig);

        services.AddSingleton<IFileStore, VolumeFileStore>(sp => new VolumeFileStore(fileStoreConfig));
        services.AddScoped<IRecordStore, PostgresRecordStore>(sp => new PostgresRecordStore(recordStoreConfig));
        return services;
    }
}