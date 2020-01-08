using System.ComponentModel.DataAnnotations;

namespace JinCreek.Server.Auth.Interfaces
{
    public class ErrorResponse
    {
        [Required]
        public string ErrorCode { get; set; }

        [Required]
        public string ErrorMessage { get; set; }
    }
}
