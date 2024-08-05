namespace SharedLib.Exceptions
{
    public class CustomErrorResponseException : Exception
    {
        public int StatusCode { get; set; }
        public CustomErrorResponseException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
