using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class road_typeMap : EntityTypeConfiguration<road_type>
    {
        public road_typeMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            

            // Table & Column Mappings
            this.ToTable("road_type", "public");
            this.Property(t => t.source).HasColumnName("source");
            this.Property(t => t.description).HasColumnName("description");
            this.Property(t => t.type).HasColumnName("type");
            this.Property(t => t.state_gid).HasColumnName("state_gid");
            
            this.HasOptional(i => i.state)
                .WithMany(i => i.road_types)
                .HasForeignKey(i => i.state_gid);
        }
    }
}