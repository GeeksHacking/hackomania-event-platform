namespace GeeksHackingPortal.Api.Endpoints.Organizers.Hackathon.Resources.History;

public class Request
{
    public Guid ActivityId { get; set; }
    public Guid ParticipantUserId { get; set; }
    public Guid ResourceId { get; set; }
}
