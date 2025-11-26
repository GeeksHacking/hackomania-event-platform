using System.Security.Claims;
using FastEndpoints;

namespace HackOMania.Api.Endpoints.Auth.WhoAmI;

public class Endpoint : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("auth/whoami");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(
            new Response
            {
                GitHubId = int.Parse(
                    User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value
                ),
                GitHubLogin = User.Claims.First(c => c.Type == "login").Value,
            },
            ct
        );
    }
}
