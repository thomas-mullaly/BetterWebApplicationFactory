using Microsoft.EntityFrameworkCore;
using TestHelper.MinimalApis.App.Auth;
using TestHelper.MinimalApis.App.Data;
using TestHelper.MinimalApis.App.Dtos.Lists;
using TestHelper.MinimalApis.App.Helpers;

namespace TestHelper.MinimalApis.App.Endpoints;

public static class ListEndpoints
{
    public static void RegisterRoutes(WebApplication app)
    {
        app.MapGet("/lists", GetList);
    }

    private static async Task<IResult> GetList(
        ICurrentUser currentUser,
        AppDbContext dbContext,
        CancellationToken ct)
    {
        var lists = await dbContext.TodoLists
            .Include(l => l.CreatedBy)
            .Include(l => l.Items)
            .Where(l => l.CreatedBy.Id == currentUser.UserId)
            .ToListAsync(ct);

        var listDtos = lists.Select(list => new TodoListDto
        {
            Id = list.Id,
            Name = list.Name,
            Items = list.Items.Select(i => new TodoItemDto { Id = i.Id, Name = i.Name }).ToList()
        }).ToList();

        return ApiResults.Ok(listDtos);
    }
}