using SqlSugar;

namespace HackOMania.Api.Entities;

/// <summary>
/// Records each withdrawal event for a participant.
/// A row with <see cref="RejoinedAt"/> = null means the participant is still withdrawn.
/// </summary>
public class ParticipantWithdrawal
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public Guid ParticipantId { get; set; }

    public DateTimeOffset WithdrawnAt { get; set; }

    /// <summary>
    /// Null while the participant is still withdrawn; set when they re-join.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTimeOffset? RejoinedAt { get; set; }
}
