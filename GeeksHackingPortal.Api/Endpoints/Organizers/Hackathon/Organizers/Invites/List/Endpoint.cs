using FastEndpoints;
using GeeksHackingPortal.Api.Authorization;
using GeeksHackingPortal.Api.Entities;
using SqlSugar;

namespace GeeksHackingPortal.Api.Endpoints.Organizers.Hackathon.Organizers.Invites.List;

public class Endpoint(ISqlSugarClient sql) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("organizers/hackathons/{HackathonId:guid}/organizers/invites");
        Policies(PolicyNames.OrganizerForHackathon);
        Description(b => b.WithTags("Activity Organizers"));
        Summary(s =>
        {
            s.Summary = "List organizer invite codes for a hackathon";
            s.Description = "Retrieves organizer invite codes created for the hackathon with usage and expiry status.";
        });
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var exists = await sql.Queryable<Entities.Hackathon>()
            .AnyAsync(h => h.Id == req.HackathonId, ct);
        if (!exists)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var invites = await sql.Queryable<ActivityOrganizerInvite>()
            .LeftJoin<ActivityOrganizerInviteUse>((invite, use) => invite.Id == use.InviteId)
            .Where(invite => invite.ActivityId == req.HackathonId)
            .GroupBy(invite => new { invite.Id, invite.Code, invite.Type, invite.CreatedAt, invite.ExpiresAt, invite.MaxUses })
            .OrderBy(invite => invite.CreatedAt, OrderByType.Desc)
            .Select((invite, use) => new Response.InviteItem
            {
                Id = invite.Id,
                Code = invite.Code,
                Type = invite.Type,
                CreatedAt = invite.CreatedAt,
                ExpiresAt = invite.ExpiresAt,
                MaxUses = invite.MaxUses,
                UseCount = SqlFunc.AggregateCount(use.Id),
            })
            .ToListAsync(ct);

        foreach (var invite in invites)
        {
            invite.IsExpired = invite.ExpiresAt is not null && invite.ExpiresAt <= now;
            invite.IsExhausted = invite.MaxUses is not null && invite.UseCount >= invite.MaxUses;
        }

        await Send.OkAsync(new Response { Invites = invites }, ct);
    }
}
