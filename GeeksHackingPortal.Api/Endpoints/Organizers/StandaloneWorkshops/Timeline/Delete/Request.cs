namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Timeline.Delete;

public class Request
{
    public Guid StandaloneWorkshopId { get; set; }
    public Guid TimelineItemId { get; set; }
}
