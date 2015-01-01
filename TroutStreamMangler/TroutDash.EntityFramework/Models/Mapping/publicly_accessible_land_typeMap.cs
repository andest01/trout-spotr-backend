using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class publicly_accessible_land_typeMap : EntityTypeConfiguration<Pal_type>
    {
        public publicly_accessible_land_typeMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            this.Property(t => t.id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.description)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("publicly_accessible_land_type", "public");
            this.Property(t => t.id).HasColumnName("id");
            this.Property(t => t.type).HasColumnName("type");
            this.Property(t => t.description).HasColumnName("description");
            this.Property(t => t.is_federal).HasColumnName("is_federal");

            this.Property(t => t.state_gid).HasColumnName("state_gid");

            this.HasRequired(t => t.state)
                .WithMany(t => t.publicly_accessible_land_types)
                .HasForeignKey(t => t.state_gid);
        }
    }
}
