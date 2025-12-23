using Pulumi;
using Pulumi.Gcp.Storage;

namespace HackOMania.Infra.Stacks;

public class BootstrapStack : Stack
{
    public BootstrapStack()
    {
        var bucket = new Bucket(
            "hackomania-infra",
            new BucketArgs
            {
                Name = "hackomania-infra",
                Location = "ASIA-SOUTHEAST1",
                ForceDestroy = true,
            }
        );

        BucketUrl = bucket.Url;
    }

    [Output]
    public Output<string> BucketUrl { get; set; }
}
