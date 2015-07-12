using System.Data.Entity;
using TroutDash.EntityFramework.Models.Mapping;

namespace TroutDash.EntityFramework.Models
{
    public partial class TroutDashPrototypeContext : DbContext
    {
        static TroutDashPrototypeContext()
        {
//            Database.SetInitializer<TroutDashPrototypeContext>(null);
        }

        public TroutDashPrototypeContext()
            : base("Name=troutdash2")
        {
            Database.SetInitializer<TroutDashPrototypeContext>(null);
        }

        public DbSet<county> counties { get; set; }
        public DbSet<publicly_accessible_land_section> PalSection { get; set; }
        public DbSet<publicly_accessible_land_type> PalTypes { get; set; }
        public DbSet<restriction> restrictions { get; set; }
        public DbSet<restriction_section> restriction_section { get; set; }
        public DbSet<state> states { get; set; }
        public DbSet<stream> streams { get; set; }
        public DbSet<trout_stream_section> trout_stream_sections { get; set; }
        public DbSet<publicly_accessible_land> publicly_accessible_lands { get; set; }
        public DbSet<lake> lakes { get; set; }
        public DbSet<lake_section> lake_sections { get; set; }
        public DbSet<restriction_route> restriction_routes { get; set; }
        public DbSet<region> regions { get; set; }
        public DbSet<road> roads { get; set; }
        public DbSet<road_type> road_types { get; set; }
        public DbSet<stream_access_point> stream_access_points { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new countyMap());
            modelBuilder.Configurations.Add(new publicly_accessible_land_sectionMap());
            modelBuilder.Configurations.Add(new publicly_accessible_land_typeMap());
            modelBuilder.Configurations.Add(new restrictionMap());
            modelBuilder.Configurations.Add(new restriction_sectionMap());
            modelBuilder.Configurations.Add(new stateMap());
            modelBuilder.Configurations.Add(new streamMap());
            modelBuilder.Configurations.Add(new trout_stream_sectionMap());
            modelBuilder.Configurations.Add(new publicly_accessible_landMap());
            modelBuilder.Configurations.Add(new lakeMap());
            modelBuilder.Configurations.Add(new lake_sectionMap());
            modelBuilder.Configurations.Add(new restriction_routeMap());
            modelBuilder.Configurations.Add(new regionMap());
            modelBuilder.Configurations.Add(new stream_access_pointMap());
            modelBuilder.Configurations.Add(new roadMap());
            modelBuilder.Configurations.Add(new road_typeMap());
        }
    }
}
