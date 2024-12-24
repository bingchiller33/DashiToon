using System.Data;
using System.Text.Json;
using Dapper;

namespace DashiToon.Api.Infrastructure.Repositories.TypeHandlers;

public class StringListTypeHandler : SqlMapper.TypeHandler<List<string>>
{
    public override void SetValue(IDbDataParameter parameter, List<string>? value)
    {
        parameter.Value = JsonSerializer.Serialize(value, JsonSerializerOptions.Default);
    }

    public override List<string> Parse(object value)
    {
        if (value is string stringValue)
        {
            return JsonSerializer.Deserialize<List<string>>(stringValue, JsonSerializerOptions.Default) ?? [];
        }

        return [];
    }
}
