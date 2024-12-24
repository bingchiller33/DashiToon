using System.Data;
using System.Text.Json;
using Dapper;

namespace DashiToon.Api.Infrastructure.Repositories.TypeHandlers;

public class StringArrayTypeHandler : SqlMapper.TypeHandler<string[]>
{
    public override void SetValue(IDbDataParameter parameter, string[]? value)
    {
        parameter.Value = JsonSerializer.Serialize(value, JsonSerializerOptions.Default);
    }

    public override string[] Parse(object value)
    {
        if (value is string stringValue)
        {
            return JsonSerializer.Deserialize<string[]>(stringValue, JsonSerializerOptions.Default) ?? [];
        }

        return [];
    }
}
