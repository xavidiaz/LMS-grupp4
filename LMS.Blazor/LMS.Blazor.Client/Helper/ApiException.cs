using System.Net;

namespace LMS.Blazor.Client.Helper
{
    public sealed class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? Message { get; }
        public ApiException(
            HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
