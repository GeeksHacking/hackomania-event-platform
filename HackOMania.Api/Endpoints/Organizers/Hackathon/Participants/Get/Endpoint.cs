using FastEndpoints;
using HackOMania.Api.Authorization;
using HackOMania.Api.Endpoints.Organizers.Hackathon.Participants.List;
using HackOMania.Api.Entities;
using SqlSugar;

namespace HackOMania.Api.Endpoints.Organizers.Hackathon.Participants.Get;

public class Endpoint(ISqlSugarClient sql) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("organizers/hackathons/{HackathonId:guid}/participants/{UserId:guid}");
        Policies(PolicyNames.OrganizerForHackathon);
        Description(b => b.WithTags("Organizers", "Participants"));
        Summary(s =>
        {
            s.Summary = "Get participant details";
            s.Description = "Retrieves full details for a participant, including registration responses.";
        });
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var hackathon = await sql.Queryable<Entities.Hackathon>().InSingleAsync(req.HackathonId);
        if (hackathon is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var participant = await sql.Queryable<Participant>()
            .Includes(p => p.ParticipantReviews)
            .Where(p => p.HackathonId == hackathon.Id && p.UserId == req.UserId)
            .FirstAsync(ct);

        if (participant is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var user = await sql.Queryable<User>().InSingleAsync(participant.UserId);
        
        string? teamName = null;
        if (participant.TeamId.HasValue)
        {
            var team = await sql.Queryable<Team>().InSingleAsync(participant.TeamId.Value);
            teamName = team?.Name;
        }

        var submissions = await sql.Queryable<ParticipantRegistrationSubmission>()
            .Includes(s => s.Question)
            .Where(s => s.ParticipantId == participant.Id)
            .ToListAsync(ct);

        var concludedStatus = participant.ConcludedStatus switch
        {
            ParticipantReview.ParticipantReviewStatus.Accepted =>
                ParticipantConcludedStatus.Accepted,
            ParticipantReview.ParticipantReviewStatus.Rejected =>
                ParticipantConcludedStatus.Rejected,
            _ => ParticipantConcludedStatus.Pending,
        };

        await Send.OkAsync(
            new Response
            {
                Id = participant.UserId,
                Name = user?.Name ?? "Unknown",
                TeamId = participant.TeamId,
                TeamName = teamName,
                ConcludedStatus = concludedStatus,
                Reviews =
                [
                    .. participant.ParticipantReviews.Select(r => new ParticipantReviewItem
                    {
                        Id = r.Id,
                        Status = r.Status switch
                        {
                            ParticipantReview.ParticipantReviewStatus.Accepted =>
                                ParticipantReviewItem.ParticipantReviewStatus.Accepted,
                            ParticipantReview.ParticipantReviewStatus.Rejected =>
                                ParticipantReviewItem.ParticipantReviewStatus.Rejected,
                            _ => throw new ArgumentOutOfRangeException(),
                        },
                        Reason = r.Reason,
                        CreatedAt = r.CreatedAt,
                    }),
                ],
                RegistrationSubmissions = submissions.Select(s => new RegistrationSubmissionItem
                {
                    QuestionId = s.QuestionId,
                    QuestionText = s.Question.QuestionText,
                    Value = s.Value,
                    FollowUpValue = s.FollowUpValue,
                }).ToList(),
            },
            ct
        );
    }
}
