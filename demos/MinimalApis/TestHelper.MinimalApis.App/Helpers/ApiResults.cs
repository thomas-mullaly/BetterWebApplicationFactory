using TestHelper.MinimalApis.App.Dtos;

namespace TestHelper.MinimalApis.App.Helpers;

internal class UnauthorizedResult : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 401;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(new ApiResult<Unit> { StatusCode = 401, IsSuccess = false });
    }
}

internal class ForbiddenResult : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 403;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(new ApiResult<Unit> { StatusCode = 403, IsSuccess = false });
    }
}

public static class ApiResults
{
    public static IResult Ok<TData>(TData data)
        => Results.Ok(new ApiResult<TData> { Data = data, StatusCode = 200, IsSuccess = true });
    public static IResult Ok()
        => Results.Ok(new ApiResult<Unit> { StatusCode = 200, IsSuccess = true });
    public static IResult BadRequest(string error)
        => Results.BadRequest(new ApiResult<Unit>
            { Errors = new List<string> { error }, StatusCode = 400, IsSuccess = false });
    public static IResult Unauthorized()
        => new UnauthorizedResult();
    public static IResult Forbidden()
        => new ForbiddenResult();
    public static IResult NotFound()
        => Results.NotFound(new ApiResult<Unit> { StatusCode = 404, IsSuccess = false });
}