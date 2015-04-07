var fs = require('fs');
debugger;
var streamGeoJSONFile = process.argv[2];
var streamDetailsJSONFile = process.argv[3];
debugger;
var streamGeoJSON = fs.readFileSync(streamGeoJSONFile, 'utf-8');
var secondFile = fs.readFileSync(streamDetailsJSONFile, 'utf-8');
debugger;

var firstJSON = JSON.parse(streamGeoJSON);
var secondJSON = JSON.parse(secondFile);


var dictionary = {};
for (var i = 0; i < secondJSON.length; i++) {
    var item = secondJSON[i];
    var id = item.Id;
    dictionary['' + id] = item;
}

for (var j = 0; j < firstJSON.features.length; j++) {
    var feature = firstJSON.features[j];
    var propertiesId = feature.properties.gid;
    var newProperty = dictionary[propertiesId];
    feature.properties = newProperty;
}

var mergedJSON = {};

// Do your thing

fs.writeFileSync('streamProperties.json', JSON.stringify(firstJSON));