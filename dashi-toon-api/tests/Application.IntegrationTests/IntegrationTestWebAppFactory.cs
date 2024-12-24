using System.Data.Common;
using Bogus;
using DashiToon.Api.Application.AuthorStudio.DashiFans.EventHandlers;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Subscriptions.Commands.Models;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.ValueObjects;
using DashiToon.Api.Infrastructure.Data;
using DashiToon.Api.Infrastructure.Payment.Paypal.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PaypalServerSdk.Standard.Models;

namespace Application.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly DbConnection _connection;

    public IntegrationTestWebAppFactory(DbConnection connection)
    {
        _connection = connection;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");

        builder.ConfigureTestServices(services =>
        {
            services
                .RemoveAll<IUser>()
                .AddTransient(provider =>
                {
                    Mock<IUser> user = new();

                    user.Setup(s => s.Id).Returns(Testing.GetUserId());
                    user.Setup(s => s.Name).Returns(Testing.GetUserName());

                    return user.Object;
                });

            services
                .RemoveAll<DbContextOptions<ApplicationDbContext>>()
                .AddDbContext<ApplicationDbContext>((sp, options) =>
                {
                    options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                    options.UseNpgsql(_connection);
                });

            services
                .RemoveAll<IDbConnectionFactory>()
                .AddScoped<IDbConnectionFactory>(_ =>
                    new NpgsqlConnectionFactory(_connection.ConnectionString + ";Password=DashiToon@123!"));

            services
                .RemoveAll<IImageStore>()
                .AddScoped(provider =>
                {
                    Mock<IImageStore> imageStore = new();

                    imageStore.Setup(s => s.GetUrl(It.IsAny<string>(), It.IsAny<DateTime?>()))
                        .ReturnsAsync((string imageUrl, DateTime? _) => imageUrl);

                    return imageStore.Object;
                });

            services
                .RemoveAll<IPaymentService>()
                .AddScoped(provider =>
                {
                    Mock<IPaymentService>? paypalService = new();

                    paypalService.Setup(s => s.CreatePlan(It.IsAny<DashiFan>()))
                        .ReturnsAsync(new PlanResult(200, new Order { Id = "XD-LMAO-69-420" }));

                    paypalService.Setup(s => s.CreateSubscription(
                            It.IsAny<DashiFan>(),
                            It.IsAny<IDomainUser>(),
                            It.IsAny<string>(),
                            It.IsAny<string>()))
                        .ReturnsAsync(new SubscriptionResult(
                                201,
                                new SubscriptionInfo
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Links =
                                    [
                                        new PayPalLink
                                        {
                                            Href = "https://sandbox.paypal.com", Rel = "approve", Method = "POST"
                                        },
                                        new PayPalLink
                                        {
                                            Href = "https://sandbox.paypal.com", Rel = "self", Method = "GET"
                                        },
                                        new PayPalLink
                                        {
                                            Href = "https://sandbox.paypal.com", Rel = "edit", Method = "PUT"
                                        }
                                    ]
                                }
                            )
                        );

                    paypalService.Setup(s => s.SuspendSubscription(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(true);
                    paypalService.Setup(s => s.CancelSubscription(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(true);
                    paypalService.Setup(s => s.ReactivateSubscription(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(true);

                    return paypalService.Object;
                });

            services
                .RemoveAll<ISearchService>()
                .AddScoped(provider => Mock.Of<ISearchService>());

            services
                .RemoveAll<ICurrencyService>()
                .AddScoped(provider =>
                {
                    Mock<ICurrencyService>? currencyService = new();

                    currencyService.Setup(s => s.ConvertPrice(It.IsAny<Price>(), It.IsAny<string>()))
                        .Returns((Price price, string _) => price);

                    return currencyService.Object;
                });

            services
                .RemoveAll<IModerationService>()
                .AddScoped(provider =>
                {
                    Faker? faker = new();

                    const bool flagged = true;

                    List<CategoryScore>? flaggedCategories = new()
                    {
                        new CategoryScore { Category = "sexual", Score = faker.Random.Float() },
                        new CategoryScore { Category = "self-harm", Score = faker.Random.Float() }
                    };

                    Mock<IModerationService>? moderationService = new();

                    moderationService.Setup(s => s.ModerateComment(It.IsAny<string>()))
                        .ReturnsAsync(ModerationAnalysis.Create(flagged, flaggedCategories));

                    moderationService.Setup(s => s.ModerateReview(It.IsAny<string>()))
                        .ReturnsAsync(ModerationAnalysis.Create(flagged, flaggedCategories));

                    moderationService.Setup(s => s.ModerateSeries(
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<string>()))
                        .ReturnsAsync(ModerationAnalysis.Create(flagged, flaggedCategories));

                    moderationService.Setup(s => s.ModerateComicChapter(
                            It.IsAny<List<string>>()))
                        .ReturnsAsync(ModerationAnalysis.Create(flagged, flaggedCategories));

                    moderationService.Setup(s => s.ModerateNovelChapter(
                            It.IsAny<List<string>>(),
                            It.IsAny<string>()))
                        .ReturnsAsync(ModerationAnalysis.Create(flagged, flaggedCategories));

                    return moderationService.Object;
                });
        });
    }
}
