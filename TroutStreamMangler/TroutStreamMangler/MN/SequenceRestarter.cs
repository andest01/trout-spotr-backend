using System;
using Npgsql;
using TroutDash.DatabaseImporter.Convention;

namespace TroutStreamMangler.MN
{
    public class CentroidResetter
    {
        private const string Script = @"update public.stream 
set (centroid_latitude, centroid_longitude) = (centroidResults.latitude, centroidResults.longitude)
from (


SELECT gid,
       name,
       geom,
       ST_y(ST_Transform(geom, 4326)) AS latitude,
       ST_X(ST_Transform(geom, 4326)) AS longitude
FROM
  (SELECT stream.gid,
          stream.name,
          st_closestpoint(stream.geom, st_centroid(trout_section.geom)) AS geom
   FROM stream stream,

     (SELECT trout_section.stream_gid,
             ST_Centroid(ST_Multi(ST_Union(trout_section.geom))) AS geom
      FROM public.trout_stream_section trout_section
      GROUP BY trout_section.stream_gid) AS trout_section
   WHERE stream.gid = trout_section.stream_gid
   ORDER BY gid) AS RESULT) as centroidResults
  where stream.gid = centroidResults.gid";

        private readonly IDatabaseConnection _dbConnection;

        public CentroidResetter(IDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public void RestartSequence()
        {

            var sql = Script;
            var connection = String.Format("Server={1};Port=5432;User Id={0};Database={2};Password=fakepassword;", _dbConnection.UserName, _dbConnection.HostName, _dbConnection.DatabaseName);
            var conn = new NpgsqlConnection(connection);
            conn.Open();

            var command = new NpgsqlCommand(sql, conn);


            try
            {
                command.ExecuteNonQuery();

            }

            catch
            {
                return;
            }
        }
    }

    public class SequenceRestarter
    {
        private readonly IDatabaseConnection _dbConnection;

        public SequenceRestarter(IDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public void RestartSequence(string sequenceName)
        {

            var sql = "ALTER SEQUENCE " + sequenceName + " RESTART WITH 1;";
            var connection = String.Format("Server={0};Port=5432;User Id={1};Database={2};Password=fakepassword;", _dbConnection.HostName, _dbConnection.UserName, _dbConnection.DatabaseName);
            var conn = new NpgsqlConnection(connection);
            conn.Open();

            var command = new NpgsqlCommand(sql, conn);


            try
            {
                command.ExecuteNonQuery();

            }

            catch
            {
                return;
            }
        }
    }
}