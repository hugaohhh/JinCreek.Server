using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace JinCreek.Server.Interfaces
{
    /// <summary>
    /// 多要素認証リクエスト
    /// </summary>
    public class MultiFactorAuthenticationRequest
    {
        /// <summary>
        /// 端末サインインで利用するアカウント。
        /// </summary>
        /// <example>
        /// "JINCREEK\\initialpoint"
        /// </example>>
        [Required(ErrorMessage = "account_required")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public string Account { get; set; }

        /// <summary>
        /// 端末認証成功時の返却値。
        /// </summary>
        /// <example>
        /// "0e4e88ae-c880-11e2-8598-5855cafa776b"
        /// </example>>
        [Required(ErrorMessage = "authId_required")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public Guid AuthId { get; set; }
    }

    /// <summary>
    /// 多要素認証レスポンス
    /// </summary>
    public class MultiFactorAuthenticationResponse
    {
        /// <summary>
        /// ユーザーに関わるサーバー側に保持する動的な設定項目。
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public Dictionary<string, string> UserConfigureDictionary { get; set; }
    }
}
