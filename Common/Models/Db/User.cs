using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models.Db
{
    public class UserGroup
    {
        [Key]
        [Column("user_group_id")]
        public Guid UserGroupId { get; set; }

        [Required]
        [Column("user_group_name")]
        public string UserGroupName { get; set; }

        [Required]
        [Column("domain_id")]
        public Guid DomainId { get; set; }

        public Domain Domain { get; set; }

        public List<User> Users { get; set; }
    }

    public class User
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("domain_id")]
        public Guid DomainId { get; set; }

        [Required]
        [Column("user_group_id")]
        public Guid UserGroupId { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("setting_by_user")]
        public string SettingByUser { get; set; }

        public UserGroup UserGroup { get; set; }

        public Domain Domain { get; set; }
    }

    public class AdminUser : User
    {
        [Column("password")]
        public string Password { get; set; }
    }

    public class GeneralUser : User
    {

    }
}
