namespace GeeksHackingPortal.Api.Endpoints.Organizers.Hackathon.Organizers.Invites.Revoke;

public class Request
{
    public Guid HackathonId { get; set; }
    public Guid InviteId { get; set; }
}
