using System.ComponentModel.DataAnnotations;

namespace JinCreek.Server.Interfaces
{
    public class DeauthenticationRequest
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
        /// 端末サインインで利用するアカウント。
        /// </summary>
        /// <example>
        /// "JINCREEK\\initialpoint"
        /// </example>>
        [Required(ErrorMessage = "account_required")]
        public string Account { get; set; }
    }
}
