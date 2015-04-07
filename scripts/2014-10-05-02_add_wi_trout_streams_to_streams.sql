INSERT INTO public."Stream"
(name, local_name, length_mi, public_length_mi, centroid_latitude, centroid_longitude, has_brown_trout, 
            has_brook_trout, has_rainbow_trout, is_brown_trout_stocked, is_brook_trout_stocked, 
            is_rainbow_trout_stocked, huc_subregion, status_message, state, source, geom)
    SELECT 
  stream.official_n as name,
 stream.local_name as local_name, 
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
 'WI' as state,
 'WI DNR' as source,
 stream.geom
FROM 
  public."wisconsin_trout_streams" stream
