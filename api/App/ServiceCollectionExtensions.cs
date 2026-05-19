using App.Abstractions;
using App.Analysis;
using App.Documents;
using App.Translation;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApp(this IServiceCollection services)
    {
        services.AddScoped<IDocumentsService, DocumentsService>();
        services.AddScoped<IDocumentAnalysisService, DocumentAnalysisService>();
        services.AddScoped<IAnalysisTranslationService, AnalysisTranslationService>();

        return services;
    }
}