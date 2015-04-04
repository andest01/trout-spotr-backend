topojson  ./streamDetails.json ./restriction_route.geojson ./trout_stream_section.geojson  -p -o ./streams.json 

topojson ./publicly_accessible_land.geojson ./streams.json -p -o package.json