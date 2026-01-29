namespace TestHelper.MinimalApis.App.Dtos;

public class Unit;

public class ApiResult<TData>
{
    public TData? Data { get; set; }
    public List<string>? Errors { get; set; }
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
}