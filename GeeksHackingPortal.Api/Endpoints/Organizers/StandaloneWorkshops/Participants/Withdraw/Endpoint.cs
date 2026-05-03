using FastEndpoints;
using GeeksHackingPortal.Api.Authorization;
using GeeksHackingPortal.Api.Entities;
using SqlSugar;

namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Participants.Withdraw;

public class Endpoint(ISqlSugarClient sql) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("organizers/standalone-workshops/{StandaloneWorkshopId:guid}/participants/{UserId:guid}/withdraw");
        Policies(PolicyNames.OrganizerForActivity);
        Description(b => b.WithTags("Activity Participants"));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var registration = await sql.Queryable<ActivityRegistration>()
            .InnerJoin<Activity>((r, a) => r.ActivityId == a.Id)
            .Where((r, a) =>
                r.ActivityId == req.StandaloneWorkshopId
                && r.UserId == req.UserId
                && r.Status == ActivityRegistrationStatus.Registered
            )
            .Select((r, a) => r)
            .FirstAsync(ct);
        if (registration is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        registration.Status = ActivityRegistrationStatus.Withdrawn;
        registration.WithdrawnAt = DateTimeOffset.UtcNow;
        await sql.Updateable(registration).ExecuteCommandAsync(ct);

        await Send.OkAsync(
            new Response
            {
                Message = "Participant has been withdrawn from the standalone workshop",
            },
            ct
        );
    }
}
