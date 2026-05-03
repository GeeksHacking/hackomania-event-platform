using FastEndpoints;
using GeeksHackingPortal.Api.Authorization;
using GeeksHackingPortal.Api.Entities;
using SqlSugar;

namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Timeline.Update;

public class Endpoint(ISqlSugarClient sql) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Patch("organizers/standalone-workshops/{StandaloneWorkshopId:guid}/timeline/{TimelineItemId:guid}");
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

        var startTime = req.StartTime ?? item.StartTime;
        var endTime = req.EndTime ?? item.EndTime;
        if (endTime <= startTime)
        {
            AddError("EndTime must be after StartTime");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        if (!string.IsNullOrWhiteSpace(req.Title))
            item.Title = req.Title;
        if (req.Description is not null)
            item.Description = string.IsNullOrWhiteSpace(req.Description) ? string.Empty : req.Description;
        item.StartTime = startTime;
        item.EndTime = endTime;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await sql.Updateable(item).ExecuteCommandAsync(ct);
        await Send.OkAsync(ToResponse(item), ct);
    }

    private static Response ToResponse(EventTimelineItem item) =>
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
