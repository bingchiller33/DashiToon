using Dapper;
using DashiToon.Api.Infrastructure.Repositories.TypeHandlers;

namespace DashiToon.Api.Infrastructure;

public static class DapperTypeHandlerExtensions
{
    public static void AddDapperTypeHandler()
    {
        SqlMapper.AddTypeHandler(new StringArrayTypeHandler());
        SqlMapper.AddTypeHandler(new StringListTypeHandler());
        SqlMapper.AddTypeHandler(new ReportTypeHandler());
    }
}
