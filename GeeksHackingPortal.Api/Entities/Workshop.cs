using SqlSugar;

namespace GeeksHackingPortal.Api.Entities;

public class Workshop
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public Guid HackathonId { get; set; }

    [Navigate(NavigateType.OneToOne, nameof(Id))]
    public Activity Activity { get; set; } = null!;

    [SugarColumn(ColumnName = "Title", IsNullable = true)]
    public string? LegacyTitle { get; set; }

    [SugarColumn(ColumnName = "Description", IsNullable = true, ColumnDataType = "longtext")]
    public string? LegacyDescription { get; set; }

    [SugarColumn(ColumnName = "StartTime")]
    public DateTimeOffset LegacyStartTime { get; set; }

    [SugarColumn(ColumnName = "EndTime")]
    public DateTimeOffset LegacyEndTime { get; set; }

    [SugarColumn(ColumnName = "Location", IsNullable = true)]
    public string? LegacyLocation { get; set; }

    public int MaxParticipants { get; set; }

    [SugarColumn(ColumnName = "IsPublished")]
    public bool LegacyIsPublished { get; set; }

    [SugarColumn(ColumnName = "CreatedAt")]
    public DateTimeOffset LegacyCreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [SugarColumn(ColumnName = "UpdatedAt")]
    public DateTimeOffset LegacyUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Navigate(NavigateType.OneToMany, nameof(WorkshopParticipant.WorkshopId))]
    public List<WorkshopParticipant> Participants { get; set; } = null!;
}
