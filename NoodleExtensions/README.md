# NoodleExtensions

This adds a host of new things you can do with your maps.

### MAPPERS: EITHER USE MAPPING EXTENSIONS OR NOODLE EXTENSIONS, DO NOT USE BOTH AT THE SAME TIME. Noodle Extensions is meant to completely replace Mapping Extensions, as they both do the same thing. Having both requirements can break some features.

### NOODLE EXTENSIONS WILL NOT READ MAPS THAT USE MAPPING EXTENSIONS. YOU HAVE TO INSTALL MAPPING EXTENSIONS FOR THOSE. (You can have both of the mods installed at once)

#### If you use any of these features, you MUST add "`Noodle Extensions`" as a requirement for your map for them to function, you can go [Here](https://github.com/Kylemc1413/SongCore/blob/master/README.md) to see how adding requirements to the info.dat works.

All of these cool features are done through CustomJSONData, from the `"_customData"` field or `"_customEvents"`.

Example of `_customData`:

	"_notes":[
	  {
	    "_time": 8.0,
	    "_lineIndex": 2,
	    "_lineLayer": 0,
	    "_type": 1,
	    "_cutDirection": 1,
	    "_customData": {
	      "foo":3,
	      "bar":"Hello, BSMG!"
	    }
	  }
	]

