using DbUp;
using System.Reflection;

namespace Infra.RecordStore;

public static class PostgresMigrator
{
    public static void MigrateDatabase(RecordStoreConfig config)
    {
        var result = DeployChanges.To
            .PostgresqlDatabase(config.ConnectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => s.EndsWith(".psql"))
            .LogToConsole()
            .Build()
            .PerformUpgrade();

        if (!result.Successful)
        {
            Console.Error.WriteLine(result.Error);
            throw new InvalidOperationException("Database migration failed.");
        }
    }
}