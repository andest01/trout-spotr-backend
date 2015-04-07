using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class lake_sectionMap : EntityTypeConfiguration<lake_section>
    {
        public lake_sectionMap()
        {
            // Primary Key
            this.HasKey(t => t.gid);

            // Properties
            this.Property(t => t.gid)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // Table & Column Mappings
            this.ToTable("lake_secction", "public");
            this.Property(t => t.gid).HasColumnName("gid");
            this.Property(t => t.start).HasColumnName("start");
            this.Property(t => t.stop).HasColumnName("stop");

            this.HasRequired(t => t.lake)
                .WithMany(t => t.lake_sections)
                .HasForeignKey(d => d.lake_gid);    

            this.HasRequired(t => t.stream)
                .WithMany(t => t.lake_sections)
                .HasForeignKey(d => d.stream_gid);
        }
    }
}