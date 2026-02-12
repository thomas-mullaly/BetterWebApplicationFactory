using System.Net;
using System.Net.Http.Json;
using Shouldly;
using TestHelper.MinimalApis.App.Dtos;
using TestHelper.MinimalApis.App.Dtos.Lists;
using TestHelper.MinimalApis.App.Models;

namespace TestHelper.MinimalApis.App.Tests.Lists;

public class GetListsTests : EndpointTestBase
{
    public GetListsTests(EndpointTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ReturnsOnlyListsCreatedByTheUser()
    {
        var (user, aspnetUser) = await AuthHelper.CreateUserAsync("test@example.com");
        var client = ClientFactory.AuthenticateAsUser(user, aspnetUser).CreateClient();

        var (otherUser, _) = await AuthHelper.CreateUserAsync("other@example.com");

        var usersLists = new List<TodoList>
        {
            new()
            {
                Name = "List 1",
                CreatedBy = user,
                Items =
                [
                    new()
                    {
                        Name = "Item 1"
                    },
                    new()
                    {
                        Name = "Item 2"
                    }
                ]
            },
            new()
            {
                Name = "List 2",
                CreatedBy = user,
                Items =
                [
                    new()
                    {
                        Name = "Item 3"
                    }
                ]
            }
        };

        await SetupDbContext.TodoLists.AddRangeAsync(usersLists, TestContext.Current.CancellationToken);
        await SetupDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var otherUsersLists = new List<TodoList>
        {
            new()
            {
                Name = "List 3",
                CreatedBy = otherUser,
                Items =
                {
                    new()
                    {
                        Name = "Item 4"
                    }
                }
            }
        };

        await SetupDbContext.TodoLists.AddRangeAsync(otherUsersLists, TestContext.Current.CancellationToken);
        await SetupDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var response = await client.GetAsync("/api/lists", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<ApiResult<List<TodoListDto>>>(TestContext.Current.CancellationToken);

        dto.IsSuccess.ShouldBeTrue();
        dto.StatusCode.ShouldBe(200);
        dto.Data.Count.ShouldBe(2);

        foreach (var expectedList in usersLists)
        {
            var actualList = dto.Data.Single(l => l.Id == expectedList.Id);
            
            actualList.Name.ShouldBe(expectedList.Name);
            actualList.Items.Count.ShouldBe(expectedList.Items.Count);

            foreach (var expectedItem in expectedList.Items)
            {
                var actualItem = actualList.Items.Single(i => i.Id == expectedItem.Id);
                actualItem.CompletedAt.ShouldBe(expectedItem.CompletedAt);
                actualItem.CreatedAt.ShouldBe(expectedItem.CreatedAt);
                actualItem.Name.ShouldBe(expectedItem.Name);
                actualItem.IsComplete.ShouldBe(expectedItem.IsComplete);
                actualItem.TodoListId.ShouldBe(expectedItem.TodoListId);
            }
        }
    }
}