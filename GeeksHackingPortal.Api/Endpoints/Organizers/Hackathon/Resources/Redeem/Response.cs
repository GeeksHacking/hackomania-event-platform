namespace GeeksHackingPortal.Api.Endpoints.Organizers.Hackathon.Resources.Redeem;

public class Response
{
    public Guid RedemptionId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid ActivityId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
