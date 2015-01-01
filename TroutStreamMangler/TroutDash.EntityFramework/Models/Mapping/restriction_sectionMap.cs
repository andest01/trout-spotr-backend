using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class restriction_sectionMap : EntityTypeConfiguration<restriction_section>
    {
        public restriction_sectionMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            this.Property(t => t.id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("restriction_section", "public");
            this.Property(t => t.id).HasColumnName("id");
            this.Property(t => t.start).HasColumnName("start");
            this.Property(t => t.stop).HasColumnName("stop");
            this.Property(t => t.restriction_id).HasColumnName("restriction_id");
            this.Property(t => t.stream_gid).HasColumnName("stream_gid");

            // Relationships
            this.HasRequired(t => t.restriction)
                .WithMany(t => t.restriction_section)
                .HasForeignKey(d => d.restriction_id);
//            this.HasRequired(t => t.Stream)
//                .WithMany(t => t.restriction_section)
//                .HasForeignKey(d => d.stream_gid);

        }
    }
}
