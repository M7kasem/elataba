namespace Elattaba.API.Helper
{
    public class APIException : ResponseAPI
    {
        public APIException(int statusCode, string? message = null, object? data = null, string? detailedMessage = null) : base(statusCode, message, data)
        {
            DetailedMessage = detailedMessage;
        }
        public string? DetailedMessage { get; set; }
    }
}
