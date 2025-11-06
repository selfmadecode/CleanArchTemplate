using Microsoft.AspNetCore.Http;

namespace Application.Models;

public record BaseResponse<T>
{
    public int StatusCode { get; set; }
    public T? Data { get; set; }
    public string ResponseMessage { get; set; }
    public int TotalCount { get; set; }
    public List<string> Errors { get; set; } = [];

    public BaseResponse(T data, string responseMessage = "Success")
    {
        Data = data;
        StatusCode = StatusCodes.Status200OK;
        ResponseMessage = responseMessage;
    }

    public BaseResponse(T data, int totalCount, string responseMessage = "Success")
    {
        Data = data;
        TotalCount = totalCount;
        StatusCode = StatusCodes.Status200OK;
        ResponseMessage = responseMessage;
    }

    public BaseResponse(string error, List<string> errors)
    {
        StatusCode = StatusCodes.Status400BadRequest;
        ResponseMessage = error;
        Errors = [.. errors, error];
    }
}
