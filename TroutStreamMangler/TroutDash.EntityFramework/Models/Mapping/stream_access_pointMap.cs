using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class stream_access_pointMap : EntityTypeConfiguration<stream_access_point>
    {
        public stream_access_pointMap()
        {
            // Primary Key
            this.HasKey(t => t.gid);

            // Properties

            this.Property(t => t.street_name)
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("stream_access_point", "public");
            this.Property(t => t.gid).HasColumnName("gid");
            this.Property(t => t.linear_offset).HasColumnName("linear_offset");
            this.Property(t => t.is_accessible).HasColumnName("is_over_publicly_accessible_land");
            this.Property(t => t.stream_gid).HasColumnName("stream_gid");
            this.Property(t => t.centroid_latitude).HasColumnName("centroid_latitude");
            this.Property(t => t.centroid_longitude).HasColumnName("centroid_longitude");

            // Relationships
                
            this.HasRequired(t => t.stream)
                .WithMany(t => t.stream_access_points)
                .HasForeignKey(d => d.stream_gid);

            this.HasRequired(t => t.road)
                .WithMany(t => t.road_crossings)
                .HasForeignKey(d => d.road_gid);
        }
    }
}