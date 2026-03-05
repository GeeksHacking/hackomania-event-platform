using FastEndpoints;
using HackOMania.Api.Entities;
using HackOMania.Api.Extensions;
using SqlSugar;

namespace HackOMania.Api.Endpoints.Participants.Hackathon.Status;

public class Endpoint(ISqlSugarClient sql) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("participants/hackathons/{HackathonId:guid}/status");
        Description(b => b.WithTags("Participants"));
        Summary(s =>
        {
            s.Summary = "Get my participation status";
            s.Description =
                "Retrieves the current user's participation status for a hackathon, including team info and review status.";
        });
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var hackathon = await sql.Queryable<Entities.Hackathon>()
            .WithCache()
            .InSingleAsync(req.HackathonId);
        if (hackathon is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var currentUserId = User.GetUserId();

        var isOrganizer = await sql.Queryable<Organizer>()
            .WithCache()
            .AnyAsync(o => o.HackathonId == hackathon.Id && o.UserId == currentUserId, ct);

        var participant = await sql.Queryable<Participant>()
            .Where(p => p.HackathonId == hackathon.Id && p.UserId == currentUserId)
            .WithCache()
            .FirstAsync(ct);

        var isWithdrawn = participant is not null
            && await sql.Queryable<ParticipantWithdrawal>()
                .Where(w => w.ParticipantId == participant.Id && w.RejoinedAt == null)
                .WithCache()
                .AnyAsync(ct);

        if (participant is null || isWithdrawn)
        {
            await Send.OkAsync(
                new Response
                {
                    IsParticipant = false,
                    IsOrganizer = isOrganizer,
                    TeamId = null,
                    TeamName = null,
                    Status = null,
                    ReviewReason = null,
                    ReviewedAt = null,
                },
                ct
            );
            return;
        }

        string? teamName = null;

        if (participant.TeamId.HasValue)
        {
            var team = await sql.Queryable<Team>()
                .Where(t => t.Id == participant.TeamId.Value)
                .WithCache()
                .FirstAsync(ct);
            teamName = team?.Name;
        }

        var latestReview = await sql.Queryable<ParticipantReview>()
            .Where(r => r.ParticipantId == participant.Id)
            .OrderByDescending(r => r.CreatedAt)
            .WithCache()
            .FirstAsync(ct);

        // Participant must have a review newer than their latest withdrawal.
        var lastWithdrawalAt = await sql.Queryable<ParticipantWithdrawal>()
            .Where(w => w.ParticipantId == participant.Id)
            .OrderByDescending(w => w.WithdrawnAt)
            .Select(w => w.WithdrawnAt)
            .WithCache()
            .FirstAsync(ct);

        var needsReReview = lastWithdrawalAt != default
            && (latestReview is null || latestReview.CreatedAt < lastWithdrawalAt);

        var status = needsReReview
            ? ParticipantStatus.Pending
            : latestReview switch
            {
                { Status: ParticipantReview.ParticipantReviewStatus.Accepted } =>
                    ParticipantStatus.Accepted,
                { Status: ParticipantReview.ParticipantReviewStatus.Rejected } =>
                    ParticipantStatus.Rejected,
                _ => ParticipantStatus.Pending,
            };

        await Send.OkAsync(
            new Response
            {
                IsParticipant = true,
                IsOrganizer = isOrganizer,
                TeamId = participant.TeamId,
                TeamName = teamName,
                Status = status,
                ReviewReason = latestReview?.Reason,
                ReviewedAt = latestReview?.CreatedAt,
            },
            ct
        );
    }
}
