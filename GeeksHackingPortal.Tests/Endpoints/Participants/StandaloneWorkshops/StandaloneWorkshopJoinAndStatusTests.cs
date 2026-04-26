using System.Net.Http.Json;
using GeeksHackingPortal.Tests.Data;
using GeeksHackingPortal.Tests.Helpers;
using GeeksHackingPortal.Tests.Models;

namespace GeeksHackingPortal.Tests.Endpoints.Participants.StandaloneWorkshops;

public class StandaloneWorkshopJoinAndStatusTests
{
    [ClassDataSource<AuthenticatedHttpClientDataClass>]
    public required AuthenticatedHttpClientDataClass Client { get; init; }

    [ClassDataSource<HttpClientDataClass>]
    public required HttpClientDataClass AnonymousClient { get; init; }

    [Test]
    public async Task JoinStandaloneWorkshop_WithValidWorkshop_ReturnsOk()
    {
        // Arrange
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(Client.HttpClient);

        // Act
        var response = await Client.HttpClient.PostAsync(
            $"/participants/standalone-workshops/{workshop.Id}/join",
            null
        );
        var result = await response.Content.ReadFromJsonAsync<StandaloneWorkshopJoinResponse>();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.StandaloneWorkshopId).IsEqualTo(workshop.Id);
    }

    [Test]
    public async Task JoinStandaloneWorkshop_AlreadyJoined_ReturnsOk()
    {
        // Arrange
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(Client.HttpClient);
        await Client.HttpClient.PostAsync(
            $"/participants/standalone-workshops/{workshop.Id}/join",
            null
        );

        // Act
        var response = await Client.HttpClient.PostAsync(
            $"/participants/standalone-workshops/{workshop.Id}/join",
            null
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task JoinStandaloneWorkshopByShortCode_WithValidShortCode_ReturnsOk()
    {
        // Arrange
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(Client.HttpClient);

        // Act
        var response = await Client.HttpClient.PostAsJsonAsync(
            "/participants/standalone-workshops/join",
            new { workshop.ShortCode }
        );
        var result = await response.Content.ReadFromJsonAsync<StandaloneWorkshopJoinResponse>();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.StandaloneWorkshopId).IsEqualTo(workshop.Id);
    }

    [Test]
    public async Task JoinStandaloneWorkshop_WithUnpublishedWorkshop_ReturnsNotFound()
    {
        // Arrange
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(
            Client.HttpClient,
            isPublished: false
        );

        // Act
        var response = await Client.HttpClient.PostAsync(
            $"/participants/standalone-workshops/{workshop.Id}/join",
            null
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetStandaloneWorkshopStatus_AfterJoin_ReturnsRegistered()
    {
        // Arrange
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(Client.HttpClient);
        await Client.HttpClient.PostAsync(
            $"/participants/standalone-workshops/{workshop.Id}/join",
            null
        );

        // Act
        var response = await Client.HttpClient.GetAsync(
            $"/participants/standalone-workshops/{workshop.Id}/status"
        );
        var status = await response.Content.ReadFromJsonAsync<StandaloneWorkshopStatusResponse>();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(status).IsNotNull();
        await Assert.That(status!.IsRegistered).IsTrue();
        await Assert.That(status.RegisteredAt).IsNotNull();
        await Assert.That(status.WithdrawnAt).IsNull();
    }

    [Test]
    public async Task WithdrawStandaloneWorkshop_AfterJoin_ReturnsWithdrawnStatus()
    {
        // Arrange
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(Client.HttpClient);
        await Client.HttpClient.PostAsync(
            $"/participants/standalone-workshops/{workshop.Id}/join",
            null
        );

        // Act
        var withdrawResponse = await Client.HttpClient.PostAsync(
            $"/participants/standalone-workshops/{workshop.Id}/withdraw",
            null
        );
        var statusResponse = await Client.HttpClient.GetAsync(
            $"/participants/standalone-workshops/{workshop.Id}/status"
        );
        var status = await statusResponse.Content.ReadFromJsonAsync<StandaloneWorkshopStatusResponse>();

        // Assert
        await Assert.That(withdrawResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(statusResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(status).IsNotNull();
        await Assert.That(status!.IsRegistered).IsFalse();
        await Assert.That(status.WithdrawnAt).IsNotNull();
    }

    [Test]
    public async Task JoinStandaloneWorkshop_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await AnonymousClient.HttpClient.PostAsync(
            $"/participants/standalone-workshops/{Guid.NewGuid()}/join",
            null
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }
}
