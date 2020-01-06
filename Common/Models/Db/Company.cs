using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Common.Models.Db
{
    public class Company
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("company_id")]
        public long Id { get; set; }


        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Column("code", TypeName = "VARCHAR(255) BINARY")]
        public string Code { get; set; }

        [Column("name", TypeName = "LONGTEXT BINARY")]
        public string Name { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
