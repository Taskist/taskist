namespace Taskist.Core.Dtos.Git;

public class GitResult<T>
{
    public bool Success { get; set; }

    public T? Data { get; set; }

    public int StatusCode { get; set; }

    public string? Error { get; set; }

    public static GitResult<T> Ok(T data, int statusCode = 200) => new() { Success = true, Data = data, StatusCode = statusCode };

    public static GitResult<T> Fail(string error, int statusCode = 0) => new() { Success = false, Error = error, StatusCode = statusCode };
}
