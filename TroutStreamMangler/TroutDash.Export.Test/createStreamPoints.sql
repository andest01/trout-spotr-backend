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
   ORDER BY gid) AS RESULT