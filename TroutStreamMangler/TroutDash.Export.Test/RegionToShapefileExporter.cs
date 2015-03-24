using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TroutDash.DatabaseImporter.Convention;
using TroutDash.EntityFramework.Models;

namespace TroutDash.Export.Test
{
    public class RegionToShapefileExporter
    {
        private readonly TroutDashPrototypeContext _context;
        private readonly DirectoryInfo _rootDirectory;
        private readonly IEnumerable<string> _shapes;
        private readonly IDatabaseConnection _dataseConnection;

        public RegionToShapefileExporter(TroutDashPrototypeContext context, DirectoryInfo rootDirectory, IEnumerable<string> shapes, IDatabaseConnection dataseConnection)
        {
            _context = context;
            _rootDirectory = rootDirectory;
            _shapes = shapes;
            _dataseConnection = dataseConnection;
        }

        public void Export()
        {
            var regions = _context.regions.ToList();
            CreateRegion();


            foreach (var region in regions)
            {
                // create a region directory
                var folder = _rootDirectory.FullName + "\\" + region.name;
                var currentDirectory = Directory.Exists(folder) == false 
                    ? Directory.CreateDirectory(folder) 
                    : new DirectoryInfo(folder);

                foreach (var shapeName in _shapes)
                {
                    string lake;
                    if (shapeName == "county")
                    {
                        lake = CreateCountyShapeFromRegionId(region.gid);
                    }

                    else if (shapeName == "lake")
                    {
                        lake = CreateLakeShapefileFromRegionId(region.gid);
                    }
                    else
                    {
                        lake = CreateShapeQueryForRegion(region.gid, shapeName);
                    }
                    
                    var command = CreateCommand(lake, region.name, shapeName);
                    ExecuteShellCommand.ExecuteProcess(command, _rootDirectory);
                    CreateGeoJsonScript(currentDirectory, shapeName, lake);
                }
            }
        }

        private void CreateRegion()
        {
            var regionDirectoryName = "Regions";
            var regionFolder = _rootDirectory.FullName + "\\" + regionDirectoryName;
            if (Directory.Exists(regionFolder) == false)
            {
                Directory.CreateDirectory(regionFolder);
            }
            var regionSql = CreateRegionShapefiles();
            var c = CreateCommand(regionSql, regionDirectoryName, "region");
            ExecuteShellCommand.ExecuteProcess(c, _rootDirectory);

            var stateSql =
                @"select distinct state.* from public.state state, public.region region where ST_Contains(state.geom, region.geom)";
            var stateCommand = CreateCommand(stateSql, regionDirectoryName, "state");
            ExecuteShellCommand.ExecuteProcess(stateCommand, _rootDirectory);

            var countySql = @"select county.* from public.county county where region_id is not null;";
            var countyCommand = CreateCommand(countySql, regionDirectoryName, "county");
            ExecuteShellCommand.ExecuteProcess(countyCommand, _rootDirectory);
            var targetDirectory = new DirectoryInfo(regionFolder);

            CreateGeoJsonScript(targetDirectory, "region", regionSql);
            CreateGeoJsonScript(targetDirectory, "state", stateSql);
            CreateGeoJsonScript(targetDirectory, "county", countySql);
        }

        private void CreateGeoJsonScript(DirectoryInfo target, string name, string sql)
        {
            var file = String.Format("{0}.geojson", name);
            File.Delete(target.FullName + "\\" + file);
            const string command2 = @"ogr2ogr -f ""GeoJSON"" {0} -preserve_fid PG:""host={1} user={2} dbname={3}"" -sql ""{4}""";
//            var command = "ogr2ogr -f GeoJSON -preserve_fid {1} {0}.shp";
            var completeCommand = string.Format(command2, file, _dataseConnection.HostName, _dataseConnection.UserName, _dataseConnection.DatabaseName, sql);

            ExecuteShellCommand.ExecuteProcess(completeCommand, target);
        }

        private string CreateCommand(string force, string regionName, string shapeName)
        {
            var HostName = _dataseConnection.HostName;
            var UserName = _dataseConnection.UserName;
            var DatabaseName = _dataseConnection.DatabaseName;
//            var Password = "fakepassword";
            var path = Path(regionName, shapeName);
//            var forceCommand = String.Format(@"pgsql2shp -f {4} -h {0} -u {1} -P {5} {2} ""{3}""", HostName,
//                UserName, DatabaseName, force, path, Password);
            var forceCommand = String.Format(@"pgsql2shp -f {4} -h {0} -u {1} {2} ""{3}""", HostName,
                UserName, DatabaseName, force, path);
            return forceCommand;
        }

        private static string Path(string regionName, string shapeName)
        {
            var path = "./" + regionName + "/" + shapeName + ".shp";
            return path;
        }

        private string CreateLakeShapefileFromRegionId(int regionId)
        {
            const string sql =
                @"SELECT lake.* FROM public.lake lake, public.region region, public.stream stream where region.gid = {0} and ST_Intersects(stream.geom, region.geom) and ST_Intersects(lake.geom, region.geom) and ST_Intersects(lake.geom, stream.geom);";
            var result = string.Format(sql, regionId);
            return result;
        }

        private string CreateShapeQueryForRegion(int regionId, string tableName)
        {
            const string sql = @"SELECT {0}.* FROM public.{0} {0}, public.region region where region.gid = {1} and ST_Intersects( {0}.geom, region.geom);";
            var result = string.Format(sql, tableName, regionId);
            return result;
        }

        private string CreateCountyShapeFromRegionId(int regionId)
        {
            const string sql = @"SELECT county.* FROM public.county county where county.region_id = {0}";
            var result = string.Format(sql, regionId);
            return result;
        }

        private string CreateRegionShapefiles()
        {
            const string sql = @"SELECT region.* FROM public.region;";
            var result = string.Format(sql);
            return result;
        }
    }
}