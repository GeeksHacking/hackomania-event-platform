using FastEndpoints;
using FluentValidation;

namespace HackOMania.Api.Endpoints.Organizers.Hackathon.Participants.Get;

public class Request
{
    public Guid HackathonId { get; set; }
    public Guid UserId { get; set; }
}

public class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.HackathonId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
