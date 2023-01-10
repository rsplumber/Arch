namespace Arch.Responses;

public sealed class ResponseFactory<TResponse>
{
    public static BaseResponse Create(object response)
    {
        return new BaseResponse
        {
            Meta = new Meta()
            {
                Code = 1000,
                Message = "Success"
            },
            Data = response
        };
    }

    public static BaseResponse Create(int code, string message, object response)
    {
        return new BaseResponse()
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