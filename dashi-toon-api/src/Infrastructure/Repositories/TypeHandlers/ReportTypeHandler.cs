using System.Data;
using System.Text.Json;
using Dapper;
using DashiToon.Api.Application.Moderation.Model;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Infrastructure.Repositories.TypeHandlers;

public class ReportTypeHandler : SqlMapper.TypeHandler<List<ReportDto>>
{
    public override void SetValue(IDbDataParameter parameter, List<ReportDto>? value)
    {
        parameter.Value = JsonSerializer.Serialize(value);
    }

    public override List<ReportDto> Parse(object value)
    {
        if (value is not string stringValue)
        {
            return [];
        }

        List<ReportDto>? reports =
            JsonSerializer.Deserialize<List<ReportDto>>(stringValue, JsonSerializerOptions.Default) ?? [];

        foreach (ReportDto? report in reports.Where(report =>
                     !string.IsNullOrEmpty(report.AnalysisFlaggedCategoriesString)))
        {
            report.AnalysisFlaggedCategories.AddRange(
                JsonSerializer.Deserialize<List<CategoryScore>>(report.AnalysisFlaggedCategoriesString ??
                                                                string.Empty) ?? []);
        }

        return reports;
    }
}
