using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
//    public class road_crossingMap : EntityTypeConfiguration<road_crossing>
//    {
//        public road_crossingMap()
//        {
//            // Primary Key
//            this.HasKey(t => t.gid);
//
//            // Table & Column Mappings
//            this.ToTable("stream_access_point", "public");
//            this.Property(t => t.street_name).HasColumnName("street_name");
//            this.Property(t => t.is_over_publicly_accessible_land).HasColumnName("is_over_publicly_accessible_land");
//            this.Property(t => t.road_gid).HasColumnName("road_gid");
//            this.Property(t => t.stream_gid).HasColumnName("stream_gid");
//
//            this.HasRequired(t => t.road_gid)
//                .With(t => t.roads)
//                .HasForeignKey(d => d.road_type_id);
//
//            this.HasRequired(t => t.road_type)
//                .WithMany(t => t.roads)
//                .HasForeignKey(d => d.road_type_id);
//        }
//    }

    public class roadMap : EntityTypeConfiguration<road>
    {
        public roadMap()
        {
            // Primary Key
            this.HasKey(t => t.gid);

            // Properties
            

            // Table & Column Mappings
            this.ToTable("road", "public");
            this.Property(t => t.name).HasColumnName("name");
            this.Property(t => t.local_name).HasColumnName("local_name");
            this.Property(t => t.source).HasColumnName("source");
            this.Property(t => t.state_gid).HasColumnName("state_gid");
            this.Property(t => t.road_type_id).HasColumnName("road_type_id");

            this.HasRequired(i => i.state)
                .WithMany(i => i.roads)
                .HasForeignKey(i => i.state_gid);

            this.HasRequired(t => t.road_type)
                .WithMany(t => t.roads)
                .HasForeignKey(d => d.road_type_id);
//
//
//            this.HasMany(t => t.stream)
//                .WithMany(t => t.roads)
//                .Map(m =>
//                {
//                    m.ToTable("stream_access_point", "public");
//                    m.MapLeftKey("road_gid");
//                    m.MapRightKey("stream_gid");
//                });
        }
    }
}