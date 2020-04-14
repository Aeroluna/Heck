# NoodleExtensions

This adds a host of new things you can do with your maps.

### THIS MOD WILL NOT READ MAPS THAT USE MAPPING EXTENSIONS. YOU HAVE TO INSTALL MAPPING EXTENSIONS FOR THOSE. (You can have both of the mods installed at once)

#### If you use any of these features, you MUST add "Noodle Extensions" as a requirement for your map for them to function, you can go [Here](https://github.com/Kylemc1413/SongCore/blob/master/README.md) to see how adding requirements to the info.dat works

All of these cool features are done through CustomJSONData, from the "_customData" field

Example of _customData:

    "_notes":[
        {
            "_time":8.0,
            "_lineIndex":2,
            "_lineLayer":0,
            "_type":1,
            "_cutDirection":1,
            "_customData":{
                "foo":3,
                "bar":"Hello, BSMG!"
            }
        },

## Notes
  * `"_notes"` -> `"_customData"`
    * `"_position"`: `[x, y]` (float) Should be self explanatory. Will override `_lineIndex` and `_lineLayer` NOTE: All positions are based off [Beatwalls system](https://camo.githubusercontent.com/295a4c05e569c99c6bf07cfabda8d80afdec1b7d/68747470733a2f2f692e696d6775722e636f6d2f557a37614944672e706e673d31303078313030).
    * `"_rotation"`: (float) Think the `360Degree` Characteristic but as a float and not limited to that characteristic. 0 will always be the initial position the player is facing at the beginning of the song
    * `"_cutDirection"`: (float) Rotate notes 360 degrees with as much precision as you want (0 is down). Will override `_cutDirection`.
    * `"_flip"`: `[flip line index, flip jump]` (float) Flip notes from an initial spawn position to its true position. [PREVIEW](https://streamable.com/9o2puz) Flip line index is the initial `x` the note will spawn at and flip jump is how high (or low) the note will jump up (or down) when flipping to its true position.

## Obstacles
  * `"_obstacles"` -> `"_customData"`
    * `"_position"`: `[x, y]` (float) Should be self explanatory. Will override `_lineIndex` and `_lineLayer` ***NOTE: All positions are based off [Beatwalls system](https://camo.githubusercontent.com/295a4c05e569c99c6bf07cfabda8d80afdec1b7d/68747470733a2f2f692e696d6775722e636f6d2f557a37614944672e706e673d31303078313030).
    * `"_scale"`: `[w, h]` (float) Width and height of the wall. A `_scale` of `[1, 1]` will be perfectly square.
    * `"_rotation"`: (float) Think the `360Degree` Characteristic but as a float and not limited to that characteristic. 0 will always be the initial position the player is facing at the beginning of the song
    * `"_localRotation"`: `[x, y, z]` (float) Allows you to [rotate the wall](https://cdn.discordapp.com/attachments/642393483000283146/695698691943825559/unknown.png). This won't affect the direction it spawns from or the path it takes.

## Events
  * `"_events"` -> `"_customData"`
    * ONLY APPLYS TO EVENTS 14 AND 15 (360 rotation events)
    * `"_rotation"`: (float) Rotate by this amount. Just like normal rotation events value, but you know, as a float and not whatever dumb stuff Beat Games is doing.
