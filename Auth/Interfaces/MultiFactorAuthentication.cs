using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JinCreek.Server.Auth.Interfaces
{
    public class MultiFactorAuthenticationRequest
    {
        [Required(ErrorMessage = "account_required")]
        public string Account { get; set; }

        [Required(ErrorMessage = "authId_required")]
        public string AuthId { get; set; }
    }

    public class MultiFactorAuthenticationResponse
    {
        public Dictionary<string, string> UserConfigureDictionary { get; set; }
    }
}
