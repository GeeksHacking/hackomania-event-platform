namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Participants.Get;

public class Request
{
    public Guid StandaloneWorkshopId { get; set; }
    public Guid UserId { get; set; }
}
