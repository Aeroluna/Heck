'use strict'

const fs = require('fs');

const INPUT = "ExpertPlusStandard.dat"
const OUTPUT = "ExpertStandard.dat"

let difficulty = JSON.parse(fs.readFileSync(INPUT));


//#region this just counts how many time you ran it for fun, feel free to remove.
if(!(fs.existsSync("count.txt"))) {
    fs.writeFileSync("count.txt", parseInt("0").toString())
}
let count = parseInt(fs.readFileSync("count.txt"))
count++
fs.writeFileSync("count.txt", count.toString())
console.log("GIVE IT UP FOR RUN " + count)
//#endregion


difficulty._customData = { _pointDefinitions: [], _customEvents: [] };

const _customData = difficulty._customData;
const _obstacles = difficulty._obstacles;
const _notes = difficulty._notes;
const _customEvents = _customData._customEvents;
const _pointDefinitions = _customData._pointDefinitions;

let filterednotes

_obstacles.forEach(wall => {
    if (!wall._customData) {
        wall._customData = {}
    }
})

_notes.forEach(note => {
    if (!note._customData) {
        note._customData = {}
    }
})
//#region helper functions
function round(value, decimals) {
  return Number(Math.round(value+'e'+decimals)+'e-'+decimals);
}
function getJumps(njs, offset) {
    const _startHalfJumpDurationInBeats = 4
    const _maxHalfJumpDistance = 18
    const _startBPM = 170
    const bpm = 170
    const _startNoteJumpMovementSpeed = njs
    const _noteJumpStartBeatOffset = offset

    let _noteJumpMovementSpeed = (_startNoteJumpMovementSpeed * bpm) / _startBPM
    let num = 60 / bpm
    let num2 = _startHalfJumpDurationInBeats
    while (_noteJumpMovementSpeed * num * num2 > _maxHalfJumpDistance) {
        num2 /= 2
    }
    num2 += _noteJumpStartBeatOffset
    if (num2 < 1) {
        num2 = 1
    }
    const _jumpDuration = num * num2 * 2
    const _jumpDistance = _noteJumpMovementSpeed * _jumpDuration
    return {half: num2, dist: _jumpDistance}
}

function offestOnNotesBetween(p1,p2,offset) {
	filterednotes = _notes.filter(n => n._time >= p1 && n._time <= p2)
	filterednotes.forEach(object =>
		{
			//always worth having.
			//man this shit BETTER not be undefined.
			if(typeof offset !== "undefined") {object._customData._noteJumpStartBeatOffset = offset;}

		})
	return filterednotes;
}

function lerp(v0,v1,t) {
	return v0*(1-t)+v1*t;
}
function trackOnNotesBetween(track, p1,p2,potentialOffset) {
  filterednotes = _notes.filter(n => n._time >= p1 && n._time <= p2)
  filterednotes.forEach(object =>
  {
    object._customData._track = track;
    if(typeof potentialOffset !== "undefined") {object._customData._noteJumpStartBeatOffset = potentialOffset;}

  })
  return filterednotes;
}
//applies a track to notes on two tracks between two times based on the color of the notes
//IT GONNA FUCK UP WITH BOMBS I TELL YOU HWAT BOI
//red, blue, p1, p2, potentialOffset
function trackOnNotesBetweenRBSep(trackR, trackB, p1, p2, potentialOffset){
	filterednotes = _notes.filter(n => n._time >= p1 && n._time <= p2)
	filterednotes.forEach(object =>
		{
			if(typeof potentialOffset !== "undefined") {object._customData._noteJumpStartBeatOffset = potentialOffset;}
			if(object._type == 0) {object._customData._track = trackR};
			if(object._type == 1) {object._customData._track = trackB};
	  
		})
	return filterednotes;
}
//p1, p2, potentialoffset, up, down, left, right, 
//TODO: ADD OTHER DIRS
function trackOnNotesBetweenDirSep(p1, p2, potentialOffset, trackUp, trackDown, trackLeft, trackRight) {
	filterednotes = _notes.filter(n => n._time >= p1 && n._time <= p2)
	filterednotes.forEach(object =>
		{
			if(object._cutDirection == 0 && typeof trackUp !== "undefined") {object._customData._track = trackUp}
			if(object._cutDirection == 1 && typeof trackUp !== "undefined") {object._customData._track = trackDown}
			if(object._cutDirection == 2 && typeof trackUp !== "undefined") {object._customData._track = trackLeft}
			if(object._cutDirection == 3 && typeof trackUp !== "undefined") {object._customData._track = trackRight}
			//i might want to make this only run if I assign a track...
			if(typeof potentialOffset !== "undefined") {object._customData._noteJumpStartBeatOffset = potentialOffset;}

		})
	return filterednotes;
}

