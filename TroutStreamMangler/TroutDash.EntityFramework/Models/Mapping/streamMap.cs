using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class streamMap : EntityTypeConfiguration<stream>
    {
        public streamMap()
        {
            // Primary Key
            this.HasKey(t => t.gid);

            // Properties
            this.Property(t => t.gid)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.name)
                .HasMaxLength(70);

            this.Property(t => t.local_name)
                .HasMaxLength(70);

            this.Property(t => t.status_mes)
                .HasMaxLength(50);

            this.Property(t => t.state)
                .HasMaxLength(20);

            this.Property(t => t.source)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("stream", "public");
            this.Property(t => t.gid).HasColumnName("gid");
            this.Property(t => t.name).HasColumnName("name");
            this.Property(t => t.local_name).HasColumnName("local_name");
            this.Property(t => t.length_mi).HasColumnName("length_mi");
            this.Property(t => t.centroid_latitude).HasColumnName("centroid_latitude");
            this.Property(t => t.centroid_longitude).HasColumnName("centroid_longitude");
            this.Property(t => t.has_brown_trout).HasColumnName("has_brown_trout");
            this.Property(t => t.has_brook_trout).HasColumnName("has_brook_trout");
            this.Property(t => t.has_rainbow_trout).HasColumnName("has_rainbow_trout");
            this.Property(t => t.is_brown_trout_stocked).HasColumnName("is_brown_trout_stocked");
            this.Property(t => t.is_brook_trout_stocked).HasColumnName("is_brook_trout_stocked");
            this.Property(t => t.is_rainbow_trout_stocked).HasColumnName("is_rainbow_trout_stocked");
            this.Property(t => t.status_mes).HasColumnName("status_message");
            this.Property(t => t.state).HasColumnName("state");
            this.Property(t => t.source).HasColumnName("source");
            this.Property(t => t.state_gid).HasColumnName("state_gid");
            this.Property(t => t.source_id).HasColumnName("source_id");
            this.Property(t => t.slug).HasColumnName("slug");

            // Relationships
            this.HasRequired(t => t.state1)
                .WithMany(t => t.streams)
                .HasForeignKey(d => d.state_gid);

            this.HasMany(t => t.publicly_accessible_lands)
                .WithMany(t => t.streams)
                .Map(m =>
                {
                    m.ToTable("stream_publicly_accessible_land", "public");
                    m.MapLeftKey("trout_stream_gid");
                    m.MapRightKey("publicly_accessible_land_gid");
                });

        }

        
    }
}
