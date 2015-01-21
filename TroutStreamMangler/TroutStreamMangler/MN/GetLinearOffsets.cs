using System;
using System.Collections.Generic;
using Npgsql;

namespace TroutStreamMangler.MN
{
    public class GetLinearOffsets
    {
        public GetLinearOffsets()
        {
        }

        public IEnumerable<Tuple<decimal, decimal>> GetStuff(string streamId, string geometryTable, int geometryId, string geometryIdColumnName)
        {
            const string linearReferenceString =
                @"SELECT (St_dump(St_intersection(palbuffer, strouter.geom))).geom                                                                 as thegeom,
       st_linelocatepoint(st_linemerge(strouter.geom), st_startpoint((st_dump(st_intersection(palbuffer, strouter.geom))).geom)) AS start,
       st_linelocatepoint(st_linemerge(strouter.geom), st_endpoint((st_dump(st_intersection(palbuffer, strouter.geom))).geom))   AS stop
FROM   streams_with_measured_kittle_routes strouter, 
       ( 
              SELECT st_buffer(st_union(dumpgeom), 3, 'endcap=flat join=round') AS palbuffer 
              FROM   ( 
                            SELECT pal.geom                        AS dumpgeom 
                            FROM   {1}        AS pal,
                            streams_with_measured_kittle_routes routes
                            WHERE  ST_Intersects(pal.geom, routes.geom) 
                            and routes.kittle_nbr = '{0}') AS DUMP) AS palbuffer
WHERE  strouter.kittle_nbr = '{0}'
";

            NpgsqlConnection conn =
                new NpgsqlConnection(
                    "Server=localhost;Port=5432;User Id=postgres;Password=fakepassword;Database=mn_import;");
            conn.Open();

            var sql = string.Format(linearReferenceString, streamId, geometryTable);


            NpgsqlCommand command = new NpgsqlCommand(sql, conn);


            try
            {
                NpgsqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    var start = Convert.ToDecimal(dr[1]);
                    var stop = Convert.ToDecimal(dr[2]);

                    yield return new Tuple<decimal, decimal>(start, stop);

                }

            }

            finally
            {
                conn.Close();
            }
        }

        public IEnumerable<Tuple<decimal, decimal>> GetStuff(string streamId, string geometryTable)
        {
            const string linearReferenceString =
                @"SELECT (St_dump(St_intersection(palbuffer, strouter.geom))).geom                                                                 as thegeom,
       st_linelocatepoint(st_linemerge(strouter.geom), st_startpoint((st_dump(st_intersection(palbuffer, strouter.geom))).geom)) AS start,
       st_linelocatepoint(st_linemerge(strouter.geom), st_endpoint((st_dump(st_intersection(palbuffer, strouter.geom))).geom))   AS stop
FROM   streams_with_measured_kittle_routes strouter, 
       ( 
              SELECT st_buffer(st_union(dumpgeom), 3, 'endcap=flat join=round') AS palbuffer 
              FROM   ( 
                            SELECT pal.geom                        AS dumpgeom 
                            FROM   {1}        AS pal,
                            streams_with_measured_kittle_routes routes
                            WHERE  ST_Intersects(pal.geom, routes.geom) 
                            and routes.kittle_nbr = '{0}') AS DUMP) AS palbuffer
WHERE  strouter.kittle_nbr = '{0}'
";

            NpgsqlConnection conn =
                new NpgsqlConnection(
                    "Server=localhost;Port=5432;User Id=postgres;Password=fakepassword;Database=mn_import;");
            conn.Open();

            var sql = string.Format(linearReferenceString, streamId, geometryTable);


            NpgsqlCommand command = new NpgsqlCommand(sql, conn);


            try
            {
                NpgsqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    var start = Convert.ToDecimal(dr[1]);
                    var stop = Convert.ToDecimal(dr[2]);

                    yield return new Tuple<decimal, decimal>(start, stop);

                }

            }

            finally
            {
                conn.Close();
            }
        }
    }
}