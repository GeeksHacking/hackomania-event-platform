using FastEndpoints;
using SqlSugar;

namespace GeeksHackingPortal.Api.Endpoints.Participants.Hackathon.List;

public class Endpoint(ISqlSugarClient sql) : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("participants/hackathons");
        AllowAnonymous();
        Description(b => b.WithTags("Participants", "Hackathons"));
        Summary(s =>
        {
            s.Summary = "List all hackathons";
            s.Description = "Retrieves all published hackathons.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var hackathons = await sql.Queryable<Entities.Hackathon>()
            .Includes(h => h.Activity)
            .WithCache()
            .ToListAsync(ct);

        await Send.OkAsync(
            new Response
            {
                Hackathons = hackathons
                    .Where(h => h.Activity.IsPublished)
                    .Select(h => new Response.HackathonItem
                    {
                        Id = h.Id,
                        Name = h.Activity.Title,
                        Description = h.Activity.Description,
                        Venue = h.Activity.Location,
                        HomepageUri = h.HomepageUri,
                        ShortCode = h.ShortCode,
                        IsPublished = h.Activity.IsPublished,
                        EventStartDate = h.Activity.StartTime,
                        EventEndDate = h.Activity.EndTime,
                        SubmissionsStartDate = h.SubmissionsStartDate,
                        ChallengeSelectionEndDate = h.ChallengeSelectionEndDate,
                        SubmissionsEndDate = h.SubmissionsEndDate,
                        JudgingStartDate = h.JudgingStartDate,
                        JudgingEndDate = h.JudgingEndDate,
                    }),
            },
            ct
        );
    }
}
