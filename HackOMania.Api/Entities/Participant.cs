using SqlSugar;

namespace HackOMania.Api.Entities;

public class Participant : HackathonUser
{
    public Guid TeamId { get; set; }

    [Navigate(NavigateType.ManyToOne, nameof(TeamId), nameof(Team.Id))]
    public Team Team { get; set; } = null!;

    [Navigate(NavigateType.OneToMany, nameof(ParticipantReview.ParticipantId))]
    public List<ParticipantReview> ParticipantReviews { get; set; } = null!;
}
