using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Security;
using HackOMania.Api.Entities;
using HackOMania.Api.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using SqlSugar;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace HackOMania.Api.Endpoints.Callback.Login.GitHub;

public class Endpoint(IOptions<AppOptions> options, ISqlSugarClient db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Routes("callback/login/github");
        Verbs(Http.POST, Http.GET);
        AllowAnonymous();

        Description(d => d.ExcludeFromDescription());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await HttpContext.AuthenticateAsync(Providers.GitHub);
        if (!result.Succeeded || result.Principal is null)
        {
            await Send.ForbiddenAsync(cancellation: ct);
            return;
        }

        var existing = await db.Queryable<GitHubOnlineAccount>()
            .Where(a => a.UserLogin == result.Principal.GetClaim(ClaimTypes.NameIdentifier))
            .SingleAsync();

        if (existing is null)
        {
            await db.InsertNav(
                    new GitHubOnlineAccount
                    {
                        UserLogin = result.Principal.GetClaim(ClaimTypes.NameIdentifier),
                        User = new User()
                        {
                            Name = result.Principal.GetClaim(ClaimTypes.Name) ?? "HackOMan",
                        },
                    }
                )
                .Include(a => a.User)
                .ExecuteCommandAsync();
        }

        await CookieAuth.SignInAsync(o =>
        {
            o.Claims.AddRange(result.Principal.Claims);
        });

        await Send.RedirectAsync(options.Value.FrontendUrl, allowRemoteRedirects: true);
    }
}
