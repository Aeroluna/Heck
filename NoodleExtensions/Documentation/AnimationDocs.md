# Animation
This document covers animation for both Noodle Extensions and Chroma.

Note: any mention of easing supports anything seen at https://easings.net/

You can also use "splineCatmullRom" in pointDefinitions as a simple spline implementation

You can find the script and map used to generate all the examples in the documentation [Here](examples/documentationMap)

# Custom Events
All of the below fields are stored inside the `_customData` of your difficulty dat file.

Custom event's are stored inside the `_customEvents` field of your `_customData`, 
```js
"_version": "2.0.0",
"_customData": {
  "_customEvents": [
    {
      "_time": float,
      "_type": "string",
      "_data": {
        "foo": 1.0,
        "message": "Hello from a custom event!"
      }
    }
  ]
}
"_events": [],
"_notes": [],
"_obstacles":[]
```
## Events
- [`AnimateTrack`](#AnimateTrack)
- [`AssignPathAnimation`](#AssignPathAnimation)
- [`AssignTrackParent`](#AssignTrackParent)
- [`AssignPlayerToTrack`](#AssignPlayerToTrack)




# Point Definitions
Point definitions are used to describe what happens over the course of an animation, they are used **slightly differently for different properties.** They consist of an array of points.

Point definitions can be defined inside the `_pointDefinitions` field of your `_customData`, any point definition defined here can be called via their `_name` when one would fit.
```js
"_version": "2.0.0",
"_customData": {
  "_pointDefinitions": [
    {
        "_name":"nameOfDefinition",
        "_points":[
            [0,0,0,0],
            [0,4,5,0.5,"optionalEasing", "optionalSpline"],
            [0,0,0,1,"optionalEasing", "optionalSpline"]]
    }
  ]
},
"_events": [],
"_notes": [],
"_obstacles":[]
```

A point definition usually follows the pattern of `[data, time, "optionalEasing", "optionalSpline"]`, 
- data can be multiple points of data, this part of a point varies per property,
- time is a float from 0-1, points must be ordered by their time values
- "optionalEasing" is an optional field, with any easing from easings.net. This is the easing that will be used in the interpolation from the last point to the one with the easing defined on it.
- "optionalSpline" is an optional field, with any spline implemented, currently only `"splineCatmullRom`". It acts like easings, affecting the movement from the last point to the one with the spline on it.


# _customData
- [`_track`](#_track)
- [`_animation`](#_animation)


# AnimateTrack

```js
{
    "_time":float, //time in beats
    "_type":"AnimateTrack",
    "_data":{
        "_track":string //The track you want to animate.
        "_duration":float //The length of the event in beats (OPTIONAL)
        "_easing": string //An easing for the animation to follow (defaults to linear)
        "_property":pointDefinition //The property you want to animate
    }
}
```
Animate track will animate the properties of everything on the track individually at the same time. The animation will go over the `pointDefinition` over the course of `_duration`

## Properties that work with AnimateTrack
- [`_position`](#_position)
- [`_rotation`](#_rotation)
- [`_localRotation`](#_localRotation)
- [`_dissolve`](#_dissolve)
- [`_dissolveArrow`](#_dissolveArrow)
- [`_color`](#_color) (Chroma)
- [`_time`](#_time)

(I should write something better here lo)

# AssignPathAnimation
```js
{
    "_time":float, //time in beats
    "_type":"AssignPathAnimation",
    "_data":{
        "_track":string //The track you want to animate.
        "_duration":float //how long it takes to reach this path (OPTIONAL)
        "_easing": string //An easing for moving to the path (defaults to linear)
        "_property":pointDefinition //The property you want to assign the path to
    }
}
```
`AssignPathAnimation` will assign a "path" to the notes. This is pretty different from `AnimateTrack`, in this case, the time value of the `pointDefinition` is the point each object on the track is at in its life span. Meaning a point with time 0 would be right when the object spawns, a point with time 0.5 would be right around when the player is expected to hit it, and 1 being right before the object despawns. At 0.75, walls and notes will begin their despawn animation and start flying away very quickly.

## Properties that work with AssignPathAnimation

- [`_position`](#_position)
- [`_rotation`](#_rotation)
- [`_localRotation`](#_localRotation)
- [`_dissolve`](#_dissolve)
- [`_dissolveArrow`](#_dissolveArrow)
- [`_color`](#_color) (Chroma)
- [`_definitePosition`](#_definitePosition)
 
 (visuals here n stuff)

# AssignTrackParent
```js
{
    "_time":float, //time in beats
    "_type":"AssignTrackParent",
    "_data":{
        "_childrenTracks":[string] //Array of tracks to parent to _parentTrack
        "_parentTrack":string //The track you want to animate.
    }
}
```
`AssignTrackParent` will parent any number of children tracks to a single parent track. 

Only transform properties animated with [`AnimateTrack`](#AnimateTrack) will influence the player's position. Those properties being:
- [`_position`](#_position)
- [`_rotation`](#_rotation)
- [`_localRotation`](#_rotation)

# AssignPlayerToTrack
`AssignPlayerToTrack` will assign the player object to the specified `_track`. 
```js
{
    "_time":float, //time in beats
    "_type":"AssignPlayerToTrack",
    "_data":{
        "_track":string //the track you wish to assign to the player
    }
}
```
Only transform properties animated with [`AnimateTrack`](#AnimateTrack) will influence the player's position. Those properties being:
- [`_position`](#_position)
- [`_rotation`](#_rotation)
- [`_localRotation`](#_rotation)

### IT IS HIGHLY RECOMMENDED TO HAVE A TRACK DEDICATED TO THE PLAYER, AND NOT USE EASINGS IN MOVEMENT.
This is vr, non-linear movement or any form of rotation can easily cause severe motion sickness.
To clarify,
## It is very easy to make people motion sick with player tracks, please use them carefully and sparingly.

# _track
`_track` is a string property in the `_customData` of the object you want to give said track to. It can be placed on any object in the `_obstacles` or `_notes` arrays.

ex: 
```json
"_notes":[
  {
    "_time": 8.0,
    "_lineIndex": 2,
    "_lineLayer": 0,
    "_type": 1,
    "_cutDirection": 1,
    "_customData": {
      "_track":"ExampleTrack"
    }
  }
]
```

# _animation
`_animation` is an object that can be put in the `_customData` of any object in the `_obstacles` or `_notes` array.


`_animation` effectively works as [`AssignPathAnimation`](#AssignPathAnimation), but instantly. It can contain a `pointDefinition` of any property that would work with a path animation. Will overwrite any path animation on the object's track.


# Properties
- [`_position`](#_position)
- [`_rotation`](#_rotation)
- [`_localRotation`](#_localRotation)
- [`_dissolve`](#_dissolve)
- [`_dissolveArrow`](#_dissolveArrow)
- [`_interactable`](#_interactable)
- [`_color`](#_color) (Chroma)
- [`_definitePosition`](#_definitePosition) (EXCLUSIVE TO AssignPathAnimation)
- [`_time`](#_time) (EXCLUSIVE TO AnimateTrack)

# _position
`_position` may be used in both [`AnimateTrack`](#AnimateTrack) and [`AssignPathAnimation`](#AssignPathAnimation)

Describes the position **offset** of an object. It will continue any normal movement and have this stacked on top of it.

**Note:** `_position` uses what is known as Spooky Units, 1 Spooky unit = 1 lane.

Point definition: `[x, y, z, time, (optional)easing, (optional)spline]`


```js
// Example
"_pointDefinitions":[
{
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
}]
```
## event examples
```js

//animate track
{
	"_time":4,
	"_type":"AnimateTrack",
	"_data":{
		"_track":"firstPositionDemo",
		"_position":"examplePositionPointDef",
		"_duration":8
	}
}
```
Above event results in:

![AnimateTrack Position Demo](media/PositionAnimateTrack1.gif)
```js

//AssignPath
{
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
}
```
Above event results in:

![AnimateTrack Position Demo](media/PositionAssignPath.gif)

# _localRotation
`_localRotation` may be used in both [`AnimateTrack`](#AnimateTrack) and [`AssignPathAnimation`](#AssignPathAnimation)

This property describes the **local** rotation offset of an object. This means it is rotated with itself as the origin. Uses euler values.

Point definition: `[pitch, yaw, roll, time, (optional)easing]`
```js
// Example
"_pointDefinitions":[
  {
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
]
```
## event examples
```js
//AnimateTrack
{
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
}
```
Above event results in:

![AnimateTrack localrotation demo](media/LocalRotationAnimateTrack.gif)
```js
//AssignPathAnimation
{
	"_time":30,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"localRotationDemo",
		"_localRotation":"localSpinDemoPath"
	}
}

```
Above event results in: 

![AssignPath localrotation demo](media/LocalRotationAssignPath.gif)
# _rotation
`_rotation` may be used in both [`AnimateTrack`](#AnimateTrack) and [`AssignPathAnimation`](#AssignPathAnimation)

This property describes the **global** rotation offset of an object. This means it is rotated with **the world** as the origin. Uses euler values. Think of 360 mode.

Point definition: `[pitch, yaw, roll, time, (optional)easing]`
```js
"_pointDefinitions":[
  {
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
]
```
## event examples
```js
//AnimateTrack
{
	"_time":40,
	"_type":"AnimateTrack",
	"_data":{
		"_track":"RotationDemo",
		"_rotation":"RotationPointsAnimate",
		"_duration":10
	}
}
```
Above event results in:

![AnimateTrack rotation demo why are you reading this](media/RotationAnimateTrack.gif)
```js
//AssignPath
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
}
```
Above event results in:

!["animate my ass"](media/RotationAssignPath.gif)
# _dissolve
`_dissolve` may be used in both [`AnimateTrack`](#AnimateTrack) and [`AssignPathAnimation`](#AssignPathAnimation)

This property controls the dissolve effect on both notes and walls. Its the look that happens when things go away upon failing a song, for example. **Keep in mind that notes and the arrows on notes have seperate dissolve properties**, see [`_dissolveArrow`](#_dissolveArrow) 

**Note**: How this looks will depend on the player's graphics settings. 

Point definition: `[transparency, time, (optional)easing]`
```js
// Example
"_pointDefinitions": [
 {
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
]
```
## event examples
```js
//AnimateTrack
{
	"_time":60,
	"_type":"AnimateTrack",
	"_data":{
		"_track":"dissolveDemo",
		"_dissolve":"dissolveDemoAnimate",
		"_duration":10
	}
},
```
Above event results in:

![wowee stop hovering](media/DissolveAnimateTrack.gif)

```js
{
	"_time":70,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"dissolveDemo",
    "_dissolve":"dissolveDemoPath"
  	}
}
```
Above event results in:

![fuck you danike](media/DissolveAssignPath.gif)
# _dissolveArrow
`_dissolveArrow` may be used in both [`AnimateTrack`](#AnimateTrack) and [`AssignPathAnimation`](#AssignPathAnimation)

This property controls the dissolve effect on the arrows of notes. Similar to the look of the disappearing notes modifier.

Point definition: `[transparency, time, (optional)easing]`
```js
// Example

[
  {
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
]
```
## event examples
```js
//AnimateTrack
{
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
}
```
Above event results in:

![HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger](media/DissolveAnimateTrack.gif)
```js
//AssignhPathAnimation
{
	"_time":90,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"dissolveArrowDemo",
		"_dissolveArrow":"dissolveArrowDemoPath"
	}
}
```
![cyan is a gay furry coder](media/DissolveArrowAssignPath.gif)

# _color
`_color` may be used in both [`AnimateTrack`](#AnimateTrack) and [`AssignPathAnimation`](#AssignPathAnimation)

### WARNING: THIS PROPERTY IS IMPLEMENTED BY CHROMA AND REQUIRES CHROMA TO FUNCTION

Describes the color of an object. Will override any other color the object may have had.

Point definition: `[red,green,blue,alpha,time,(optional)easing]`
```js
// Example
{
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
}, 
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
```
## event examples
```js
//AnimateTrack
{
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
```
Above event results in:

![reaxt was in here and he was in pain](media/ColorAnimateTrack.gif)
```js
//AssignPathAnimation
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
}
```
Above event results in:

![reaxt was in here and he was in pain](media/ColorAssignPath.gif)
# _definitePosition
`_definitePosition` may be used in [`AssignPathAnimation`](#AssignPathAnimation)

Describes the **definite** position of an object. Will completely overwrite the object's default movement. However, this does take into account lineIndex/lineLayer and world rotation.

**Note:** `_definitePosition` uses what is known as Spooky Units, 1 Spooky unit = 1 lane.

Point definition: `[x, y, z, time, (optional)easing, (optional)spline]`

```js
// Example
"_definitePosition":[
  {
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
]
```
## event examples
```js
{
  "_time":0,
  "_type":"AssignPathAnimation",
  "_data":{
      "_track":"definitePosDemo",
      "_definitePosition":"defPosNormal",
      "_duration":0
    }
  }
{
	"_time":132,
	"_type":"AssignPathAnimation",
	"_data":{
		"_track":"definitePosDemo",
		"_definitePosition":"defPosPath",
		"_duration":3
  }
}
```
Above event results in:

![its almost done](media/DefinitePositionAnimateTrack.gif)


# _time
`_time` may be used in [`AnimateTrack`](#AnimateTrack)

`_time` is a little weird so bear with this section. `_time` can only be used in animate track as it lets you control what point in the note's "life" it is at a given time using [`AnimateTrack`](#AnimateTrack). 

```js
[
  //example
  [0,0],
  [0.2,0.2],
  [0.3,0.4],
  [0.4,0.4]
]
```
//reaxt note: this could be wrong I need to test all of this before publishing

It is worth noting that every object on one track will get the same time values when animating this property. This means they would suddenly appear to all be at the same point. **It is recommended for every object to have its own track when using `_time`**

Say you want a time AnimateTrack on an object that will make it behave normally for starters. You want the AnimateTrack to start *right when the object spawns*, meaning `_time-halfJump` of the object. It's duration should be `halfJump*2`. With this, the point definition of 
```js
[
  [0,0],
  [1,1]
]
```
would behave as normal. 
```js
[
  [0,0],
  [0.45, 0.15],
  [0.15, 0.30],
  [0.5, 0.5],
  [1,1]
]
```
would appear to go forwards, then backwards

## example
This time example is not the best. It is highly recommended you script/automate anything involving time. This is simply showcasing on one note to help visualize.
```js
// Example
"_pointDefinitions":[
  {
    "_name":"SingleNoteTime",
    "_points":[
      [0,0],
      [0.45, 0.15],
      [0.15, 0.30],
      [0.5, 0.5],
      [1,1]
    ]
  }
]
```
## event
```js
{
	"_time":153,
	"_type":"AnimateTrack",
	"_data":{
		"_time":"SingleNoteTime",
		"_duration":10,
		"_track":"singleNoteTimeTrack"
	}
}
```
Above event results in:

![i hate manually writing time stuff](media/TimeIsDumb.gif)
