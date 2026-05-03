namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Timeline.List;

public class Response
{
    public required List<TimelineItemDto> TimelineItems { get; set; }
}

public class TimelineItemDto
{
    public Guid Id { get; set; }
    public Guid StandaloneWorkshopId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
