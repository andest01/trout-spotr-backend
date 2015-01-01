using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class trout_stream_sectionMap : EntityTypeConfiguration<trout_stream_section>
    {
        public trout_stream_sectionMap()
        {
            // Primary Key
            this.HasKey(t => t.gid);

            // Properties
            this.Property(t => t.gid)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.section_name)
                .HasMaxLength(70);

            // Table & Column Mappings
            this.ToTable("trout_stream_section", "public");
            this.Property(t => t.gid).HasColumnName("gid");
            this.Property(t => t.section_name).HasColumnName("section_name");
            this.Property(t => t.length_mi).HasColumnName("length_mi");
            this.Property(t => t.public_length).HasColumnName("public_length");
            this.Property(t => t.centroid_latitude).HasColumnName("centroid_latitude");
            this.Property(t => t.centroid_longitude).HasColumnName("centroid_longitude");
            this.Property(t => t.source_id).HasColumnName("source_id");
            this.Property(t => t.stream_gid).HasColumnName("stream_gid");
            this.Property(t => t.start).HasColumnName("start");
            this.Property(t => t.stop).HasColumnName("stop");

            // Relationships
            this.HasRequired(t => t.stream)
                .WithMany(t => t.trout_stream_sections)
                .HasForeignKey(d => d.stream_gid);
        }
    }
}