using FastEndpoints;
using GeeksHackingPortal.Api.Authorization;
using GeeksHackingPortal.Api.Entities;
using SqlSugar;

namespace GeeksHackingPortal.Api.Endpoints.Organizers.Hackathon.Organizers.Invites.Revoke;

public class Endpoint(ISqlSugarClient sql) : Endpoint<Request>
{
    public override void Configure()
    {
        Delete("organizers/hackathons/{HackathonId:guid}/organizers/invites/{InviteId:guid}");
        Policies(PolicyNames.OrganizerForHackathon);
        Description(b => b.WithTags("Activity Organizers"));
        Summary(s =>
        {
            s.Summary = "Revoke a hackathon organizer invite code";
            s.Description = "Expires an organizer invite code immediately so it can no longer be redeemed.";
        });
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var updated = await sql.Updateable<ActivityOrganizerInvite>()
            .SetColumns(i => i.ExpiresAt == DateTimeOffset.UtcNow)
            .Where(i => i.Id == req.InviteId && i.ActivityId == req.HackathonId)
            .ExecuteCommandAsync(ct);

        if (updated == 0)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
