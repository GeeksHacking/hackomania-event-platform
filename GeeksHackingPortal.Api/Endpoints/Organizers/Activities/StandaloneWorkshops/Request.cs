namespace GeeksHackingPortal.Api.Endpoints.Organizers.Activities.StandaloneWorkshops;

public class Request
{
    public Guid StandaloneWorkshopId { get; set; }
    private Uri? _homepageUri;

    public Uri? HomepageUri
    {
        get => _homepageUri;
        set
        {
            _homepageUri = value;
            HasHomepageUri = true;
        }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public bool HasHomepageUri { get; private set; }

    public string? ShortCode { get; set; }
    public int? MaxParticipants { get; set; }
}
