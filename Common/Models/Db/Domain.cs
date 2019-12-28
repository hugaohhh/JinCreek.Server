using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models.Db
{
    public class Domain
    {
        [Key]
        [Column("domain_id")]
        public Guid DomainId { get; set; }

        [Required]
        [Column("domain_name")]
        public string DomainName { get; set; }

        public List<User> Users { get; set; }

        public List<UserGroup> UserGroups { get; set; }
    }
}
