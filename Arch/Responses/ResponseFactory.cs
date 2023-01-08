namespace Arch.Responses;

public sealed class ResponseFactory<TResponse>
{
    public static BaseResponse<TResponse> Create(TResponse response)
    {
        return new BaseResponse<TResponse>()
        {
            Meta = new Meta()
            {
                Code = 1000,
                Message = "Success"
            },
            Data = response
        };
    }

    public static BaseResponse<TResponse> Create(int code, string message, TResponse response)
    {
        return new BaseResponse<TResponse>()
        {
            Meta = new Meta()
            {
                Code = code,
                Message = message
            },
            Data = response
        };
    }
}