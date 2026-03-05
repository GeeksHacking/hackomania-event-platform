using FastEndpoints;
using HackOMania.Api.Entities;
using HackOMania.Api.Extensions;
using SqlSugar;

namespace HackOMania.Api.Endpoints.Participants.Teams.JoinByCode;

public class Endpoint(ISqlSugarClient sql) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("participants/teams/join");
        Description(b => b.WithTags("Participants", "Teams"));
        Summary(s =>
        {
            s.Summary = "Join a team by join code";
            s.Description =
                "Joins the current user to a team using only the team's join code. If the user is not already a participant in the hackathon, they will be automatically registered.";
        });
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            throw new ArgumentNullException(nameof(userId));
        }

        // Find the team by join code
        var team = await sql.Queryable<Team>()
            .Where(t => t.JoinCode == req.JoinCode)
            .WithCache()
            .FirstAsync(ct);

        if (team is null)
        {
            AddError(r => r.JoinCode, "Invalid join code.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        // Get the hackathon for this team
        var hackathon = await sql.Queryable<Entities.Hackathon>()
            .WithCache()
            .InSingleAsync(team.HackathonId);

        if (hackathon is null || !hackathon.IsPublished)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Check if user is already a participant in this hackathon
        var participant = await sql.Queryable<Participant>()
            .Where(p => p.HackathonId == hackathon.Id && p.UserId == userId.Value)
            .WithCache()
            .FirstAsync(ct);

        var autoJoinedHackathon = false;

        if (participant is null)
        {
            // Auto-register the user as a participant in the hackathon
            participant = new Participant
            {
                HackathonId = hackathon.Id,
                UserId = userId.Value,
                TeamId = team.Id,
                JoinedAt = DateTimeOffset.UtcNow,
            };

            await sql.Insertable(participant).ExecuteCommandAsync(ct);
            autoJoinedHackathon = true;
        }
        else
        {
            var openWithdrawal = await sql.Queryable<ParticipantWithdrawal>()
                .Where(w => w.ParticipantId == participant.Id && w.RejoinedAt == null)
                .OrderByDescending(w => w.WithdrawnAt)
                .FirstAsync(ct);

            if (openWithdrawal is not null)
            {
                var latestRejoinReview = await sql.Queryable<ParticipantReview>()
                    .Where(r =>
                        r.ParticipantId == participant.Id && r.CreatedAt > openWithdrawal.WithdrawnAt
                    )
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstAsync(ct);

                if (latestRejoinReview?.Status != ParticipantReview.ParticipantReviewStatus.Accepted)
                {
                    AddError(r => r.JoinCode, "You must be accepted in a new review before re-joining this hackathon.");
                    await Send.ErrorsAsync(cancellation: ct);
                    return;
                }

                // Close the open withdrawal record to re-activate the participant
                var now = DateTimeOffset.UtcNow;
                await sql.Updateable<ParticipantWithdrawal>()
                    .SetColumns(w => w.RejoinedAt == now)
                    .Where(w => w.ParticipantId == participant.Id && w.RejoinedAt == null)
                    .ExecuteCommandAsync(ct);

                autoJoinedHackathon = true;
            }

            // Check if they're already in a team
            if (participant.TeamId.HasValue)
            {
                AddError(r => r.JoinCode, "You are already in a team for this hackathon.");
                await Send.ErrorsAsync(cancellation: ct);
                return;
            }

            // Assign the participant to the team
            participant.TeamId = team.Id;
            await sql.Updateable(participant)
                .UpdateColumns(p => p.TeamId)
                .ExecuteCommandAsync(ct);
        }

        await Send.OkAsync(
            new Response
            {
                TeamId = team.Id,
                HackathonId = hackathon.Id,
                AutoJoinedHackathon = autoJoinedHackathon,
            },
            ct
        );
    }
}
