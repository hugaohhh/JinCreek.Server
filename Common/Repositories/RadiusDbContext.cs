using JinCreek.Server.Common.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace JinCreek.Server.Common.Repositories
{
    public class RadiusDbContext : DbContext
    {
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public virtual DbSet<Nas> Nas { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public virtual DbSet<Radacct> Radacct { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public virtual DbSet<Radcheck> Radcheck { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public virtual DbSet<Radgroupcheck> Radgroupcheck { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public virtual DbSet<Radgroupreply> Radgroupreply { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public virtual DbSet<Radpostauth> Radpostauth { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public virtual DbSet<Radreply> Radreply { get; set; }
        // DbSetアクセスのため自動プロパティを利用
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public virtual DbSet<Radusergroup> Radusergroup { get; set; }

        public RadiusDbContext(DbContextOptions<RadiusDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Nas>(entity =>
            {
                entity.ToTable("nas");

                entity.HasIndex(e => e.Nasname)
                    .HasName("nasname");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(10)");

                entity.Property(e => e.Community)
                    .HasColumnName("community")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("'NULL'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasColumnType("varchar(200)")
                    .HasDefaultValueSql("'''RADIUS Client'''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Nasname)
                    .IsRequired()
                    .HasColumnName("nasname")
                    .HasColumnType("varchar(128)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                //entity.Property(e => e.Ports)
                //    .HasColumnName("ports")
                //    .HasColumnType("int(5)")
                //    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Secret)
                    .IsRequired()
                    .HasColumnName("secret")
                    .HasColumnType("varchar(60)")
                    .HasDefaultValueSql("'''secret'''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Server)
                    .HasColumnName("server")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("'NULL'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Shortname)
                    .HasColumnName("shortname")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("'NULL'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasColumnType("varchar(30)")
                    .HasDefaultValueSql("'''other'''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");
            });

            modelBuilder.Entity<Radacct>(entity =>
            {
                entity.ToTable("radacct");

                entity.HasIndex(e => e.Acctinterval)
                    .HasName("acctinterval");

                entity.HasIndex(e => e.Acctsessionid)
                    .HasName("acctsessionid");

                entity.HasIndex(e => e.Acctsessiontime)
                    .HasName("acctsessiontime");

                entity.HasIndex(e => e.Acctstarttime)
                    .HasName("acctstarttime");

                entity.HasIndex(e => e.Acctstoptime)
                    .HasName("acctstoptime");

                entity.HasIndex(e => e.Acctuniqueid)
                    .HasName("acctuniqueid")
                    .IsUnique();

                entity.HasIndex(e => e.Delegatedipv6prefix)
                    .HasName("delegatedipv6prefix");

                entity.HasIndex(e => e.Framedinterfaceid)
                    .HasName("framedinterfaceid");

                entity.HasIndex(e => e.Framedipaddress)
                    .HasName("framedipaddress");

                entity.HasIndex(e => e.Framedipv6address)
                    .HasName("framedipv6address");

                entity.HasIndex(e => e.Framedipv6prefix)
                    .HasName("framedipv6prefix");

                entity.HasIndex(e => e.Nasipaddress)
                    .HasName("nasipaddress");

                entity.HasIndex(e => e.Username)
                    .HasName("username");

                entity.HasIndex(e => new { e.Acctstoptime, e.Nasipaddress, e.Acctstarttime })
                    .HasName("bulk_close");

                entity.Property(e => e.Radacctid)
                    .HasColumnName("radacctid")
                    .HasColumnType("bigint(21)");

                entity.Property(e => e.Acctauthentic)
                    .HasColumnName("acctauthentic")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("'NULL'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                //entity.Property(e => e.Acctinputoctets)
                //    .HasColumnName("acctinputoctets")
                //    .HasColumnType("bigint(20)")
                //    .HasDefaultValueSql("'NULL'");

                //entity.Property(e => e.Acctinterval)
                //    .HasColumnName("acctinterval")
                //    .HasColumnType("int(12)")
                //    .HasDefaultValueSql("'NULL'");

                //entity.Property(e => e.Acctoutputoctets)
                //    .HasColumnName("acctoutputoctets")
                //    .HasColumnType("bigint(20)")
                //    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Acctsessionid)
                    .IsRequired()
                    .HasColumnName("acctsessionid")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                //entity.Property(e => e.Acctsessiontime)
                //    .HasColumnName("acctsessiontime")
                //    .HasColumnType("int(12) unsigned")
                //    .HasDefaultValueSql("'NULL'");

                //entity.Property(e => e.Acctstarttime)
                //    .HasColumnName("acctstarttime")
                //    .HasColumnType("datetime")
                //    .HasDefaultValueSql("'NULL'");

                //entity.Property(e => e.Acctstoptime)
                //    .HasColumnName("acctstoptime")
                //    .HasColumnType("datetime")
                //    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Acctterminatecause)
                    .IsRequired()
                    .HasColumnName("acctterminatecause")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Acctuniqueid)
                    .IsRequired()
                    .HasColumnName("acctuniqueid")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                //entity.Property(e => e.Acctupdatetime)
                //    .HasColumnName("acctupdatetime")
                //    .HasColumnType("datetime")
                //    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Calledstationid)
                    .IsRequired()
                    .HasColumnName("calledstationid")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Callingstationid)
                    .IsRequired()
                    .HasColumnName("callingstationid")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.ConnectinfoStart)
                    .HasColumnName("connectinfo_start")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("'NULL'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.ConnectinfoStop)
                    .HasColumnName("connectinfo_stop")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("'NULL'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Delegatedipv6prefix)
                    .IsRequired()
                    .HasColumnName("delegatedipv6prefix")
                    .HasColumnType("varchar(45)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Framedinterfaceid)
                    .IsRequired()
                    .HasColumnName("framedinterfaceid")
                    .HasColumnType("varchar(44)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Framedipaddress)
                    .IsRequired()
                    .HasColumnName("framedipaddress")
                    .HasColumnType("varchar(15)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Framedipv6address)
                    .IsRequired()
                    .HasColumnName("framedipv6address")
                    .HasColumnType("varchar(45)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Framedipv6prefix)
                    .IsRequired()
                    .HasColumnName("framedipv6prefix")
                    .HasColumnType("varchar(45)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Framedprotocol)
                    .HasColumnName("framedprotocol")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("'NULL'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Groupname)
                    .IsRequired()
                    .HasColumnName("groupname")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Nasipaddress)
                    .IsRequired()
                    .HasColumnName("nasipaddress")
                    .HasColumnType("varchar(15)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Nasportid)
                    .HasColumnName("nasportid")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("'NULL'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Nasporttype)
                    .HasColumnName("nasporttype")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("'NULL'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Realm)
                    .HasColumnName("realm")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Servicetype)
                    .HasColumnName("servicetype")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("'NULL'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");
            });

            modelBuilder.Entity<Radcheck>(entity =>
            {
                entity.ToTable("radcheck");

                entity.HasIndex(e => e.Username)
                    .HasName("username");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11) unsigned");

                entity.Property(e => e.Attribute)
                    .IsRequired()
                    .HasColumnName("attribute")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Op)
                    .IsRequired()
                    .HasColumnName("op")
                    .HasColumnType("char(2)")
                    //.HasDefaultValueSql("'''=='''")
                    .HasDefaultValueSql("'=='")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value")
                    .HasColumnType("varchar(253)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");
            });

            modelBuilder.Entity<Radgroupcheck>(entity =>
            {
                entity.ToTable("radgroupcheck");

                entity.HasIndex(e => e.Groupname)
                    .HasName("groupname");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11) unsigned");

                entity.Property(e => e.Attribute)
                    .IsRequired()
                    .HasColumnName("attribute")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Groupname)
                    .IsRequired()
                    .HasColumnName("groupname")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Op)
                    .IsRequired()
                    .HasColumnName("op")
                    .HasColumnType("char(2)")
                    //.HasDefaultValueSql("'''=='''")
                    .HasDefaultValueSql("'=='")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value")
                    .HasColumnType("varchar(253)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");
            });

            modelBuilder.Entity<Radgroupreply>(entity =>
            {
                entity.ToTable("radgroupreply");

                entity.HasIndex(e => e.Groupname)
                    .HasName("groupname");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11) unsigned");

                entity.Property(e => e.Attribute)
                    .IsRequired()
                    .HasColumnName("attribute")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Groupname)
                    .IsRequired()
                    .HasColumnName("groupname")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Op)
                    .IsRequired()
                    .HasColumnName("op")
                    .HasColumnType("char(2)")
                    //.HasDefaultValueSql("'''='''")
                    .HasDefaultValueSql("'='")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value")
                    .HasColumnType("varchar(253)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");
            });

            modelBuilder.Entity<Radpostauth>(entity =>
            {
                entity.ToTable("radpostauth");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Authdate)
                    .HasColumnName("authdate")
                    .HasColumnType("timestamp(6)")
                    .HasDefaultValueSql("current_timestamp(6)")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.Pass)
                    .IsRequired()
                    .HasColumnName("pass")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Reply)
                    .IsRequired()
                    .HasColumnName("reply")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");
            });

            modelBuilder.Entity<Radreply>(entity =>
            {
                entity.ToTable("radreply");

                entity.HasIndex(e => e.Username)
                    .HasName("username");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11) unsigned");

                entity.Property(e => e.Attribute)
                    .IsRequired()
                    .HasColumnName("attribute")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Op)
                    .IsRequired()
                    .HasColumnName("op")
                    .HasColumnType("char(2)")
                    //.HasDefaultValueSql("'''='''")
                    .HasDefaultValueSql("'='")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value")
                    .HasColumnType("varchar(253)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");
            });

            modelBuilder.Entity<Radusergroup>(entity =>
            {
                entity.ToTable("radusergroup");

                entity.HasIndex(e => e.Username)
                    .HasName("username");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11) unsigned");

                entity.Property(e => e.Groupname)
                    .IsRequired()
                    .HasColumnName("groupname")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");

                entity.Property(e => e.Priority)
                    .HasColumnName("priority")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasColumnType("varchar(64)")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_unicode_ci");
            });
        }
    }
}
