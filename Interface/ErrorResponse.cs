using System.ComponentModel.DataAnnotations;

namespace JinCreek.Server.Interfaces
{
    public class ErrorResponse
    {
        public static readonly ErrorResponse NotMatchSimDevice = new ErrorResponse { ErrorCode = "1001", ErrorMessage = "Not Match SimDevice Info" };
        public static readonly ErrorResponse NotMatchMultiFactor = new ErrorResponse { ErrorCode = "1002", ErrorMessage = "Not Match MultiFactor Info" };

        [Required]
        public string ErrorCode { get; set; }

        [Required]
        public string ErrorMessage { get; set; }
    }
}
