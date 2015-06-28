using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace TroutStreamMangler.MN.Models
{
    public partial class MinnesotaShapeDataContext : DbContext
    {
        static MinnesotaShapeDataContext()
        {
        }

        public MinnesotaShapeDataContext()
            : base("Name=mn_import")
        {
            Database.SetInitializer<MinnesotaShapeDataContext>(null);
        }

        // PAL
        public DbSet<dnr_wildlife_management_area_boundaries_publicly_accessible>
            dnr_wildlife_management_area_boundaries_publicly_accessible { get; set; }

        public DbSet<state_forest_management_units>
            state_forest_management_units { get; set; }

        public DbSet<dnr_stat_plan_areas_prk> dnr_stat_plan_areas_prk { get; set; }
        public DbSet<mndnr_fisheries_acquisition> mndnr_fisheries_acquisition { get; set; }

        // Restrictions
        public DbSet<strm_regsln3> strm_regsln3 { get; set; }

        // Streams
        public DbSet<StreamRoute> StreamRoute { get; set; }

        public DbSet<dnr_hydro_features_all> Lakes { get; set; }

        // Trout Stream Sections
        public DbSet<trout_streams_minnesota> trout_streams_minnesota { get; set; }
    }
}