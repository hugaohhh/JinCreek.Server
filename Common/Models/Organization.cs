using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace JinCreek.Server.Common.Models
{
    public class Organization : IValidatableObject
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Code { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        [Column(TypeName = "LONGTEXT BINARY")]
        public string Name { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        [Column(TypeName = "LONGTEXT BINARY")]
        public string Address { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        [Phone]
        [StringLength(11, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 10)]
        [Column(TypeName = "LONGTEXT BINARY")]
        public string DelegatePhone { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        [Url]
        [Column(TypeName = "LONGTEXT BINARY")]
        public string Url { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        [Phone]
        [StringLength(11, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 10)]
        [Column(TypeName = "LONGTEXT BINARY")]
        public string AdminPhone { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        [EmailAddress]
        [Column(TypeName = "LONGTEXT BINARY")]
        public string AdminMail { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        [Column(TypeName = "DATE")]
        public DateTime StartDay { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        [Column(TypeName = "DATE")]
        public DateTime EndDay { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        public bool IsValid { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
        public List<Domain> Domains { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
        public List<SimGroup> SimGroups { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
        public List<DeviceGroup> DeviceGroups { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDay >= EndDay)
            {
                yield return new ValidationResult("EndDay must be greater than StartDay");
            }
        }
    }
}
