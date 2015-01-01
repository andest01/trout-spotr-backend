using System;
using System.ComponentModel.DataAnnotations.Schema;
using TroutDash.EntityFramework.Models;

namespace TroutStreamMangler.MN.Models
{
    public enum use_reg_types
    {
        NA = 0,
        AnglingOnly = 1,
        GeneralUse = 2,
        RestrictedUse = 3,
        NoAccess = 4
    }

    [Table("mndnr_fisheries_acquisition", Schema = "public")]
    public class mndnr_fisheries_acquisition : GeometryExtended
    {
        public const string Restricteduse = "Restricted Use";
        public const string AnglingOnly = "Angling Only";
        public const string GeneralUse = "General Use";
        public const string NoAccess = "NO ACCESS";

        [Column("id")]
        public int id { get; set; }

        // WMA0900901
        [Column("unit_name")]
        public string unit_name { get; set; }

        // Whitewater WMA
        [Column("unit_type")]
        public string unit_type { get; set; }

        [Column("admin_unit")]
        public string admin_unit { get; set; }

        [Column("use_reg")]
        public string use_reg { get; set; }

        [Column("trout_strm")]
        public string trout_strm { get; set; }

        public bool IsTroutStream
        {
            get
            {
                // there's one entry in there that's null which is indeed for trout. :)
                return string.IsNullOrWhiteSpace(trout_strm) ||
                       string.Equals(trout_strm, "Y", StringComparison.OrdinalIgnoreCase);
            }
        }

        public use_reg_types use_reg_type
        {
            get
            {
                if (string.Equals(use_reg, Restricteduse, StringComparison.OrdinalIgnoreCase))
                {
                    return use_reg_types.RestrictedUse;
                }

                if (string.Equals(use_reg, AnglingOnly, StringComparison.OrdinalIgnoreCase))
                {
                    return use_reg_types.AnglingOnly;
                }

                if (string.Equals(use_reg, GeneralUse, StringComparison.OrdinalIgnoreCase))
                {
                    return use_reg_types.GeneralUse;
                }

                if (string.Equals(use_reg, NoAccess, StringComparison.OrdinalIgnoreCase))
                {
                    return use_reg_types.NoAccess;
                }

                return use_reg_types.NA;
            }
        }

        // M-009-017
        [Column("kittle_no")]
        public string kittle_no { get; set; }

        [Column("kittname")]
        public string kittname { get; set; }
    }
}