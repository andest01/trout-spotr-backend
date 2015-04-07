using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class stateMap : EntityTypeConfiguration<state>
    {
        public stateMap()
        {
            // Primary Key
            this.HasKey(t => t.gid);

            // Properties
            this.Property(t => t.statefp)
                .IsRequired()
                .HasMaxLength(2);

            this.Property(t => t.short_name)
                .HasMaxLength(10);

            this.Property(t => t.Name)
                .HasMaxLength(26);

            // Table & Column Mappings
            this.ToTable("state", "public");
            this.Property(t => t.gid).HasColumnName("gid");
            this.Property(t => t.statefp).HasColumnName("statefp");
            this.Property(t => t.short_name).HasColumnName("short_name");
            this.Property(t => t.Name).HasColumnName("name");
        }
    }
}
