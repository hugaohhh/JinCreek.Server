using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JinCreek.Server.Interfaces
{
    public class SimDeviceAuthenticationRequest
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

        // 現在の端末に割り当てられているIPアドレス; 将来的(iOS/Android)にサーバー側でIPアドレス変更に伴うLTE再接続を行う場合に利用
        [RegularExpression("(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]).){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])", ErrorMessage = "ip_address_invalid")]
        public string DeviceIpAddress { get; set; }
    }

    public class SimDeviceAuthenticationResponse
    {
        [Required]
        public string AuthId { get; set; }

        // 主にWindows端末を想定し、自身のIPアドレスと異なる場合、LTE再接続を試みる。iOS/Androidはサーバー側での対応を予定
        public string AssignDeviceIpAddress { get; set; }

        public List<string> CanLogonUsers { get; set; }

        // is_disconnect_network_screen_lock など
        public Dictionary<string, string> SimDeviceConfigureDictionary { get; set; }
    }
}
