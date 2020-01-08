using System.ComponentModel.DataAnnotations;

namespace JinCreek.Server.Interfaces
{
    public class ErrorResponse
    {
        [Required]
        public string ErrorCode { get; set; }

        [Required]
        public string ErrorMessage { get; set; }
    }
}
