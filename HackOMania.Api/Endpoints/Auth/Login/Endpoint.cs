using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace HackOMania.Api.Endpoints.Auth.Login;

public class Endpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("auth/login");
        AllowAnonymous();
        Description(d => d.ExcludeFromDescription());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.ResultAsync(
            Results.Challenge(
                properties: new AuthenticationProperties { RedirectUri = "/" },
                authenticationSchemes: [Providers.GitHub]
            )
        );
    }
}