//#endregion

//#region use this area to do your stuff

//#region 4-10 _position demo

trackOnNotesBetween("firstPositionDemo", 4, 20) //this function achieves the same as the lines below, its just a simpler/easier way to do so
filterednotes = _notes.filter(n=> n._time >= 4 && n._time <= 25)
filterednotes.forEach(note => {
	note._customData._track = "firstPositionDemo"
})

//push demo point definitions
_pointDefinitions.push({
	"_name":"examplePositionPointDef",
	"_points":[
		[0,0,0,0],
		[0,5,0,0.5,"splineCatmullRom"],
		[0,0,0,1,"splineCatmullRom"]
	  ]
}, {
	"_name":"examplePositionPath",
	"_points":[
		[0,0,0,0],
		[0,5,0,0.25,"splineCatmullRom"],
		[0,0,0,0.5,"splineCatmullRom"]
	  ]
})

//animate track
_customEvents.push({
	"_time":4,
	"_type":"AnimateTrack",
	"_data":{
		"_track":"firstPositionDemo",
		"_position":"examplePositionPointDef",
		"_duration":8
	}
})
//AssignPath
_customEvents.push({
	"_time":12,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"firstPositionDemo",
		"_position":"examplePositionPath",
		"_duration":4,
		"_easing":"easeInBounce"
	}
}, {
	"_time":16,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"firstPositionDemo",
		"_position":[[0,0,0,0],[0,0,0,1]],
		"_duration":4,
		"_easing":"easeOutBounce"
	}
})
//#endregion

//#region _localRotation 20-40

trackOnNotesBetween("localRotationDemo", 20, 40)
_pointDefinitions.push({
	"_name":"localSpinDemoAnimate",
	"_points":[
		[0,0,0,0],
		[90,0,0,0.25],
		[180,0,0,0.5],
		[270,0,0,0.75],
		[360,0,0,1]
	  ]
}, {
	"_name":"localSpinDemoAnimateRev",
	"_points":[
		[0,0,0,0],
		[-90,0,0,0.25],
		[-180,0,0,0.5],
		[-270,0,0,0.75],
		[-360,0,0,1]
	  ]
}, {
	"_name":"localSpinDemoPath",
	"_points":[
		[0,0,0,0],
		[0,0,90,0.125],
		[0,0,180,0.25],
		[0,0,270,0.375],
		[0,0,360,0.5]
	  ]
}
)
//AnimateTrack,
_customEvents.push({
	"_time":20,
	"_type":"AnimateTrack",
	"_data":{
		"_track":"localRotationDemo",
		"_duration":5,
		"_localRotation":"localSpinDemoAnimate",
		"_easing":"easeInOutExpo"
	}
}, {
	"_time":25,
	"_type":"AnimateTrack",
	"_data":{
		"_track":"localRotationDemo",
		"_duration":5,
		"_localRotation":"localSpinDemoAnimateRev",
		"_easing":"easeInOutExpo"
	}
}, {
	"_time":30,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"localRotationDemo",
		"_duration":0,
		"_localRotation":"localSpinDemoPath"
	}
})


//#endregion

//#region _rotation 40-60

trackOnNotesBetween("RotationDemo", 40, 60)
_pointDefinitions.push({
	"_name":"RotationPointsAnimate",
	"_points":[
		[0,0,0,0],
		[0,90,0,0.25],
		[0,180,0,0.5],
		[0,270,0,0.75],
		[0,360,0,1]
	]
}, {
	"_name":"RotationPointsPath",
	"_points":[
		[0,0,0,0],
		[0,45,0,0.125, "splineCatmullRom"],
		[0,-45,0,0.25,"splineCatmullRom"],
		[0,22.5,0,0.375,"splineCatmullRom"],
		[0,-22.5,0,0.5,"splineCatmullRom"],
		[0,0,0,0.625,"splineCatmullRom"]
	]
}
)
//AnimateTrack
_customEvents.push({
	"_time":40,
	"_type":"AnimateTrack",
	"_data":{
		"_track":"RotationDemo",
		"_rotation":"RotationPointsAnimate",
		"_duration":10
	}
},
 {
	"_time":50,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"RotationDemo",
		"_rotation":"RotationPointsPath",
		"_duration":5
	}
}, {
	"_time":55,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"RotationDemo",
		"_rotation":[[0,0,0,0]],
		"_duration":5
	}
})

