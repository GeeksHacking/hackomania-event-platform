using HackOMania.Tests.Helpers;

namespace HackOMania.Tests;

public class SmokeTests
{
    [Test]
    public async Task CreateValidHackathonRequest_ProducesExpectedRequestShape()
    {
        const string testIdentifierSuffix = "ABC12345";
        var request = TestDataHelper.CreateValidHackathonRequest(testIdentifierSuffix);

        await Assert.That(request.Name).Contains(testIdentifierSuffix);
        await Assert.That(request.ShortCode).StartsWith("T");
        await Assert.That(request.ShortCode).Contains(testIdentifierSuffix);
        await Assert.That(request.EventStartDate).IsLessThan(request.EventEndDate);
        await Assert.That(request.SubmissionsStartDate).IsLessThan(request.SubmissionsEndDate);
        await Assert.That(request.JudgingStartDate).IsLessThan(request.JudgingEndDate);
    }
}
