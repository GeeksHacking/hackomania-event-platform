using SqlSugar;

namespace GeeksHackingPortal.Api.Entities;

[SugarIndex("IX_ActivityOrganizerInvite_Code", nameof(Code), OrderByType.Asc, IsUnique = true)]
public class ActivityOrganizerInvite
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public Guid ActivityId { get; set; }

    public string Code { get; set; } = null!;

    public OrganizerType Type { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Maximum number of times the invite can be used. Null means unlimited.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public int? MaxUses { get; set; }

    [Navigate(NavigateType.ManyToOne, nameof(ActivityId))]
    public Activity Activity { get; set; } = null!;

    [Navigate(NavigateType.OneToMany, nameof(ActivityOrganizerInviteUse.InviteId))]
    public List<ActivityOrganizerInviteUse> Uses { get; set; } = null!;
}

