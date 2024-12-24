using System.Net;
using Amazon;
using Amazon.S3;
using currencyapi;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Infrastructure.Currency;
using DashiToon.Api.Infrastructure.Data;
using DashiToon.Api.Infrastructure.Data.Interceptors;
using DashiToon.Api.Infrastructure.Email;
using DashiToon.Api.Infrastructure.Identity;
using DashiToon.Api.Infrastructure.Image.Service;
using DashiToon.Api.Infrastructure.Image.Store;
using DashiToon.Api.Infrastructure.Moderation;
using DashiToon.Api.Infrastructure.Payment.Paypal;
using DashiToon.Api.Infrastructure.Repositories;
using DashiToon.Api.Infrastructure.Search;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Moderations;

namespace DashiToon.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabaseServices(configuration);

        services.AddIdentityServices(configuration);

        services.AddImageService(configuration);

        services.AddPaymentService(configuration);

        services.AddSearchService(configuration);

        services.AddCurrencyService(configuration);

        services.AddAiService(configuration);

        return services;
    }

    private static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ApplicationDbContextInitialiser>();

        services.AddScoped<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(connectionString));
        services.AddScoped<IChapterRepository, ChapterRepository>();
        services.AddScoped<ISeriesRepository, SeriesRepository>();
        services.AddScoped<IVolumeRepository, VolumeRepository>();

        services.AddScoped<IHomepageRepository, HomepageRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IAnalyticRepository, AnalyticRepository>();

        DapperTypeHandlerExtensions.AddDapperTypeHandler();

        return services;
    }

    private static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddCookie(IdentityConstants.ExternalScheme)
            .AddCookie(IdentityConstants.ApplicationScheme)
            .AddBearerToken(IdentityConstants.BearerScheme)
            .AddGoogle(options =>
            {
                string? clientId = configuration["Authentication:Google:ClientId"];
                string? clientSecret = configuration["Authentication:Google:ClientSecret"];

                Guard.Against.NullOrWhiteSpace(clientId, message: "Client id is missing.");
                Guard.Against.NullOrWhiteSpace(clientSecret, message: "ClientSecret is missing.");

                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
            });

        services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Task.CompletedTask;
            };

            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
        });

        services.AddAuthorizationBuilder();

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        services.AddTransient<IEmailSender<ApplicationUser>, EmailSender>();
        services.Configure<AuthMessageSenderOptions>(configuration);

        services.AddSingleton(TimeProvider.System);
        services.AddTransient<IIdentityService, IdentityService>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator));
        });

        return services;
    }


    private static IServiceCollection AddImageService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAmazonS3>(_ =>
            new AmazonS3Client(
                configuration["AWS:AccessKey"],
                configuration["AWS:SecretKey"],
                RegionEndpoint.GetBySystemName(configuration["AWS:Region"])));

        services.AddScoped<IImageStore, ImageStore>();

        services.AddScoped<IImageService, ImageService>();
        return services;
    }

    private static IServiceCollection AddPaymentService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PaypalOptions>(configuration.GetSection("Paypal"));

        services.AddHttpClient<IPaymentService, PaypalService>();

        return services;
    }

    private static IServiceCollection AddSearchService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ElasticsearchClient>(provider =>
        {
            ElasticsearchClientSettings? connectionSettings = new(
                new CloudNodePool(
                    configuration["Elasticsearch:CloudId"]!,
                    new ApiKey(configuration["Elasticsearch:ApiKey"]!
                    )
                )
            );

            connectionSettings.EnableDebugMode();

            return new ElasticsearchClient(connectionSettings);
        });

        services.AddSingleton<ISearchService, ElasticSearchService>(provider =>
        {
            IServiceScope? scope = provider.CreateScope();

            Task<ElasticSearchService>? service = ElasticSearchService.CreateAsync(
                scope.ServiceProvider.GetRequiredService<ElasticsearchClient>(),
                scope.ServiceProvider.GetRequiredService<IApplicationDbContext>(),
                scope.ServiceProvider.GetRequiredService<ILogger<ElasticSearchService>>()
            );

            return service.GetAwaiter().GetResult();
        });
        return services;
    }

    private static IServiceCollection AddCurrencyService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<Currencyapi>(provider => new Currencyapi(configuration["CurrencyApi:ApiKey"]!));

        services.AddScoped<ICurrencyService, CurrencyService>();

        return services;
    }

    private static IServiceCollection AddAiService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenAiOptions>(configuration.GetSection("OpenAi"));

        services.AddHttpClient<IModerationService, OpenAiModerationService>();

        return services;
    }
}
