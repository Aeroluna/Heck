# NoodleExtensions

This adds a host of new things you can do with your maps.

### MAPPERS: EITHER USE MAPPING EXTENSIONS OR NOODLE EXTENSIONS, DO NOT USE BOTH AT THE SAME TIME. Noodle Extensions is meant to completely replace Mapping Extensions, as they both do the same thing. Having both requirements can break some features.

## NOODLE EXTENSIONS WILL NOT READ MAPS THAT USE MAPPING EXTENSIONS. YOU HAVE TO INSTALL MAPPING EXTENSIONS FOR THOSE. (You can have both of the mods installed at once)

#### If you use any of these features, you MUST add "`Noodle Extensions`" as a requirement for your map for them to function, you can go [Here](https://github.com/Kylemc1413/SongCore/blob/master/README.md) to see how adding requirements to the info.dat works.

All of these cool features are done through CustomJSONData, from the `"_customData"` field or [`"_customEvents"`](Documentation/AnimationDocs.md#custom-events)

### Documentation for animation can be found [here](Documentation/AnimationDocs.md)

Example of `_customData`:
```json
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
```


## Objects (Notes and Obstacles)
  * When sorting your notes, sort by `_position`, not `_lineIndex` and `_lineLayer`.
  * `"_notes"`/`"_obstacles"` -> `"_customData"`
    * `"_position"`: `[x, y]` (float) Should be self explanatory. Will override `_lineIndex` and `_lineLayer` NOTE: All positions are based off [Beatwalls system](https://camo.githubusercontent.com/295a4c05e569c99c6bf07cfabda8d80afdec1b7d/68747470733a2f2f692e696d6775722e636f6d2f557a37614944672e706e673d31303078313030).
    * `"_rotation"`: `[x, y, z]` (float) Also known as "world rotation". Think the `360Degree` Characteristic but way more options. This field can also be just a single float (`"_rotation": 90`) and it will be interpreted as [0, x, 0] (`"_rotation": [0, 90, 0]`). [0, 0, 0] will always be the initial position the player is facing at the beginning of the song.
    * `"_localRotation"`: `[x, y, z]` (float) Allows you to [rotate the object](https://cdn.discordapp.com/attachments/642393483000283146/695698691943825559/unknown.png). This won't affect the direction it spawns from or the path it takes. The origin for walls is the front bottom center, [as illustrated by spooky](https://cdn.discordapp.com/attachments/642393483000283146/725065831850967150/unknown.png). THIS MAY HAVE SOME STRANGE EFFECTS ON NOTES.
    * `"_noteJumpMovementSpeed"`: (float) Set the NJS of an individual object.
    * `"_noteJumpStartBeatOffset"`: (float) Set the spawn offset of an individual object.
    * `"_fake"`: (bool) When true, causes the note/wall to not show up in the note/wall count and to not count towards score in any way.
    * `"_interactable"`: (bool) When true, the note/wall cannot be interacted with. This means notes cannot be cut and walls will not interact with sabers/putting your head in the wall. Notes will still count towards your score.
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

