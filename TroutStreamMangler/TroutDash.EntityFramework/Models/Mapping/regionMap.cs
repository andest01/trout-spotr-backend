using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class regionMap : EntityTypeConfiguration<region>
    {
        public regionMap()
        {
            // Primary Key
            // Properties
            this.HasKey(t => t.gid);

            // Properties
            this.Property(t => t.name);

            // Table & Column Mappings
            this.ToTable("region", "public");
            this.Property(t => t.gid).HasColumnName("gid");
            this.Property(t => t.name).HasColumnName("name");
            this.HasMany(t => t.counties).WithOptional(t => t.region);

        }
    }
}