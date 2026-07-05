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
                400 => "A bad request, you have made", // خطأ في البيانات اللي العميل باعتها
                401 => "Authorized, you are not",      // العميل مش مسجل دخول
                404 => "Resource found, it was not",   // الحاجة مش موجودة في الداتا بيز
                500 => "Server error occurred",        // خطأ من السيرفر نفسه
                _ => null                              // أي كود تاني مش هيرجع رسالة
            };
        }
    }
}
