using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Common.Models.Db
{
    public class UserGroup
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Key]
        [Column("user_group_id")]
        public Guid UserGroupId { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        [Column("user_group_name")]
        public string UserGroupName { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        [Column("domain_id")]
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
        [Column("user_id")]
        public Guid UserId { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        [Column("domain_id")]
        public Guid DomainId { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Required]
        [Column("user_group_id")]
        public Guid UserGroupId { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Column("last_name")]
        public string LastName { get; set; }

        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("setting_by_user")]
        public string SettingByUser { get; set; }

        public UserGroup UserGroup { get; set; }

        public Domain Domain { get; set; }
    }

    public class AdminUser : User
    {
        // DBアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [Column("password")]
        public string Password { get; set; }
    }

    public class GeneralUser : User
    {

    }
}
