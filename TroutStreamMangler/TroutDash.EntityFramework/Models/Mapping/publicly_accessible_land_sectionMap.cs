using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class publicly_accessible_land_sectionMap : EntityTypeConfiguration<publicly_accessible_land_section>
    {
        public publicly_accessible_land_sectionMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            this.Property(t => t.id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // Table & Column Mappings
            this.ToTable("publicly_accessible_land_section", "public");
            this.Property(t => t.id).HasColumnName("id");
            this.Property(t => t.start).HasColumnName("start");
            this.Property(t => t.stop).HasColumnName("stop");
            this.Property(t => t.publicly_accessible_land_type_id).HasColumnName("publicly_accessible_land_type_id");
            this.Property(t => t.stream_gid).HasColumnName("stream_gid");

            // Relationships
            this.HasRequired(t => t.Stream)
                .WithMany(t => t.publicly_accessible_land_section2)
                .HasForeignKey(d => d.stream_gid);


        }
    }
}
