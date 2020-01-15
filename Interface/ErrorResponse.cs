using System.ComponentModel.DataAnnotations;

namespace JinCreek.Server.Interfaces
{
    /// <summary>
    /// エラーレスポンス
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// SIMデバイス認証の合致しないエラー
        /// </summary>
        public static readonly ErrorResponse NotMatchSimDevice = new ErrorResponse { ErrorCode = "1001", ErrorMessage = "Not Match SimDevice Info" };
        /// <summary>
        /// 多要素認証の合致しないエラー
        /// </summary>
        public static readonly ErrorResponse NotMatchMultiFactor = new ErrorResponse { ErrorCode = "1002", ErrorMessage = "Not Match MultiFactor Info" };
        /// <summary>
        /// SIMデバイス認証済みの合致しないがエラー
        /// </summary>
        public static readonly ErrorResponse NotMatchAuthId = new ErrorResponse { ErrorCode = "1003", ErrorMessage = "Not Match AuthId Info" };
        
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
