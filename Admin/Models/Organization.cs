using System;

namespace Admin.Models
{
    /// <summary>
    /// 組織
    /// </summary>
    public class Organization
    {
        /// <summary>
        /// コード
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 住所
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 代表電話番号
        /// </summary>
        public string TelNo { get; set; }

        /// <summary>
        /// コーポレートサイトURL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 管理者連絡先電話番号
        /// </summary>
        public string AdminTelNo { get; set; }

        /// <summary>
        /// 管理者連絡先メールアドレス
        /// </summary>
        public string AdminEmail { get; set; }

        /// <summary>
        /// 利用開始日
        /// </summary>
        public DateTime StartAt { get; set; }

        /// <summary>
        /// 利用終了日
        /// </summary>
        public DateTime EndAt { get; set; }

        /// <summary>
        /// 有効
        /// </summary>
        public Boolean IsActive { get; set; }
    }
}
