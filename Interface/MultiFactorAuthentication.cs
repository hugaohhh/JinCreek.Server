using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JinCreek.Server.Interfaces
{
    public class MultiFactorAuthenticationRequest
    {
        /// <summary>
        /// 端末サインインで利用するアカウント。
        /// </summary>
        /// <example>
        /// "JINCREEK\\initialpoint"
        /// </example>>
        [Required(ErrorMessage = "account_required")]
        public string Account { get; set; }

        /// <summary>
        /// 端末認証成功時の返却値。
        /// </summary>
        /// <example>
        /// "0e4e88ae-c880-11e2-8598-5855cafa776b"
        /// </example>>
        [Required(ErrorMessage = "authId_required")]
        public string AuthId { get; set; }
    }

    public class MultiFactorAuthenticationResponse
    {
        /// <summary>
        /// ユーザーに関わるサーバー側に保持する動的な設定項目。
        /// </summary>
        public Dictionary<string, string> UserConfigureDictionary { get; set; }
    }
}
