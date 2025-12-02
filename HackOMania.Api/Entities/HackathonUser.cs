using SqlSugar;

namespace HackOMania.Api.Entities;

public abstract class HackathonUser
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid HackathonId { get; set; }

    [SugarColumn(IsPrimaryKey = true)]
    public Guid UserId { get; set; }

    [Navigate(NavigateType.OneToMany, nameof(ResourceRedemption.RedeemerId))]
    public List<ResourceRedemption> Redemptions { get; set; } = null!;
}
