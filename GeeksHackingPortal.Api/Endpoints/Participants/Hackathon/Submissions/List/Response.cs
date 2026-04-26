namespace GeeksHackingPortal.Api.Endpoints.Participants.Hackathon.Submissions.List;

public class Response
{
    public IEnumerable<ResponseSubmission> Submissions { get; set; } = [];

    public class ResponseSubmission
    {
        public Guid Id { get; set; }
        public Guid? ChallengeId { get; set; }
        public string Title { get; set; } = null!;
        public string? Summary { get; set; }
        public string? Location { get; set; }
        public Uri? DevpostUri { get; set; }
        public Uri? RepoUri { get; set; }
        public Uri? DemoUri { get; set; }
        public Uri? SlidesUri { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
    }
}
