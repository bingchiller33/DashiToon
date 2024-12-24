using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using DashiToon.Api.Application.AuthorStudio.DashiFans.EventHandlers;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Subscriptions.Commands.Models;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.ValueObjects;
using DashiToon.Api.Infrastructure.Identity;
using DashiToon.Api.Infrastructure.Payment.Paypal.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaypalServerSdk.Standard;
using PaypalServerSdk.Standard.Authentication;
using PaypalServerSdk.Standard.Controllers;
using PaypalServerSdk.Standard.Http.Response;
using PaypalServerSdk.Standard.Models;
using Environment = PaypalServerSdk.Standard.Environment;

namespace DashiToon.Api.Infrastructure.Payment.Paypal;

public class PaypalService : IPaymentService
{
    private static readonly Dictionary<string, CheckoutPaymentIntent> PaymentIntentMap = new()
    {
        { "CAPTURE", CheckoutPaymentIntent.Capture }, { "AUTHORIZE", CheckoutPaymentIntent.Authorize }
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<PaypalService> _logger;
    private readonly OrdersController _ordersController;
    private readonly ICurrencyService _currencyService;

    public PaypalService(
        IOptions<PaypalOptions> options,
        ILogger<PaypalService> logger,
        HttpClient httpClient,
        ICurrencyService currencyService)
    {
        PaypalServerSdkClient? client = new PaypalServerSdkClient.Builder()
            .Environment(Environment.Sandbox)
            .ClientCredentialsAuth(
                new ClientCredentialsAuthModel.Builder(options.Value.ClientId, options.Value.ClientSecret).Build()
            )
            .LoggingConfig(config =>
                config
                    .LogLevel(LogLevel.Information)
                    .RequestConfig(reqConfig => reqConfig.Headers(true))
                    .RequestConfig(reqConfig => reqConfig.Body(true))
                    .ResponseConfig(respConfig => respConfig.Headers(true))
                    .ResponseConfig(respConfig => respConfig.Body(true))
                    .Logger(logger)
            )
            .Build();

        _ordersController = client.OrdersController;
        _logger = logger;

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api-m.sandbox.paypal.com");
        _httpClient.DefaultRequestHeaders.Add(
            "Authorization",
            $"Bearer {client.ClientCredentialsAuth.FetchToken().AccessToken}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        _currencyService = currencyService;
    }

    public async Task<OrderResult> CreateOrder(Price price)
    {
        try
        {
            Price orderPrice = _currencyService.ConvertPrice(price, "USD");

            OrdersCreateInput ordersCreateInput = new()
            {
                Body = new OrderRequest
                {
                    Intent = PaymentIntentMap["CAPTURE"],
                    PurchaseUnits =
                    [
                        new PurchaseUnitRequest
                        {
                            Amount = new AmountWithBreakdown
                            {
                                CurrencyCode = orderPrice.Currency,
                                MValue = Math.Round(orderPrice.Amount, 2, MidpointRounding.ToPositiveInfinity)
                                    .ToString(CultureInfo.InvariantCulture)
                            }
                        }
                    ]
                }
            };

            ApiResponse<Order>? result = await _ordersController.OrdersCreateAsync(ordersCreateInput);

            return new OrderResult(result.StatusCode, result.Data);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create order: {ExMsg}", e.Message);
            return new OrderResult(500, "Failed to create order.");
        }
    }

    public async Task<OrderResult> CaptureOrder(string orderId)
    {
        try
        {
            OrdersCaptureInput ordersCaptureInput = new() { Id = orderId };

            ApiResponse<Order>? result = await _ordersController.OrdersCaptureAsync(ordersCaptureInput);

            return new OrderResult(result.StatusCode, result.Data);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to capture order: {ExMsg}", e.Message);
            return new OrderResult(500, "Failed to capture order.");
        }
    }

    public async Task<PlanResult> CreatePlan(DashiFan plan)
    {
        try
        {
            Price planPrice = _currencyService.ConvertPrice(plan.Price, "USD");

            var request = new
            {
                product_id = "DashiToon",
                name = plan.Name,
                billing_cycles =
                    new object[]
                    {
                        new
                        {
                            frequency = new { interval_unit = "MONTH", interval_count = 1 },
                            tenure_type = "REGULAR",
                            sequence = 1,
                            total_cycles = 0,
                            pricing_scheme =
                                new
                                {
                                    fixed_price = new
                                    {
                                        value = Math
                                            .Round(planPrice.Amount, 2, MidpointRounding.ToPositiveInfinity)
                                            .ToString(CultureInfo.InvariantCulture),
                                        currency_code = planPrice.Currency
                                    }
                                }
                        }
                    },
                payment_preferences = new { auto_bill_outstanding = true, payment_failure_threshold = 2 },
                description = plan.Description,
                status = "ACTIVE",
                taxes = new { percentage = "10", inclusive = true }
            };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("v1/billing/plans", request);

            _logger.LogInformation("Response Headers: {Header}", response.Headers);
            _logger.LogInformation("{StatusCode} Response Body: {Content}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            response.EnsureSuccessStatusCode();

            PlanInfo? data =
                await JsonSerializer.DeserializeAsync<PlanInfo>(await response.Content.ReadAsStreamAsync());

            return new PlanResult((int)response.StatusCode, data!);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create plan: {ExMsg}", e.Message);
            return new PlanResult(500, default!);
        }
    }

    public async Task<PlanResult> UpdatePlan(DashiFan tier)
    {
        try
        {
            HttpResponseMessage planDetailResponse = await _httpClient.GetAsync($"v1/billing/plans/{tier.PlanId}");

            PlanInfo? planInfo =
                JsonSerializer.Deserialize<PlanInfo>(await planDetailResponse.Content.ReadAsStreamAsync());

            if (planInfo!.Status != "ACTIVE")
            {
                HttpResponseMessage resp =
                    await _httpClient.PostAsync($"v1/billing/plans/{tier.PlanId}/activate", null);

                resp.EnsureSuccessStatusCode();
            }

            object[] request = new object[]
            {
                new { op = "replace", path = "/description", value = tier.Description },
                new { op = "replace", path = "/name", value = tier.Name }
            };

            HttpResponseMessage response =
                await _httpClient.PatchAsJsonAsync($"v1/billing/plans/{tier.PlanId}", request);

            // Deactivate again to restore original status
            if (planInfo!.Status != "ACTIVE")
            {
                HttpResponseMessage resp =
                    await _httpClient.PostAsync($"v1/billing/plans/{tier.PlanId}/deactivate", null);

                resp.EnsureSuccessStatusCode();
            }

            _logger.LogInformation("Response Headers: {Header}", response.Headers);
            _logger.LogInformation("{StatusCode} Response Body: {Content}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            response.EnsureSuccessStatusCode();

            return new PlanResult((int)response.StatusCode, default!);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update plan: {ExMsg}", e.Message);
            return new PlanResult(500, default!);
        }
    }

    public async Task UpdatePlanPricing(DashiFan tier)
    {
        try
        {
            HttpResponseMessage planDetailResponse = await _httpClient.GetAsync($"v1/billing/plans/{tier.PlanId}");

            PlanInfo? planInfo =
                JsonSerializer.Deserialize<PlanInfo>(await planDetailResponse.Content.ReadAsStreamAsync());

            if (planInfo!.Status != "ACTIVE")
            {
                HttpResponseMessage resp =
                    await _httpClient.PostAsync($"v1/billing/plans/{tier.PlanId}/activate", null);

                resp.EnsureSuccessStatusCode();
            }

            Price planPrice = _currencyService.ConvertPrice(tier.Price, "USD");

            var request = new
            {
                pricing_schemes = new object[]
                {
                    new
                    {
                        billing_cycle_sequence = 1,
                        pricing_scheme = new
                        {
                            fixed_price =
                                new
                                {
                                    value = Math.Round(planPrice.Amount, 2, MidpointRounding.ToPositiveInfinity)
                                        .ToString(CultureInfo.InvariantCulture),
                                    currency_code = planPrice.Currency
                                }
                        }
                    }
                }
            };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                $"v1/billing/plans/{tier.PlanId}/update-pricing-schemes",
                request);

            if (planInfo.Status != "ACTIVE")
            {
                HttpResponseMessage resp =
                    await _httpClient.PostAsync($"v1/billing/plans/{tier.PlanId}/deactivate", null);

                resp.EnsureSuccessStatusCode();
            }

            _logger.LogInformation("Response Headers: {Header}", response.Headers);
            _logger.LogInformation("{StatusCode} Response Body: {Content}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update plan pricing: {ExMsg}", e.Message);
        }
    }

    public async Task UpdatePlanStatus(DashiFan tier)
    {
        try
        {
            HttpResponseMessage response = tier.IsActive
                ? await _httpClient.PostAsync($"v1/billing/plans/{tier.PlanId}/activate", null)
                : await _httpClient.PostAsync($"v1/billing/plans/{tier.PlanId}/deactivate", null);

            _logger.LogInformation("Response Headers: {Header}", response.Headers);
            _logger.LogInformation("{StatusCode} Response Body: {Content}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update plan status: {ExMsg}", e.Message);
        }
    }

    public async Task<SubscriptionResult> CreateSubscription(
        DashiFan tier,
        IDomainUser user,
        string returnUrl,
        string cancelUrl)
    {
        try
        {
            ApplicationUser? appUser = user as ApplicationUser;

            var request = new
            {
                plan_id = tier.PlanId,
                quantity = 1,
                subscriber =
                    new
                    {
                        name = new { given_name = appUser!.UserName, surname = appUser.UserName },
                        email_address = appUser.Email
                    },
                application_context = new
                {
                    brand_name = "DashiToon",
                    locale = "en-US",
                    shipping_preference = "NO_SHIPPING",
                    user_action = "SUBSCRIBE_NOW",
                    payment_method =
                        new { payer_selected = "PAYPAL", payee_preferred = "IMMEDIATE_PAYMENT_REQUIRED" },
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            };

            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync("v1/billing/subscriptions", request);

            _logger.LogInformation("Response Headers: {Header}", response.Headers);
            _logger.LogInformation("{StatusCode} Response Body: {Content}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            SubscriptionInfo? data = await JsonSerializer.DeserializeAsync<SubscriptionInfo>(
                await response.Content.ReadAsStreamAsync());

            return new SubscriptionResult((int)response.StatusCode, data!);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create subscription: {ExMsg}", e.Message);
            return new SubscriptionResult(500, default!);
        }
    }

    public async Task<SubscriptionResult> GetSubscriptionDetail(Subscription subscription)
    {
        try
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync($"v1/billing/subscriptions/{subscription.SubscriptionId}");

            _logger.LogInformation("Response Headers: {Header}", response.Headers);
            _logger.LogInformation("{StatusCode} Response Body: {Content}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            SubscriptionInfo? data = await JsonSerializer.DeserializeAsync<SubscriptionInfo>(
                await response.Content.ReadAsStreamAsync());

            return new SubscriptionResult((int)response.StatusCode, data!);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create subscription: {ExMsg}", e.Message);
            return new SubscriptionResult(500, default!);
        }
    }

    public async Task<SubscriptionResult> ReviseSubscriptionPlan(Subscription subscription)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                $"v1/billing/subscriptions/{subscription.SubscriptionId}/revise",
                new { plan_id = subscription.Tier.PlanId });

            _logger.LogInformation("Response Headers: {Header}", response.Headers);
            _logger.LogInformation("{StatusCode} Response Body: {Content}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            SubscriptionInfo? data = await JsonSerializer.DeserializeAsync<SubscriptionInfo>(
                await response.Content.ReadAsStreamAsync());

            return new SubscriptionResult((int)response.StatusCode, data!);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to revise subscription plan: {ExMsg}", e.Message);
            return new SubscriptionResult(500, default!);
        }
    }

