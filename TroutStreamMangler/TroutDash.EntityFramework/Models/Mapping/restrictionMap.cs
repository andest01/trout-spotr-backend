using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class restrictionMap : EntityTypeConfiguration<restriction>
    {
        public restrictionMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            this.Property(t => t.id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.legal_text)
                .IsRequired();

            this.Property(t => t.short_text)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("restriction", "public");
            this.Property(t => t.id).HasColumnName("id");
            this.Property(t => t.legal_text).HasColumnName("legal_text");
            this.Property(t => t.short_text).HasColumnName("short_text");
            this.Property(t => t.isSeasonal).HasColumnName("isSeasonal");
            this.Property(t => t.start_time).HasColumnName("start_time");
            this.Property(t => t.end_time).HasColumnName("end_time");
            this.Property(t => t.isAnglingRestriction).HasColumnName("isAnglingRestriction");
            this.Property(t => t.isHarvestRestriciton).HasColumnName("isHarvestRestriciton");
            this.Property(t => t.state_gid).HasColumnName("state_gid");

            // Relationships
            this.HasRequired(t => t.state)
                .WithMany(t => t.restrictions)
                .HasForeignKey(d => d.state_gid);

        }
    }
}
