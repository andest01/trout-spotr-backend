INSERT INTO public."Stream"
(name, local_name, length_mi, public_length_mi, centroid_latitude, centroid_longitude, has_brown_trout, 
            has_brook_trout, has_rainbow_trout, is_brown_trout_stocked, is_brook_trout_stocked, 
            is_rainbow_trout_stocked, huc_subregion, status_message, state, source, geom)
    SELECT 
  stream.kittle_nam as name,
 '' as local_name, 	
 st_length(stream.geom, true) * 0.000621371 as length_mi,
 0 as public_length_mi,
 st_y(st_centroid(stream.geom)) as centroid_latitude,
  st_x(st_centroid(stream.geom)) as centroid_longitude,
 true as has_brown_trout,
 true as has_brook_trout,
 true as has_rainbow_trout,
 true as is_brown_trout_stocked,
 true as is_brook_trout_stocked,
 true as is_rainbow_trout_stocked, 
 '1231' as huc_subregion,
 'flood' as status_message,
 'MN' as state,
 'MN DNR' as source,
 stream.geom
FROM 
  public."mn_trout_streams_4326" stream

WHERE (stream.trout_flag = 1::numeric OR stream.kittle_nam::text !~~ '%Unnamed%'::text) AND stream.length > 0.2::double precision;