Example of `_customEvents`:

	"_version": "2.0.0",
	"_customData": {
	  "_customEvents": [
	    {
	      "_time": 0.0,
	      "_type": "HelloWorld",
	      "_data": {
	        "foo": 1.0,
	        "message": "Hello from a custom event!"
	      }
	    }
	  ]
	}
	"_events": [
	// ...

## Objects (Notes and Obstacles)
  * `"_notes"`/`"_obstacles"` -> `"_customData"`
    * `"_position"`: `[x, y]` (float) Should be self explanatory. Will override `_lineIndex` and `_lineLayer` NOTE: All positions are based off [Beatwalls system](https://camo.githubusercontent.com/295a4c05e569c99c6bf07cfabda8d80afdec1b7d/68747470733a2f2f692e696d6775722e636f6d2f557a37614944672e706e673d31303078313030).
    * `"_rotation"`: `[x, y, z]` (float) Also known as "world rotation". Think the `360Degree` Characteristic but way more options. This field can also be just a single float (`"_rotation": 90`) and it will be interpreted as [0, x, 0] (`"_rotation": [0, 90, 0]`). [0, 0, 0] will always be the initial position the player is facing at the beginning of the song.
    * `"_localRotation"`: `[x, y, z]` (float) Allows you to [rotate the object](https://cdn.discordapp.com/attachments/642393483000283146/695698691943825559/unknown.png). This won't affect the direction it spawns from or the path it takes. The origin for walls is the front bottom center, [as illustrated by spooky](https://cdn.discordapp.com/attachments/642393483000283146/725065831850967150/unknown.png). THIS MAY HAVE SOME STRANGE EFFECTS ON NOTES.
    * `"_noteJumpMovementSpeed"`: (float) Set the NJS of an individual object.
    * `"_noteJumpStartBeatOffset"`: (float) Set the spawn offset of an individual object.
    * `"_track"`: (string) This adds the object to the specified track. Tracks are used by custom events.
    * `"_animation"`: These individually applies path animations to the object. See `"AssignPathAnimation"` custom event for more info.
      * `"_position"`: (Point Definition)
      * `"_rotation"`: (Point Definition)
      * `"_scale"`: (Point Definition)
      * `"_localRotation"`: (Point Definition)
      * `"_definitePosition"`: (Point Definition)
      * `"_dissolve"`: (Point Definition)
      * `"_dissolveArrow"`: (Point Definition) (Note Only)
## Notes
  * `"_notes"` -> `"_customData"`
    * `"_cutDirection"`: (float) Rotate notes 360 degrees with as much precision as you want (0 is down). Will override `"_cutDirection"`.
    * `"_flip"`: `[flip line index, flip jump]` (float) Flip notes from an initial spawn position to its true position. [PREVIEW](https://streamable.com/9o2puz) (Map by AaltopahWi). Flip line index is the initial `x` the note will spawn at and flip jump is how high (or low) the note will jump up (or down) when flipping to its true position. Base game behaviour will set one note's flip jump to -1 and the other to 1.

## Obstacles
  * `"_obstacles"` -> `"_customData"`
    * `"_scale"`: `[w, h, l]` (float) Width, height and length of the wall. `"_scale": [1, 1, 1]` will be perfectly square. Each number is fully optional and will default to `_width`, `_type`, and `_duration` respectively.

## Events
  * `"_events"` -> `"_customData"`
    * ONLY APPLYS TO EVENTS 14 AND 15 (360 rotation events).
    * `"_rotation"`: (float) Rotate by this amount. Just like normal rotation events value, but you know, as a float and not whatever dumb stuff Beat Games is doing.

## Custom Events
  * `"_customData"` -> `"_customEvents"`
    * `"_type"`: `"AssignPathAnimation"` 
      * Assigns the animation that objects of a track will follow throughout its lifespan.
      * A time of `0` represents the moment an object finishes jumping in.
      * A note will reach the player at approximately `0.5`.
      * `"_data"`:
        * `"_track"`: (string) Name of the track to affect.
        * `"_duration"`: (float) How long it takes to assign the animation in beats. Will default to 0 if not specified.
        * `"_easing":`: (string) Refer to https://easings.net/en.
        * `"_position"`: (Point Definition) Will be relative to base game movement.
        * `"_rotation"`: (Point Definition)
        * `"_scale"`: (Point Definition) [Yeah....](https://cdn.discordapp.com/attachments/443569023951568906/719503041883144192/unknown.png)
        * `"_localRotation"`: (Point Definition) 
        * `"_definitePosition"`: (Point Definition) Defines the exact position an object should be at. Completely overwrites base game movement. Will still respect `_lineIndex` and `_lineLayer`.
        * `"_dissolve"`: (Point Definition) Transparency. `1` is fully opaque and `0` is fully transparent. NOTE: Unlike other point definitions that take four numbers per point, `_dissolve` and `_dissolveArrow` only take two (transparency and time).
        * `"_dissolveArrow"`: (Point Definition) (Note Only) Transparency of the arrow on notes. Think the `Disappearing Arrows` Modifier.
    * `"_type"`: `"AnimateTrack"` 
      * Defines a set offset to apply to all objects of a track.
      * `"_data"`:
        * `"_track"`: (string) See Above.
        * `"_duration"`: (float) How long it takes to complete the animation in beats. Will default to 0 if not specified.
        * `"_easing":`: (string) See above.
        * `"_position"`: (Point Definition) See above.
        * `"_rotation"`: (Point Definition) See above.
        * `"_scale"`: (Point Definition) See above.
        * `"_localRotation"`: (Point Definition) See above.
        * `"_dissolve"`: (Point Definition) See above.
        * `"_dissolveArrow"`: (Point Definition) (Note Only) See above.

## Point Definition
What is a point definition? Well, at its core, its a collection of points with time.
Here is an example of one being defined (look at `_position`):
		
	{
	  "_time": 3.0,
	  "_type": "AnimateTrack",
	  "_data": {
	    "_track": "ZigZagTrack",
	    "_duration": 1,
	    "_position": [
	      [0, 0, 0, 0],
	      [1, 0, 0, 0.25],
	      [-1, 0, 0, 0.75],
	      [0, 0, 0, 1]
	    ]
	  }
	}

Each point has three floats to define x, y, and z as well as a fourth float to define time (which is always on a 0 - 1 scale).
You can also apply an easing and/or a spline (splines only work for position) by adding strings to a point.
Refer to https://easings.net/en for easing names.
Currently only one spline type exists: `"splineCatmullRom"`.

Example:
 
	"_position": [
	  [0,0,0,0],
	  [1,5,0,0.5,"easeInOutSine"]
	  [6,0,0,0.75,"splineCatmullRom"]
	  [5,-2,-1,1,"easeOutCubic,"splineCatmullRom"]
	]"

Would you rather define a point definition once and reuse that?
  * `"_customData"` -> `"_pointDefinitions"`
    * `"_name"`: (string) What to name your point definition.
    * `"_points"`: (Point Definition) See above.

Example for [this](https://streamable.com/uuz00f) (Map by Skeelie):

	"_customData": {
	  "_pointDefinitions": [
	    {
          "_name": "Squish",
          "_points": [
            [0.5, 4, 0.5, 0.2],
            [3, 0.5, 3, 0.23],
            [0.5, 3, 0.5, 0.26],
            [1.5, 0.75, 1.5, 0.29],
            [0.75, 1.5, 0.75, 0.32],
            [1.1, 0.9, 1.1, 0.35],
            [0.9, 1.1, 0.9, 0.38],
            [1, 1, 1, 0.41]
          ]
        }, {
          "_name": "DropFromAbove",
          "_points": [
            [0, 40, 0, 0],
            [0, 0, 0, 0.2],
          ]
        }
	  ],
	  "_customEvents": [
	    {
	      "_time": 3.0,
	      "_type": "AssignPathAnimation",
	      "_data": {
	        "_track": "BounceTrack",
	        "_position": "DropFromAbove",
            "_scale": "Squish"
	      }
	    }
	  ]
	}
