namespace HackOMania.Api.Endpoints.Auth.WhoAmI;

public class Response
{
    public required int GitHubId { get; set; }
    public required string GitHubLogin { get; set; }
}
