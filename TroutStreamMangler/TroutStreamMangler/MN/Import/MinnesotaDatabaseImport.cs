using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroutDash.DatabaseImporter;

namespace TroutStreamMangler.MN.Import
{
    public class MinnesotaDatabaseImport : DatabaseImporter
    {
        public MinnesotaDatabaseImport(IDbConnection connection, ITableImporterManifest tableManifest) : base(connection, tableManifest)
        {

        }

        protected override IEnumerable<FileInfo> SpatialReferenceSystemScripts()
        {
            Console.WriteLine("Adding custom spatial reference systems...");
            var file =
                new FileInfo(
                    @"C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\MN\SridImport.sql");
            return file == null 
                ? new FileInfo[0] 
                : new [] { file };
        }
    }
}
