using System.Text.Json.Serialization;

namespace DashiToon.Api.Infrastructure.Payment.Paypal.Models;

public class PlanInfo
{
    [JsonPropertyName("id")] public string Id { get; set; } = null!;
    [JsonPropertyName("product_id")] public string ProductId { get; set; } = null!;
    [JsonPropertyName("name")] public string Name { get; set; } = null!;
    [JsonPropertyName("status")] public string Status { get; set; } = null!;
    [JsonPropertyName("description")] public string Description { get; set; } = null!;
    [JsonPropertyName("create_time")] public DateTimeOffset CreateTime { get; set; }
    [JsonPropertyName("links")] public object[] Links { get; set; } = [];
}
