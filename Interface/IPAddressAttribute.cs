using System.ComponentModel.DataAnnotations;
using System.Net;

namespace JinCreek.Server.Interfaces
{
    class IpAddressAttribute : ValidationAttribute
    {
        private string GetErrorMessage(string ipAddress)
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                return $"Invalid IpAddress {ipAddress}.";
            }
            return ErrorMessage;
        }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var ipAddress = (string)value;
            if (ipAddress == null)
            {
                return ValidationResult.Success;
            }
            return IPAddress.TryParse(ipAddress, out _) ? ValidationResult.Success : new ValidationResult(GetErrorMessage(ipAddress));
        }
    }
}
