namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Participants.List;

public class Response
{
    public required int TotalCount { get; init; }
    public required int RegisteredCount { get; init; }
    public required int WithdrawnCount { get; init; }
    public required List<ParticipantItem> Participants { get; init; }
}

public class ParticipantItem
{
    public required Guid RegistrationId { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset RegisteredAt { get; init; }
    public DateTimeOffset? WithdrawnAt { get; init; }
    public required List<RegistrationSubmissionItem> RegistrationSubmissions { get; init; }
}

public class RegistrationSubmissionItem
{
    public required Guid QuestionId { get; init; }
    public required string QuestionText { get; init; }
    public required string Value { get; init; }
    public string? FollowUpValue { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
