using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JinCreek.Server.Interfaces
{
    public class SimDeviceAuthenticationRequest
    {
        /// <summary>
        /// IMEI。機器の識別番号
        /// </summary>
        /// <example>
        /// "352555093320000"
        /// </example>>
        [Required(ErrorMessage = "imei_required")]
        [StringLength(15, ErrorMessage = "imei_invalid_length")]
        public string DeviceImei { get; set; }

        /// <summary>
        /// ICCID。SIMカードに割り当てられるシリアル番号。
        /// </summary>
        /// <example>
        /// "8981100005819480000"
        /// </example>>
        [Required(ErrorMessage = "iccid_required")]
        [StringLength(19)]
        public string SimIccId { get; set; }

        /// <summary>
        /// IMSI。携帯電話事業者と契約する際に発行される加入者識別番号。
        /// </summary>
        /// <example>
        /// "440103213100000"
        /// </example>>
        [Required(ErrorMessage = "imsi_required")]
        [StringLength(15)]
        public string SimImsi { get; set; }

        /// <summary>
        /// MSISDN。携帯電話網への加入を一意に識別する番号
        /// </summary>
        /// <example>
        /// "02017911000"
        /// </example>>
        [Required(ErrorMessage = "msisdn_required")]
        [StringLength(15)]
        public string SimMsisdn { get; set; }

        /// <summary>
        /// 現在の端末に割り当てられているIPアドレス; 将来的(iOS/Android)にサーバー側でIPアドレス変更に伴うLTE再接続を行う場合に利用。
        /// </summary>
        [RegularExpression("(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]).){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])", ErrorMessage = "ip_address_invalid")]
        public string DeviceIpAddress { get; set; }
    }

    public class SimDeviceAuthenticationResponse
    {
        /// <summary>
        /// 認証成功時のUUID。
        /// </summary>
        /// <example>
        /// "0e4e88ae-c880-11e2-8598-5855cafa776b"
        /// </example>>
        [Required]
        public string AuthId { get; set; }

        // 主にWindows端末を想定し、自身のIPアドレスと異なる場合、LTE再接続を試みる。iOS/Androidはサーバー側での対応を予定
        public string AssignDeviceIpAddress { get; set; }

        /// <summary>
        /// ログイン可能ユーザー一覧。
        /// </summary>
        /// <example>
        /// [ "JINCREEK\\initialpoint" ]
        /// </example>>
        public List<string> CanLogonUsers { get; set; }

        // is_disconnect_network_screen_lock など
        /// <summary>
        /// SIMや端末機器に関わるサーバー側に保持する動的な設定項目。
        /// </summary>
        public Dictionary<string, string> SimDeviceConfigureDictionary { get; set; }
    }
}
