namespace GeeksHackingPortal.Api.Endpoints.Organizers.Hackathon.Resources.List;

public class Response
{
    public IEnumerable<ResponseResource> Resources { get; set; } = [];

    public class ResponseResource
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string RedemptionStmt { get; set; } = "true";
        public bool IsPublished { get; set; }
    }
}
