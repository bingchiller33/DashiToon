namespace DashiToon.Api.Application.Common.Security;

[AttributeUsage(AttributeTargets.Class)]
public class RestrictAttribute : Attribute
{
    public RestrictAttribute()
    {
    }

    public required string Require { get; set; }
}
