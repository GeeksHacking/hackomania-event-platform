namespace GeeksHackingPortal.Api.Endpoints.Organizers.Hackathon.Resources.Delete;

public class Request
{
    public Guid ActivityId { get; set; }
    public string ResourceId { get; set; } = null!;
}
