
using CookiesProyect.Dtos.Common;

namespace CookiesProyect.Helpers
{
    public static class ResponseHelper
    { 
        public static ResponseDto<T> ResponseError<T>(int statusCode, string message, T data = default)
        {
            return new ResponseDto<T>
            {
                StatusCode = statusCode,
                Status = false,
                Message = message,
                Data = data
            };
        }

        public static ResponseDto<T> ResponseSuccess<T>(int statusCode, string message, T data = default)
        {
            return new ResponseDto<T>
            {
                StatusCode = statusCode,
                Status = true,
                Message = message,
                Data = data
            };
        }
    }
}