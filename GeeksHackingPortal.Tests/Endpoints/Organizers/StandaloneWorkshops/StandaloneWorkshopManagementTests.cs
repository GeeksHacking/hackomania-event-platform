using System.Net.Http.Json;
using GeeksHackingPortal.Tests.Data;
using GeeksHackingPortal.Tests.Helpers;
using GeeksHackingPortal.Tests.Models;

namespace GeeksHackingPortal.Tests.Endpoints.Organizers.StandaloneWorkshops;

public class StandaloneWorkshopManagementTests
{
    [ClassDataSource<AuthenticatedHttpClientDataClass>]
    public required AuthenticatedHttpClientDataClass Client { get; init; }

    [Test]
    public async Task RegistrationQuestionCrud_ForStandaloneWorkshop_RoundTrips()
    {
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(Client.HttpClient);
        var suffix = Guid.NewGuid().ToString()[..8];

        var createResponse = await Client.HttpClient.PostAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/registration/questions",
            new
            {
                QuestionText = "What should we know?",
                QuestionKey = $"standalone_question_{suffix}",
                Type = "Text",
                DisplayOrder = 1,
                IsRequired = true,
            }
        );
        var created = await createResponse.Content.ReadFromJsonAsync<RegistrationQuestionResponse>();

        await Assert.That(createResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);
        await Assert.That(created).IsNotNull();

