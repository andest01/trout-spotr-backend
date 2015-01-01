using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class countyMap : EntityTypeConfiguration<county>
    {
        public countyMap()
        {
            // Primary Key
            this.HasKey(t => t.gid);

            // Properties
            this.Property(t => t.statefp)
                .HasMaxLength(2);

            this.Property(t => t.countyfp)
                .HasMaxLength(3);

            this.Property(t => t.name)
                .HasMaxLength(100);

            this.Property(t => t.lsad)
                .HasMaxLength(2);

            // Table & Column Mappings
            this.ToTable("county", "public");
            this.Property(t => t.gid).HasColumnName("gid");
            this.Property(t => t.statefp).HasColumnName("statefp");
            this.Property(t => t.countyfp).HasColumnName("countyfp");
            this.Property(t => t.name).HasColumnName("name");
            this.Property(t => t.lsad).HasColumnName("lsad");
            this.Property(t => t.state_gid).HasColumnName("state_gid");

            // Relationships
            this.HasMany(t => t.stream)
                .WithMany(t => t.counties)
                .Map(m =>
                    {
                        m.ToTable("stream_county", "public");
                        m.MapLeftKey("county_gid");
                        m.MapRightKey("trout_stream_gid");
                    });

            this.HasRequired(t => t.state)
                .WithMany(t => t.counties)
                .HasForeignKey(d => d.state_gid);

        }
    }
}
