namespace Elattaba.API.Helper
{
    public class ResponseAPI
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }

        public ResponseAPI(int statusCode, string? message = null, object? data = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
            Data = data;
        }

        private string? GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                200 => "Success",
                400 => "Bad request",
                401 => "Unauthorized",
                404 => "Resource not found",
                500 => "Server error occurred",
                _ => null
            };
        }
    }
}
