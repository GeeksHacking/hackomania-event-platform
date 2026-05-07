namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Organizers.Invites.Revoke;

public class Request
{
    public Guid StandaloneWorkshopId { get; set; }
    public Guid InviteId { get; set; }
}
