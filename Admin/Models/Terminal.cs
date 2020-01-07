using System.ComponentModel.DataAnnotations;

namespace Admin.Models
{
    /// <summary>
    /// 端末
    /// </summary>
    public class Terminal
    {
        /// <summary>
        /// 名前
        /// </summary>
        [Key]
        public string Name { get; set; }

        /// <summary>
        /// 社内管理番号
        /// </summary>
        public string No { get; set; }

        /// <summary>
        /// 機種名
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// 製造番号
        /// </summary>
        public string SerialNo { get; set; }
    }
}
