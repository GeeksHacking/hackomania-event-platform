using FastEndpoints;
using GeeksHackingPortal.Api.Authorization;
using GeeksHackingPortal.Api.Entities;
using SqlSugar;

namespace GeeksHackingPortal.Api.Endpoints.Organizers.Hackathon.Resources.Delete;

public class Endpoint(ISqlSugarClient sql) : Endpoint<Request>
{
    public override void Configure()
    {
        Delete("organizers/activities/{ActivityId:guid}/resources/{ResourceId}");
        Policies(PolicyNames.OrganizerForActivity);
        Description(b => b.WithTags("Organizers", "Resources"));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var activityExists = await sql.Queryable<Activity>().AnyAsync(a => a.Id == req.ActivityId, ct);
        if (!activityExists)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var deleted = await sql.Deleteable<Resource>()
            .Where(r => r.Id.ToString() == req.ResourceId && r.ActivityId == req.ActivityId)
            .ExecuteCommandAsync(ct);

        if (deleted == 0)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
