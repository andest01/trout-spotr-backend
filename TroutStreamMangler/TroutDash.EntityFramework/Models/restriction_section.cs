namespace TroutDash.EntityFramework.Models
{
    public partial class restriction_section
    {
        public int id { get; set; }
        public double start { get; set; }
        public double stop { get; set; }
        public int restriction_id { get; set; }
        public int stream_gid { get; set; }
        public virtual restriction restriction { get; set; }
        public virtual stream Stream { get; set; }
    }
}