        var updateResponse = await Client.HttpClient.PatchAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/registration/questions/{created!.Id}",
            new { QuestionText = "Updated standalone question", IsRequired = false }
        );
        var updated = await updateResponse.Content.ReadFromJsonAsync<RegistrationQuestionResponse>();

        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(updated!.QuestionText).IsEqualTo("Updated standalone question");
        await Assert.That(updated.IsRequired).IsFalse();

        var listResponse = await Client.HttpClient.GetAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/registration/questions"
        );
        var list = await listResponse.Content.ReadFromJsonAsync<RegistrationQuestionListResponse>();

        await Assert.That(listResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(list!.Questions.Any(q => q.Id == created.Id)).IsTrue();

        var deleteResponse = await Client.HttpClient.DeleteAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/registration/questions/{created.Id}"
        );

        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task RegistrationQuestions_ForStandaloneWorkshop_ReorderPersists()
    {
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(Client.HttpClient);
        var suffix = Guid.NewGuid().ToString()[..8];

        var firstResponse = await Client.HttpClient.PostAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/registration/questions",
            new
            {
                QuestionText = "First question",
                QuestionKey = $"standalone_reorder_first_{suffix}",
                Type = "Text",
                DisplayOrder = 0,
                IsRequired = false,
            }
        );
        var first = await firstResponse.Content.ReadFromJsonAsync<RegistrationQuestionResponse>();

        var secondResponse = await Client.HttpClient.PostAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/registration/questions",
            new
            {
                QuestionText = "Second question",
                QuestionKey = $"standalone_reorder_second_{suffix}",
                Type = "Text",
                DisplayOrder = 1,
                IsRequired = false,
            }
        );
        var second = await secondResponse.Content.ReadFromJsonAsync<RegistrationQuestionResponse>();

        await Assert.That(firstResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);
        await Assert.That(secondResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);
        await Assert.That(first).IsNotNull();
        await Assert.That(second).IsNotNull();

        var moveFirstDownResponse = await Client.HttpClient.PatchAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/registration/questions/{first!.Id}",
            new { DisplayOrder = 1 }
        );
        var moveSecondUpResponse = await Client.HttpClient.PatchAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/registration/questions/{second!.Id}",
            new { DisplayOrder = 0 }
        );

        var listResponse = await Client.HttpClient.GetAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/registration/questions"
        );
        var list = await listResponse.Content.ReadFromJsonAsync<RegistrationQuestionListResponse>();
        var ordered = list!.Questions.OrderBy(question => question.DisplayOrder).ToList();

        await Assert.That(moveFirstDownResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(moveSecondUpResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(listResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(ordered[0].Id).IsEqualTo(second.Id);
        await Assert.That(ordered[1].Id).IsEqualTo(first.Id);
    }

    [Test]
    public async Task TimelineCrud_ForStandaloneWorkshop_RoundTrips()
    {
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(Client.HttpClient);
        var start = DateTimeOffset.UtcNow.AddDays(2);

        var createResponse = await Client.HttpClient.PostAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/timeline",
            new
            {
                Title = "Doors Open",
                Description = "Registration starts",
                StartTime = start,
                EndTime = start.AddHours(1),
            }
        );
        var created = await createResponse.Content.ReadFromJsonAsync<TimelineItemResponse>();

        await Assert.That(createResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(created).IsNotNull();

        var updateResponse = await Client.HttpClient.PatchAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/timeline/{created!.Id}",
            new { Title = "Updated Doors Open" }
        );
        var updated = await updateResponse.Content.ReadFromJsonAsync<TimelineItemResponse>();

        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(updated!.Title).IsEqualTo("Updated Doors Open");

        var listResponse = await Client.HttpClient.GetAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/timeline"
        );
        var list = await listResponse.Content.ReadFromJsonAsync<TimelineListResponse>();

        await Assert.That(listResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(list!.TimelineItems.Any(i => i.Id == created.Id)).IsTrue();

        var deleteResponse = await Client.HttpClient.DeleteAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/timeline/{created.Id}"
        );

        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task CreateTimelineItem_ForStandaloneWorkshop_WhenEndBeforeStart_ReturnsBadRequest()
    {
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(Client.HttpClient);
        var start = DateTimeOffset.UtcNow.AddDays(2);

        var response = await Client.HttpClient.PostAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/timeline",
            new
            {
                Title = "Invalid item",
                StartTime = start,
                EndTime = start.AddMinutes(-1),
            }
        );

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Participants_ForStandaloneWorkshop_ReturnsJoinedRegistration()
    {
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(Client.HttpClient);
        var joinResponse = await Client.HttpClient.PostAsync(
            $"/participants/standalone-workshops/{workshop.Id}/join",
            null
        );

        await Assert.That(joinResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var listResponse = await Client.HttpClient.GetAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/participants"
        );
        var list = await listResponse.Content.ReadFromJsonAsync<StandaloneParticipantListResponse>();

        await Assert.That(listResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(list!.RegisteredCount).IsEqualTo(1);
        await Assert.That(list.Participants).Count().IsEqualTo(1);

        var participant = list.Participants.Single();
        var getResponse = await Client.HttpClient.GetAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/participants/{participant.UserId}"
        );
        var detail = await getResponse.Content.ReadFromJsonAsync<StandaloneParticipantResponse>();

        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(detail!.RegistrationId).IsEqualTo(participant.RegistrationId);

        var withdrawResponse = await Client.HttpClient.PostAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/participants/{participant.UserId}/withdraw",
            new { }
        );

        await Assert.That(withdrawResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Resources_ForStandaloneWorkshop_RedeemAndOverviewRoundTrip()
    {
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(Client.HttpClient);
        var joinResponse = await Client.HttpClient.PostAsync(
            $"/participants/standalone-workshops/{workshop.Id}/join",
            null
        );
        var participantListResponse = await Client.HttpClient.GetAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/participants"
        );
        var participants =
            await participantListResponse.Content.ReadFromJsonAsync<StandaloneParticipantListResponse>();
        var participant = participants!.Participants.Single();

        var createResourceResponse = await Client.HttpClient.PostAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/resources",
            new
            {
                Name = "Sticker Pack",
                Description = "A resource for checked-in workshop attendees",
                RedemptionStmt = "return participantRedemptions === 0;",
                IsPublished = true,
            }
        );
        var resource = await createResourceResponse.Content.ReadFromJsonAsync<ResourceResponse>();

        var redeemResponse = await Client.HttpClient.PostAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/participants/{participant.UserId}/resources/{resource!.Id}/redemptions",
            new { }
        );
        var redemption = await redeemResponse.Content.ReadFromJsonAsync<ResourceRedemptionResponse>();

        var secondRedeemResponse = await Client.HttpClient.PostAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/participants/{participant.UserId}/resources/{resource.Id}/redemptions",
            new { }
        );

        var overviewResponse = await Client.HttpClient.GetAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/resources/{resource.Id}/overview"
        );
        var overview =
            await overviewResponse.Content.ReadFromJsonAsync<OrganizerResourceOverviewResponse>();

        await Assert.That(joinResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(participantListResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(createResourceResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(resource).IsNotNull();
        await Assert.That(redeemResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(redemption!.ActivityId).IsEqualTo(workshop.Id);
        await Assert.That(secondRedeemResponse.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(overviewResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(overview!.TotalRedemptions).IsEqualTo(1);
        await Assert.That(overview.Participants!.Single(p => p.UserId == participant.UserId).HasRedeemed)
            .IsTrue();
    }

    [Test]
    public async Task RedeemResource_ForStandaloneWorkshop_WhenParticipantNotRegistered_ReturnsNotFound()
    {
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(Client.HttpClient);
        var createResourceResponse = await Client.HttpClient.PostAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/resources",
            new
            {
                Name = "Unregistered Resource",
                RedemptionStmt = "return true;",
                IsPublished = true,
            }
        );
        var resource = await createResourceResponse.Content.ReadFromJsonAsync<ResourceResponse>();

        var response = await Client.HttpClient.PostAsJsonAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/participants/{Guid.NewGuid()}/resources/{resource!.Id}/redemptions",
            new { }
        );

        await Assert.That(createResourceResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Organizers_ForStandaloneWorkshop_ListsCreator()
    {
        var workshop = await TestDataHelper.CreateStandaloneWorkshopAsync(Client.HttpClient);

        var response = await Client.HttpClient.GetAsync(
            $"/organizers/standalone-workshops/{workshop.Id}/organizers"
        );
        var result = await response.Content.ReadFromJsonAsync<StandaloneOrganizerListResponse>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Organizers).Count().IsGreaterThanOrEqualTo(1);
    }

    private sealed class RegistrationQuestionListResponse
    {
        public List<RegistrationQuestionResponse> Questions { get; set; } = [];
    }

    private sealed class RegistrationQuestionResponse
    {
        public Guid Id { get; set; }
        public string QuestionText { get; set; } = "";
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
    }

    private sealed class TimelineListResponse
    {
        public List<TimelineItemResponse> TimelineItems { get; set; } = [];
    }

    private sealed class TimelineItemResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
    }

    private sealed class StandaloneParticipantListResponse
    {
        public int RegisteredCount { get; set; }
        public List<StandaloneParticipantResponse> Participants { get; set; } = [];
    }

    private sealed class StandaloneParticipantResponse
    {
        public Guid RegistrationId { get; set; }
        public Guid UserId { get; set; }
    }

    private sealed class StandaloneOrganizerListResponse
    {
        public List<StandaloneOrganizerResponse> Organizers { get; set; } = [];
    }

    private sealed class StandaloneOrganizerResponse
    {
        public Guid UserId { get; set; }
    }
}
