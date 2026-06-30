using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Todo.Application.Common.Interfaces;
using Todo.Infrastructure.BulkCompletion;
using Todo.Infrastructure.ExternalApi;
using Todo.Infrastructure.Sync;
using Todo.Persistence;

namespace Todo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddPersistence(configuration);

        services.AddSingleton<IBulkCompletionService, BulkCompletionService>();

        AddExternalApiSync(services, configuration);

        return services;
    }

    private static void AddExternalApiSync(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ExternalApiOptions>(
            configuration.GetSection(ExternalApiOptions.SectionName)
        );
        services.Configure<SyncOptions>(configuration.GetSection(SyncOptions.SectionName));

        var externalApiOptions =
            configuration.GetSection(ExternalApiOptions.SectionName).Get<ExternalApiOptions>()
            ?? new ExternalApiOptions();

        services
            .AddHttpClient<IExternalTodoApiClient, ExternalTodoApiClient>(client =>
            {
                client.BaseAddress = new Uri(externalApiOptions.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(externalApiOptions.TimeoutSeconds);
            })
            // Retry transient failures (5xx, 408, network errors) with exponential backoff.
            .AddResilienceHandler(
                "external-todo-api",
                builder =>
                    builder.AddRetry(
                        new Microsoft.Extensions.Http.Resilience.HttpRetryStrategyOptions
                        {
                            MaxRetryAttempts = externalApiOptions.RetryCount,
                            BackoffType = DelayBackoffType.Exponential,
                            UseJitter = true,
                            Delay = TimeSpan.FromSeconds(externalApiOptions.RetryBaseDelaySeconds),
                        }
                    )
            );

        services.AddHostedService<SyncBackgroundService>();
    }
}
