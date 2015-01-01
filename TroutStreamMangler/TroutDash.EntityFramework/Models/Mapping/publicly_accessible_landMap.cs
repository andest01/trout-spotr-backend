using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class publicly_accessible_landMap : EntityTypeConfiguration<publicly_accessible_land>
    {
        public publicly_accessible_landMap()
        {
            // Primary Key
            this.HasKey(t => t.gid);

            // Properties
            this.Property(t => t.gid)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

//            this.Property(t => t.typ)

            this.Property(t => t.area_name)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("publicly_accessible_land", "public");
            this.Property(t => t.gid).HasColumnName("gid");
            this.Property(t => t.shape_area).HasColumnName("shape_area");

            this.HasRequired(t => t.type)
                .WithMany(t => t.publicly_accessible_lands)
                .HasForeignKey(d => d.publicly_accessible_land_type_id);

            this.HasRequired(t => t.state)
                .WithMany(t => t.publicly_accessible_land)
                .HasForeignKey(d => d.state_gid);
        }
    }
}