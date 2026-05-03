using FastEndpoints;
using GeeksHackingPortal.Api.Authorization;
using GeeksHackingPortal.Api.Entities;
using SqlSugar;

namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Participants.Get;

public class Endpoint(ISqlSugarClient sql) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("organizers/standalone-workshops/{StandaloneWorkshopId:guid}/participants/{UserId:guid}");
        Policies(PolicyNames.OrganizerForActivity);
        Description(b => b.WithTags("Activity Participants"));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var registration = await sql.Queryable<ActivityRegistration>()
            .Where(r => r.ActivityId == req.StandaloneWorkshopId && r.UserId == req.UserId)
            .FirstAsync(ct);
        if (registration is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var user = await sql.Queryable<User>().InSingleAsync(registration.UserId);
        var submissions = await sql.Queryable<ParticipantRegistrationSubmission>()
            .LeftJoin<RegistrationQuestion>((s, q) => s.QuestionId == q.Id)
            .Where((s, q) => s.ActivityRegistrationId == registration.Id)
            .Select(
                (s, q) =>
                    new RegistrationSubmissionItem
                    {
                        QuestionId = s.QuestionId,
                        QuestionText = q.QuestionText,
                        Value = s.Value,
                        FollowUpValue = s.FollowUpValue,
                        UpdatedAt = s.UpdatedAt,
                    }
            )
            .ToListAsync(ct);
        var checkIns = await sql.Queryable<VenueCheckIn>()
            .Where(c => c.ActivityRegistrationId == registration.Id)
            .OrderByDescending(c => c.CheckInTime)
            .ToListAsync(ct);

        await Send.OkAsync(
            new Response
            {
                RegistrationId = registration.Id,
                UserId = registration.UserId,
                Name = user?.Name ?? "Unknown",
                Email = user?.Email ?? string.Empty,
                Status = registration.Status.ToString(),
                RegisteredAt = registration.RegisteredAt,
                WithdrawnAt = registration.WithdrawnAt,
                RegistrationSubmissions = submissions,
                VenueCheckIns =
                [
                    .. checkIns.Select(c => new VenueCheckInItem
                    {
                        Id = c.Id,
                        CheckInTime = c.CheckInTime,
                        CheckOutTime = c.CheckOutTime,
                        IsCheckedIn = c.IsCheckedIn,
                    }),
                ],
            },
            ct
        );
    }
}
