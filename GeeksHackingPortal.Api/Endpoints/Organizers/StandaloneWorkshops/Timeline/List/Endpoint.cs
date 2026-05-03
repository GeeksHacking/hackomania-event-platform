using FastEndpoints;
using GeeksHackingPortal.Api.Authorization;
using GeeksHackingPortal.Api.Entities;
using SqlSugar;

namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Timeline.List;

public class Endpoint(ISqlSugarClient sql) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("organizers/standalone-workshops/{StandaloneWorkshopId:guid}/timeline");
        Policies(PolicyNames.OrganizerForActivity);
        Description(b => b.WithTags("Timeline"));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var exists = await sql.Queryable<StandaloneWorkshop>()
            .AnyAsync(w => w.Id == req.StandaloneWorkshopId, ct);
        if (!exists)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var items = await sql.Queryable<EventTimelineItem>()
            .Where(t => t.ActivityId == req.StandaloneWorkshopId)
            .OrderBy(t => t.StartTime)
            .ToListAsync(ct);

        await Send.OkAsync(
            new Response { TimelineItems = [.. items.Select(ToDto)] },
            ct
        );
    }

    private static TimelineItemDto ToDto(EventTimelineItem item) =>
        new()
        {
            Id = item.Id,
            StandaloneWorkshopId = item.ActivityId,
            Title = item.Title,
            Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description,
            StartTime = item.StartTime,
            EndTime = item.EndTime,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
        };
}
