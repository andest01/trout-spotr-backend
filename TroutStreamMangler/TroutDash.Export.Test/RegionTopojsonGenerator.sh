topojson  ./region.geojson ./state.geojson ./county.geojson  -p -o ./regions.topo.json
mapshaper ./regions.topo.json -simplify visvalingam 5% -o regions.min.topo.json
topojson ./streamProperties.json ./regions.min.topo.json -p -o state.topo.json