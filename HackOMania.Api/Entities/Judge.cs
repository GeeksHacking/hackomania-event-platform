using SqlSugar;

namespace HackOMania.Api.Entities;

public class Judge
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string Name { get; set; }

    public Guid Secret { get; set; } = Guid.NewGuid();

    public bool Active { get; set; } = true;

    public Guid HackathonId { get; set; }
}
