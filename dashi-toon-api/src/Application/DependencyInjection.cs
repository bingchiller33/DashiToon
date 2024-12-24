using System.Reflection;
using DashiToon.Api.Application.Common.Behaviours;
using DashiToon.Api.Application.Common.Services;
using Ganss.Xss;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace DashiToon.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RestrictionBehaviour<,>));
        });

        services.AddScoped<IHtmlSanitizer, HtmlSanitizer>();
        services.AddScoped<NovelChapterService>();
        services.AddScoped<ComicChapterService>();
        services.AddSingleton<ChapterAnalyticService>(provider =>
        {
            IServiceScope? scope = provider.CreateScope();

            return new ChapterAnalyticService(
                scope.ServiceProvider.GetRequiredService<ISender>(),
                scope.ServiceProvider.GetRequiredService<IMemoryCache>()
            );
        });

        return services;
    }
}
