using System.Data.Entity;

namespace TroutStreamMangler.US.Models
{
    public partial class UsShapeDataContext : DbContext
    {
        static UsShapeDataContext()
        {
        }

        public UsShapeDataContext()
            : base("Name=us_import")
        {
            Database.SetInitializer<UsShapeDataContext>(null);
        }

        public DbSet<TroutStreamMangler.US.Models.county> counties { get; set; }
        public DbSet<TroutStreamMangler.US.Models.state> states { get; set; }
    }
}