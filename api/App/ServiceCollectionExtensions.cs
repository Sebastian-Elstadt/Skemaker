using App.Abstractions;
using App.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApp(this IServiceCollection services)
    {
        services.AddScoped<IDocumentsService, DocumentsService>();
        services.AddScoped<IDocumentAnalysisService, DocumentAnalysisService>();

        return services;
    }
}