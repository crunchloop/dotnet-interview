using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Todo.Application.Common.Interfaces;
using Todo.Infrastructure.ExternalApi;

namespace Todo.Application.Tests.Sync;

public class ExternalTodoApiClientRetryTests
{
    /// <summary>
    /// Stub primary handler that returns a queued sequence of status codes, counting calls.
    /// </summary>
    private sealed class SequencedHandler : HttpMessageHandler
    {
        private readonly Queue<HttpStatusCode> _statuses;

        public SequencedHandler(IEnumerable<HttpStatusCode> statuses) =>
            _statuses = new Queue<HttpStatusCode>(statuses);

        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Calls++;
            var status = _statuses.Count > 0 ? _statuses.Dequeue() : HttpStatusCode.OK;
            var response = new HttpResponseMessage(status) { Content = new StringContent("[]") };
            return Task.FromResult(response);
        }
    }

    private static ServiceProvider BuildProvider(SequencedHandler handler, int retryCount)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(handler);

        services
            .AddHttpClient<IExternalTodoApiClient, ExternalTodoApiClient>(client =>
                client.BaseAddress = new Uri("http://localhost/")
            )
            .ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<SequencedHandler>())
            .AddResilienceHandler(
                "test",
                builder =>
                    builder.AddRetry(
                        new HttpRetryStrategyOptions
                        {
                            MaxRetryAttempts = retryCount,
                            BackoffType = DelayBackoffType.Constant,
                            Delay = TimeSpan.FromMilliseconds(1),
                            UseJitter = false,
                        }
                    )
            );

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task RetriesOnTransientFailures_ThenSucceeds()
    {
        // Two 503s then a 200 — the client should transparently recover.
        var handler = new SequencedHandler(
            new[]
            {
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.OK,
            }
        );

        using var provider = BuildProvider(handler, retryCount: 3);
        var client = provider.GetRequiredService<IExternalTodoApiClient>();

        var lists = await client.GetTodoListsAsync(CancellationToken.None);

        Assert.Empty(lists);
        Assert.Equal(3, handler.Calls); // 1 initial + 2 retries
    }

    [Fact]
    public async Task ExhaustsRetries_ThenThrows()
    {
        // Always failing — after the initial call + 3 retries the client surfaces the error.
        var handler = new SequencedHandler(
            Enumerable.Repeat(HttpStatusCode.ServiceUnavailable, 10)
        );

        using var provider = BuildProvider(handler, retryCount: 3);
        var client = provider.GetRequiredService<IExternalTodoApiClient>();

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.GetTodoListsAsync(CancellationToken.None)
        );

        Assert.Equal(4, handler.Calls); // 1 initial + 3 retries
    }
}
