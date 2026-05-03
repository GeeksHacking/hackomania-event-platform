using FastEndpoints;
using GeeksHackingPortal.Api.Authorization;
using GeeksHackingPortal.Api.Entities;
using SqlSugar;

namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Timeline.Delete;

public class Endpoint(ISqlSugarClient sql) : Endpoint<Request>
{
    public override void Configure()
    {
        Delete("organizers/standalone-workshops/{StandaloneWorkshopId:guid}/timeline/{TimelineItemId:guid}");
        Policies(PolicyNames.OrganizerForActivity);
        Description(b => b.WithTags("Timeline"));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var item = await sql.Queryable<EventTimelineItem>()
            .Where(t => t.Id == req.TimelineItemId && t.ActivityId == req.StandaloneWorkshopId)
            .FirstAsync(ct);
        if (item is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await sql.Deleteable(item).ExecuteCommandAsync(ct);
        await Send.NoContentAsync(ct);
    }
}