    public async Task<bool> ReactivateSubscription(string subscriptionId, string reason)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            $"v1/billing/subscriptions/{subscriptionId}/activate",
            new { reason });

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SuspendSubscription(string subscriptionId, string reason)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            $"v1/billing/subscriptions/{subscriptionId}/suspend",
            new { reason });

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CancelSubscription(string subscriptionId, string reason)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            $"v1/billing/subscriptions/{subscriptionId}/cancel",
            new { reason });

        return response.IsSuccessStatusCode;
    }

    public async Task PayoutRevenue(string accountId, Price revenue)
    {
        try
        {
            Price payout = _currencyService.ConvertPrice(revenue, "USD");

            var request = new
            {
                items = new object[]
                {
                    new
                    {
                        recipient_type = "PAYPAL_ID",
                        receiver = accountId,
                        amount = new
                        {
                            currency = payout.Currency,
                            value = Math.Round(payout.Amount, 2, MidpointRounding.ToNegativeInfinity)
                        },
                        note = "Cảm ơn vì đã sử dụng DashiToon"
                    }
                },
                sender_batch_header = new
                {
                    email_subject = "Bạn có một khoản thanh toán từ DashiToon",
                    email_message =
                        "Bạn đã nhận được khoản thanh toán! Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!"
                }
            };

            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync("v1/payments/payouts", request);

            _logger.LogInformation("Response Headers: {Header}", response.Headers);
            _logger.LogInformation("{StatusCode} Response Body: {Content}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to payout: {ExMsg}", e.Message);
        }
    }
}
