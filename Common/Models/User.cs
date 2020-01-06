using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace JinCreek.Server.Common.Models
{
    public class UserGroup
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Key]
        public Guid Id { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        [Column(TypeName = "LONGTEXT BINARY")]
        public string UserGroupName { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        public Guid DomainId { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public Domain Domain { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public List<User> Users { get; set; }
    }

    public class User
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Key]
        public Guid Id { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        public Guid DomainId { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        public Guid UserGroupId { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Column(TypeName = "LONGTEXT BINARY")]
        public string LastName { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Column(TypeName = "LONGTEXT BINARY")]
        public string FirstName { get; set; }

        public string SettingByUser { get; set; }

        public UserGroup UserGroup { get; set; }

        public Domain Domain { get; set; }
    }

    public class AdminUser : User
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public string Password { get; set; }
    }

    public class GeneralUser : User
    {

    }
}
