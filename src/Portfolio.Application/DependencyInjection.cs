using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Portfolio.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddValidatorsFromAssembly(assembly);
        return services;
    }
}

