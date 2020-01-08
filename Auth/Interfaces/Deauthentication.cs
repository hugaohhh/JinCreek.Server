using System.ComponentModel.DataAnnotations;

namespace JinCreek.Server.Auth.Interfaces
{
    public class DeauthenticationRequest
    {
        [Required(ErrorMessage = "imei_required")]
        [StringLength(15, ErrorMessage = "imei_invalid_length")]
        public string DeviceImei { get; set; }

        [Required(ErrorMessage = "iccid_required")]
        [StringLength(19)]
        public string SimIccId { get; set; }

        [Required(ErrorMessage = "imsi_required")]
        [StringLength(15)]
        public string SimImsi { get; set; }

        [Required(ErrorMessage = "msisdn_required")]
        [StringLength(15)]
        public string SimMsisdn { get; set; }

        [Required(ErrorMessage = "account_required")]
        public string Account { get; set; }
    }
}
