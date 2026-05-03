using FastEndpoints;
using GeeksHackingPortal.Api.Authorization;
using GeeksHackingPortal.Api.Entities;
using SqlSugar;

namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Participants.List;

public class Endpoint(ISqlSugarClient sql) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("organizers/standalone-workshops/{StandaloneWorkshopId:guid}/participants");
        Policies(PolicyNames.OrganizerForActivity);
        Description(b => b.WithTags("Activity Participants"));
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

        var registrations = await sql.Queryable<ActivityRegistration>()
            .Where(r => r.ActivityId == req.StandaloneWorkshopId)
            .OrderByDescending(r => r.RegisteredAt)
            .ToListAsync(ct);

        var participants = await BuildParticipantsAsync(registrations, ct);
        await Send.OkAsync(
            new Response
            {
                TotalCount = registrations.Count,
                RegisteredCount = registrations.Count(r => r.Status == ActivityRegistrationStatus.Registered),
                WithdrawnCount = registrations.Count(r => r.Status == ActivityRegistrationStatus.Withdrawn),
                Participants = participants,
            },
            ct
        );
    }

    private async Task<List<ParticipantItem>> BuildParticipantsAsync(
        List<ActivityRegistration> registrations,
        CancellationToken ct
    )
    {
        if (registrations.Count == 0)
        {
            return [];
        }

        var registrationIds = registrations.Select(r => r.Id).ToList();
        var userIds = registrations.Select(r => r.UserId).Distinct().ToList();
        var users = (await sql.Queryable<User>().Where(u => userIds.Contains(u.Id)).ToListAsync(ct))
            .ToDictionary(u => u.Id, u => u);
        var submissions = await sql.Queryable<ParticipantRegistrationSubmission>()
            .LeftJoin<RegistrationQuestion>((s, q) => s.QuestionId == q.Id)
            .Where((s, q) => registrationIds.Contains(s.ActivityRegistrationId))
            .Select(
                (s, q) =>
                    new
                    {
                        s.ActivityRegistrationId,
                        Item = new RegistrationSubmissionItem
                        {
                            QuestionId = s.QuestionId,
                            QuestionText = q.QuestionText,
                            Value = s.Value,
                            FollowUpValue = s.FollowUpValue,
                            UpdatedAt = s.UpdatedAt,
                        },
                    }
            )
            .ToListAsync(ct);
        var submissionsByRegistration = submissions
            .GroupBy(s => s.ActivityRegistrationId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Item).ToList());

        return
        [
            .. registrations.Select(r =>
            {
                var user = users.GetValueOrDefault(r.UserId);
                return new ParticipantItem
                {
                    RegistrationId = r.Id,
                    UserId = r.UserId,
                    Name = user?.Name ?? "Unknown",
                    Email = user?.Email ?? string.Empty,
                    Status = r.Status.ToString(),
                    RegisteredAt = r.RegisteredAt,
                    WithdrawnAt = r.WithdrawnAt,
                    RegistrationSubmissions = submissionsByRegistration.GetValueOrDefault(r.Id) ?? [],
                };
            }),
        ];
    }
}
