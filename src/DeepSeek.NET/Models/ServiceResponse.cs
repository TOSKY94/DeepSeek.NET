
namespace DeepSeek.NET.Models
{
    /// <summary>
    /// Generic wrapper for API responses
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    public class ServiceResponse<T> : BaseResponse
    {
        /// <summary>
        /// Response data payload
        /// </summary>
        public T Data { get; set; } = default!;
    }

    /// <summary>
    /// Base response structure for all API responses
    /// </summary>
    public class BaseResponse
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error details if request failed
        /// </summary>
        public ErrorResponse Error { get; set; } = default!;

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }
    }

    /// <summary>
    /// Error response structure
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Error code identifier
        /// </summary>
        public string Code { get; set; } = default!;

        /// <summary>
        /// Human-readable error message
        /// </summary>
        public string Message { get; set; } = default!;

        /// <summary>
        /// Error type/category
        /// </summary>
        public string Type { get; set; } = default!;
    }
}
