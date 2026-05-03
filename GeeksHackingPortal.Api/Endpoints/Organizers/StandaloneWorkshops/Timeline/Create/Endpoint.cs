using FastEndpoints;
using GeeksHackingPortal.Api.Authorization;
using GeeksHackingPortal.Api.Entities;
using SqlSugar;

namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Timeline.Create;

public class Endpoint(ISqlSugarClient sql) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("organizers/standalone-workshops/{StandaloneWorkshopId:guid}/timeline");
        Policies(PolicyNames.OrganizerForActivity);
        Description(b => b.WithTags("Timeline"));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        if (req.EndTime <= req.StartTime)
        {
            AddError("EndTime must be after StartTime");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var exists = await sql.Queryable<StandaloneWorkshop>()
            .AnyAsync(w => w.Id == req.StandaloneWorkshopId, ct);
        if (!exists)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var item = new EventTimelineItem
        {
            Id = Guid.NewGuid(),
            ActivityId = req.StandaloneWorkshopId,
            Title = req.Title,
            Description = string.IsNullOrWhiteSpace(req.Description) ? string.Empty : req.Description,
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await sql.Insertable(item).ExecuteCommandAsync(ct);
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
