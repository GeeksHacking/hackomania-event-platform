namespace GeeksHackingPortal.Api.Endpoints.Organizers.StandaloneWorkshops.Participants.Get;

public class Response
{
    public required Guid RegistrationId { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset RegisteredAt { get; init; }
    public DateTimeOffset? WithdrawnAt { get; init; }
    public required List<RegistrationSubmissionItem> RegistrationSubmissions { get; init; }
    public required List<VenueCheckInItem> VenueCheckIns { get; init; }
}

public class RegistrationSubmissionItem
{
    public required Guid QuestionId { get; init; }
    public required string QuestionText { get; init; }
    public required string Value { get; init; }
    public string? FollowUpValue { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}

public class VenueCheckInItem
{
    public required Guid Id { get; init; }
    public required DateTimeOffset CheckInTime { get; init; }
    public DateTimeOffset? CheckOutTime { get; init; }
    public required bool IsCheckedIn { get; init; }
}
