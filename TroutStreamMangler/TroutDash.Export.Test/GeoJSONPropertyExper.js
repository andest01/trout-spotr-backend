var fs = require('fs');
debugger;
var streamGeoJSON = fs.readFileSync('stream.geojson', 'utf-8');
var secondFile = fs.readFileSync('streamDetails.json', 'utf-8');

var firstJSON = JSON.parse(streamGeoJSON);
var secondJSON = JSON.parse(secondFile);

debugger; 

var dictionary = {};
for (var i = 0; i < secondJSON.length; i++) {
	var item = secondJSON[i];
	var id = item.gid;
	dictionary['' + id] = item;
}



var mergedJSON = {};

// Do your thing

fs.writeFileSync('someFilePath', JSON.stringify(mergedJSON));