//#endregion

//#region _dissolve 60-80

trackOnNotesBetween("dissolveDemo", 60, 80)
_pointDefinitions.push({
	"_name":"dissolveDemoAnimate",
	"_points":[
		[1,0],
		[0,0.25],
		[0.5,0.50],
		[0,0.75],
		[1,1]
	]
}, {
	"_name":"dissolveDemoPath",
	"_points":[
		[0,0],
		[1,0.125],
		[1, 0.30],
		[0,0.35]
	]
}
)

_customEvents.push({
	"_time":60,
	"_type":"AnimateTrack",
	"_data":{
		"_track":"dissolveDemo",
		"_dissolve":"dissolveDemoAnimate",
		"_duration":10
	}
}, {
	"_time":70,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"dissolveDemo",
		"_dissolve":"dissolveDemoPath",
		"_duration":0
	}
})
//#endregion

//#region _dissolve 80-100
trackOnNotesBetween("dissolveArrowDemo", 80, 100)

_pointDefinitions.push({
	"_name":"dissolveArrowDemoAnimate",
	"_points":[
		[1,0],
		[0,1]
	]
}, {
	"_name":"dissolveArrowDemoPath",
	"_points":[
		[0,0.10],
		[1,0.20],
		[1, 0.30],
		[0,0.35]
	]
}
)
_customEvents.push({
	"_time":80,
	"_type":"AnimateTrack",
	"_data":{
		"_track":"dissolveArrowDemo",
		"_dissolveArrow":"dissolveArrowDemoAnimate",
		"_duration":5
	}
}, {
	"_time":85,
	"_type":"AnimateTrack",
	"_data":{
		"_track":"dissolveArrowDemo",
		"_dissolveArrow":[[0,0],[1,1]],
		"_duration":5
	}
},
 {
	"_time":90,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"dissolveArrowDemo",
		"_dissolveArrow":"dissolveArrowDemoPath"
	}
})

//#endregion

//#region color walls 100-120

_obstacles.push({
	"_time":100,
	"_duration":9,
	"_lineindex":0,
	"_type":0,
	"_width":1,
	"_customData":{
		"_track":"LeftColorWall"
	}
}, {
	"_time":100,
	"_duration":9,
	"_lineindex":3,
	"_type":0,
	"_width":1,
	"_customData":{
		"_track":"RightColorWall"
	}
})
_pointDefinitions.push({
	"_name":"RightColorWallAnimate",
	"_points":[
		[1,0,0,1,0.2],
		[0,1,0,1,0.4],
		[0,0,1,1,0.6],
		[0,1,1,1,0.8],
		[1,1,1,1,1],
	]
}, {
	"_name":"LeftColorWallAnimate",
	"_points":[
		[1,0,0,0,0.2],
		[0,1,0,0,0.4],
		[0,0,1,0,0.6],
		[0,1,1,0,0.8],
		[1,1,1,0,1],
	]
}
)

_customEvents.push({
	"_time":98,
	"_type":"AnimateTrack",
	"_data":{
		"_track":"RightColorWall",
		"_color":"RightColorWallAnimate",
		"_duration":10
	}
}, {
	"_time":98,
	"_type":"AnimateTrack",
	"_data":{
		"_track":"LeftColorWall",
		"_color":"LeftColorWallAnimate",
		"_duration":10
	}
}
)
for (let i = 0; i < 400; i++) {
	let startTime = 110;
	let interval = 0.025;
	let duration = 0.01;
	
	_obstacles.push({
		"_time":startTime+(interval*i),
		"_duration":duration,
		"_lineindex":0,
		"_type":1,
		"_width":1,
		"_customData":{
			"_track":"LeftColorWallStatic"
		}
	}, {
		"_time":startTime+(interval*i),
		"_duration":duration,
		"_lineindex":3,
		"_type":1,
		"_width":1,
		"_customData":{
			"_track":"RightColorWallStatic"
		}
	})
}

