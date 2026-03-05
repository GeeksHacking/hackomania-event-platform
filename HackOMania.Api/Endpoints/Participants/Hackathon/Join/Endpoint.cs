using FastEndpoints;
using HackOMania.Api.Entities;
using HackOMania.Api.Extensions;
using SqlSugar;

namespace HackOMania.Api.Endpoints.Participants.Hackathon.Join;

public class Endpoint(ISqlSugarClient sql, IWebHostEnvironment env) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("participants/hackathons/{HackathonId:guid}/join");
        Description(b => b.WithTags("Participants", "Hackathons").Accepts<Request>());
        Summary(s =>
        {
            s.Summary = "Join a hackathon";
            s.Description = "Registers the current user as a participant in the hackathon.";
        });

        // The join endpoint is hit heavily by the integration test suite.
        // Throttling is valuable in production, but it makes parallel tests flaky.
        if (env.IsProduction())
        {
            Throttle(hitLimit: 10, durationSeconds: 60);
        }
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            throw new ArgumentNullException(nameof(userId));
        }

        var hackathon = await sql.Queryable<Entities.Hackathon>().InSingleAsync(req.HackathonId);
        if (hackathon is null || !hackathon.IsPublished)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var existing = await sql.Queryable<Participant>()
            .Where(p => p.HackathonId == hackathon.Id && p.UserId == userId.Value)
            .FirstAsync(ct);

        if (existing is null)
        {
            existing = new Participant
            {
                HackathonId = hackathon.Id,
                UserId = userId.Value,
                TeamId = null,
                JoinedAt = DateTimeOffset.UtcNow,
            };

            await sql.Insertable(existing).ExecuteCommandAsync(ct);
        }
        else
        {
            var openWithdrawal = await sql.Queryable<ParticipantWithdrawal>()
                .Where(w => w.ParticipantId == existing.Id && w.RejoinedAt == null)
                .OrderByDescending(w => w.WithdrawnAt)
                .FirstAsync(ct);

            if (openWithdrawal is not null)
            {
                var latestRejoinReview = await sql.Queryable<ParticipantReview>()
                    .Where(r => r.ParticipantId == existing.Id && r.CreatedAt > openWithdrawal.WithdrawnAt)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstAsync(ct);

                if (latestRejoinReview?.Status != ParticipantReview.ParticipantReviewStatus.Accepted)
                {
                    AddError("You must be accepted in a new review before re-joining this hackathon.");
                    await Send.ErrorsAsync(cancellation: ct);
                    return;
                }

                // Close the open withdrawal record to re-activate the participant
                var now = DateTimeOffset.UtcNow;
                await sql.Updateable<ParticipantWithdrawal>()
                    .SetColumns(w => w.RejoinedAt == now, true)
                    .Where(w => w.ParticipantId == existing.Id && w.RejoinedAt == null)
                    .ExecuteCommandAsync(ct);
            }
        }

        await Send.OkAsync(
            new Response
            {
                HackathonId = hackathon.Id,
                UserId = userId.Value,
                JoinedAt = existing.JoinedAt,
            },
            ct
        );
    }
}
