using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class lakeMap : EntityTypeConfiguration<lake>
    {
        public lakeMap()
        {
            // Primary Key
            this.HasKey(t => t.gid);

            // Properties
            this.Property(t => t.gid)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // Table & Column Mappings
            this.ToTable("lake", "public");
            this.Property(t => t.gid).HasColumnName("gid");
            this.Property(t => t.source_id).HasColumnName("source_id");
            this.Property(t => t.name).HasColumnName("name");

            this.HasRequired(t => t.state)
                .WithMany(t => t.lakes)
                .HasForeignKey(d => d.state_gid);
        }
    }
}