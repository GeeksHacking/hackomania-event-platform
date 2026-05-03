namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Timeline.Update;

public class Request
{
    public Guid StandaloneWorkshopId { get; set; }
    public Guid TimelineItemId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
}
