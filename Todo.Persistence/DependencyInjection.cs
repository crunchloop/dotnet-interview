using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Todo.Application.Common.Interfaces;

namespace Todo.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<TodoContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("TodoContext"))
        );

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<TodoContext>()
        );

        return services;
    }
}
