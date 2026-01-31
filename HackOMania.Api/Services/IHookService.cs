using HackOMania.Api.Entities;

namespace HackOMania.Api.Services;

public interface IHookService
{
    /// <summary>
    /// Trigger hook when a participant is reviewed (accepted or rejected)
    /// </summary>
    Task OnParticipantReviewedAsync(
        Participant participant,
        User user,
        Entities.Hackathon hackathon,
        ParticipantReview.ParticipantReviewStatus status,
        string? reason = null,
        CancellationToken ct = default
    );
}
