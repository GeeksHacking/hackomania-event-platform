using GeeksHackingPortal.Api.Entities;

namespace GeeksHackingPortal.Api.Endpoints.Organizers.Hackathon.Organizers.Invites.List;

public class Response
{
    public IEnumerable<InviteItem> Invites { get; set; } = [];

    public class InviteItem
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public OrganizerType Type { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public int? MaxUses { get; set; }
        public int UseCount { get; set; }
        public bool IsExpired { get; set; }
        public bool IsExhausted { get; set; }
    }
}
