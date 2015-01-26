using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace TroutDash.EntityFramework.Models.Mapping
{
    public class restriction_routeMap : EntityTypeConfiguration<restriction_route>
    {
        public restriction_routeMap()
        {
            this.HasKey(t => t.gid);

            this.Property(t => t.gid)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.ToTable("restriction_route", "public");

            this.Property(t => t.gid).HasColumnName("gid");
            this.Property(t => t.restriction_type_id).HasColumnName("restriction_type_id");
            this.Property(t => t.source_id).HasColumnName("source_id");

            this.HasRequired(t => t.Restriction)
                .WithMany(t => t.restrictionRoutes)
                .HasForeignKey(t => t.restriction_type_id);
        }
    }
}