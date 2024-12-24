using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DashiToon.Api.Application.Subscriptions.Commands.HandleSubscriptionEvent;

public record EventInfo
{
    public DateTimeOffset CreateTime { get; set; }
    public string ResourceType { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public JsonObject Resource { get; set; } = default!;
}

public record SubscriptionActivated
{
    [JsonPropertyName("id")] public string Id { get; init; } = null!;
    [JsonPropertyName("plan_id")] public string PlanId { get; init; } = null!;
    [JsonPropertyName("status")] public string Status { get; init; } = null!;
    [JsonPropertyName("billing_info")] public Billing BillingInfo { get; init; } = null!;

    public record Billing
    {
        [JsonPropertyName("outstanding_balance")]
        public OutstandingBalance OutstandingBalance { get; init; } = null!;

        [JsonPropertyName("last_payment")] public LastPayment LastPayment { get; init; } = null!;

        [JsonPropertyName("next_billing_time")]
        public DateTimeOffset NextBillingTime { get; init; }

        [JsonPropertyName("failed_payments_count")]
        public int FailedPaymentsCount { get; init; }
    }

    public record OutstandingBalance
    {
        [JsonPropertyName("currency_code")] public string CurrencyCode { get; init; } = null!;
        [JsonPropertyName("value")] public string Value { get; init; } = null!;
    }

    public record LastPayment
    {
        [JsonPropertyName("amount")] public PaymentAmount Amount { get; init; } = null!;
        [JsonPropertyName("time")] public DateTime Time { get; init; }
    }

    public record PaymentAmount
    {
        [JsonPropertyName("currency_code")] public string CurrencyCode { get; init; } = null!;
        [JsonPropertyName("value")] public string Value { get; init; } = null!;
    }
}

public record SubscriptionPaymentCompleted
{
    public Amount Amount { get; init; } = null!;
    public string PaymentMode { get; init; } = null!;
    public DateTime CreateTime { get; init; }
    public TransactionFee TransactionFee { get; init; } = null!;
    public string BillingAgreementId { get; init; } = null!;
    public DateTime UpdateTime { get; init; }
    public string Id { get; init; } = null!;
    public string State { get; init; } = null!;
}

public record TransactionFee
{
    public string Currency { get; init; } = null!;
    public string Value { get; init; } = null!;
}

public record Amount
{
    public string Total { get; init; } = null!;
    public string Currency { get; init; } = null!;
}
