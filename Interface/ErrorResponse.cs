using System.ComponentModel.DataAnnotations;

namespace JinCreek.Server.Interfaces
{
    public class ErrorResponse
    {
        public static readonly ErrorResponse NotMatchSimDevice = new ErrorResponse { ErrorCode = "1001", ErrorMessage = "Not Match SimDevice Info" };
        public static readonly ErrorResponse NotMatchMultiFactor = new ErrorResponse { ErrorCode = "1002", ErrorMessage = "Not Match MultiFactor Info" };

        /// <summary>
        /// エラーコード。
        /// </summary>
        /// <example>
        /// "10001"
        /// </example>>
        [Required]
        public string ErrorCode { get; set; }

        /// <summary>
        /// エラーメッセージ。
        /// </summary>
        /// <example>
        /// "Not found record."
        /// </example>>
        [Required]
        public string ErrorMessage { get; set; }
    }
}