_pointDefinitions.push({
	"_name":"GradientPathOne",
	"_points":[
		[1,0,0,0.5,0.0416],
		[0,1,0,0.5,0.0832],
		[0,0,1,0.5,0.1248],
		[1,0,0,0.5,0.1664],
		[0,1,0,0.5,0.208],
		[0,0,1,0.5,0.2496],
		[1,0,0,0.5,0.2912],
		[0,1,0,0.5,0.3328],
		[0,0,1,0.5,0.3743],
		[1,0,0,0.5,0.416],
		[0,1,0,0.5,0.4576],
		[0,0,1,0.5,0.4992]
	]
}, {
	"_name":"GradientPathTwo",
	"_points":[
		[0,1,0,0.5,0.0416],
		[0,0,1,0.5,0.0832],
		[1,0,0,0.5,0.1248],
		[0,1,0,0.5,0.1664],
		[0,0,1,0.5,0.208],
		[1,0,0,0.5,0.2496],
		[0,1,0,0.5,0.2912],
		[0,0,1,0.5,0.3328],
		[1,0,0,0.5,0.3743],
		[0,1,0,0.5,0.416],
		[0,0,1,0.5,0.4576],
		[1,0,0,0.5,0.4992]
	]
}
)

_customEvents.push(
	{
	"_time":110,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"RightColorWallStatic",
		"_color":"GradientPathOne",
		"_duration":2
	}
}, {
	"_time":114,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"RightColorWallStatic",
		"_color":"GradientPathTwo",
		"_duration":6,
		"_easing":"easeOutElastic",
		
	}
}, {
	"_time":110,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"LeftColorWallStatic",
		"_color":"GradientPathTwo",
		"_duration":2
	}
}, {
	"_time":114,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"LeftColorWallStatic",
		"_color":"GradientPathOne",
		"_duration":6,
		"_easing":"easeOutElastic"
}
})
//#endregion

//#region definitePosition 130-150

trackOnNotesBetween("definitePosDemo", 130, 150)
_pointDefinitions.push({
	"_name":"defPosPath",
	"_points":[
		[0, 0, 20, 0],
		[10, 0, 20, 0.1],
		[10, 10, 20, 0.2],
		[0, 10, 20, 0.3],
		[0, 0, 20, 0.4],
		[0, 0, 10, 0.5],
		[-20, 0, 10, 1.0]
	  ]
}, {
	"_name":"defPosNormal",
	"_points":[
		[0,0,23,0],
		[0,0,0,0.5],
		[0,0,-23,1]
	]
}
)

_customEvents.push({
	"_time":132,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"definitePosDemo",
		"_definitePosition":"defPosPath",
		"_duration":3
	}
}, {
	"_time":0,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"definitePosDemo",
		"_definitePosition":"defPosNormal",
		"_duration":0
	}
})
//#endregion
//#region time 155 (this example is bad, you basically need to script time. Better example coming someday)
_pointDefinitions.push({
	"_name":"SingleNoteTime",
	"_points":[
		[0,0],
		[0.45, 0.15],
		[0.15, 0.30],
		[0.5, 0.5],
		[1,1]
	]
})
trackOnNotesBetween("singleNoteTimeTrack", 155, 155)
_customEvents.push({
	"_time":153,
	"_type":"AnimateTrack",
	"_data":{
		"_time":"SingleNoteTime",
		"_duration":10,
		"_track":"singleNoteTimeTrack"
	}
})
//#endregion

//#region write file
const precision = 4 //decimals to round to

const jsonP = Math.pow(10, precision)
const sortP = Math.pow(10, 2)
function deeperDaddy(obj) {
	if (obj)
		for (const key in obj) {
			if (obj[key] == null) {
				delete obj[key]
			} else if (typeof obj[key] === 'object' || Array.isArray(obj[key])) {
				deeperDaddy(obj[key])
			} else if (typeof obj[key] == 'number') {
				obj[key] = parseFloat(Math.round((obj[key] + Number.EPSILON) * jsonP) / jsonP)
			}
		}
}
deeperDaddy(difficulty)

difficulty._notes.sort(
	(a, b) =>
		parseFloat(Math.round((a._time + Number.EPSILON) * sortP) / sortP) - parseFloat(Math.round((b._time + Number.EPSILON) * sortP) / sortP) ||
		parseFloat(Math.round((a._lineIndex + Number.EPSILON) * sortP) / sortP) - parseFloat(Math.round((b._lineIndex + Number.EPSILON) * sortP) / sortP) ||
		parseFloat(Math.round((a._lineLayer + Number.EPSILON) * sortP) / sortP) - parseFloat(Math.round((b._lineLayer + Number.EPSILON) * sortP) / sortP)
)
difficulty._obstacles.sort((a, b) => a._time - b._time)
difficulty._events.sort((a, b) => a._time - b._time)

fs.writeFileSync(OUTPUT, JSON.stringify(difficulty, null, 0));

//#endregion