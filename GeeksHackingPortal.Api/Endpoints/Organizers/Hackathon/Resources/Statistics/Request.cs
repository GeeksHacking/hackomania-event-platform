using FastEndpoints;

namespace GeeksHackingPortal.Api.Endpoints.Organizers.Hackathon.Resources.Statistics;

public class Request
{
    public Guid ActivityId { get; set; }

    [QueryParam]
    public Guid? ResourceId { get; set; }
}
