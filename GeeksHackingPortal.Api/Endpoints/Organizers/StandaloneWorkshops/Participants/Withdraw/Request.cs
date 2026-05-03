namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Participants.Withdraw;

public class Request
{
    public Guid StandaloneWorkshopId { get; set; }
    public Guid UserId { get; set; }
}
