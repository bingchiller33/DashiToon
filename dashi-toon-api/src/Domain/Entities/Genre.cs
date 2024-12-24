namespace DashiToon.Api.Domain.Entities;

public class Genre : BaseAuditableEntity<int>
{
    public Genre(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; init; }
    public string Description { get; init; }
}
