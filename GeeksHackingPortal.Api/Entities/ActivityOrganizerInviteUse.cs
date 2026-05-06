using SqlSugar;

namespace GeeksHackingPortal.Api.Entities;

[SugarIndex(
    "IX_ActivityOrganizerInviteUse_InviteId_UserId",
    nameof(InviteId),
    OrderByType.Asc,
    nameof(UserId),
    OrderByType.Asc,
    IsUnique = true
)]
public class ActivityOrganizerInviteUse
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public Guid InviteId { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset UsedAt { get; set; } = DateTimeOffset.UtcNow;

    [Navigate(NavigateType.ManyToOne, nameof(InviteId))]
    public ActivityOrganizerInvite Invite { get; set; } = null!;
}
