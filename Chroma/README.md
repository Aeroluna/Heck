# Chroma

Colors!

#### If you use any of these features, you MUST add "Chroma" as a suggestion or requirement for your map for them to function, you can go [Here](https://github.com/Kylemc1413/SongCore/blob/master/README.md) to see how adding suggestions/requirements to the info.dat works

NOTE: All RGBA values are on a 0-1 scale (Not a 0-255 scale).

All of these cool features are done through CustomJSONData, from either the "_customData" field or "_customEvents".

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

## Environment Enhancement
* Environment
  * Set `"PrintEnvironmentEnhancementDebug"` to true in the Chroma.json config file to print environment enhancement information to your console.
  *  `"_customData"` -> `"_environment"`
     *  `"_id"`: (string) The ID to use when looking up the GameObject.
     *  `"_lookupMethod"`: (string) How to use the ID to search. (Regex, Exact, Contains)
     *  `"_hide"`: (bool) When true, disables the GameObject.
     *  `"_scale"`: `[x, y, z]` (float) Sets scale of GameObject.
     *  `"_position"`: `[x, y, z]` (float) Sets position of GameObject.
     *  `"_localPosition"`: `[x, y, z]` (float) Sets localPosition of GameObject.
     *  `"_rotation"`: `[x, y, z]` (float) Sets rotation of GameObject.
     *  `"_localRotation"`: `[x, y, z]` (float) Sets localRotation of GameObject.
     *  `"_track"`: (string) For use with Noodle Extensions.

## Color Data
* RGB
  * `"_events"` -> `"_customData"`
    * Will only apply to the single light it's attached to.
    * `"_lightID"`: (int) Causes event to only affect specified [ID](https://streamable.com/dhs31). Can be an array.
    * `"_color"`: `[r, g, b, a]` (float) Array of RGB values (Alpha is optional and will default to 1 if not specified).
    * `"_propID"`: (int) Deprecated, use _lightID. Causes event to only affect specified [propagation group](https://streamable.com/byyda).
* Gradient
  * Hate placing a thousand color blocks for a gradient? So do I. This will handle all of that for you.
  * `"_events"` -> `"_customData"`
    * `"_lightGradient"`:
      * `"_duration"`: (float) How long should it take to get from start color to end color (in beats).
      * `"_startColor"`: `[r, g, b, a]` (float) Initial color at beginning of gradient.
      * `"_endColor"`: `[r, g, b, a]` (float) Final color of gradient.
      * `"_easing"`: (string) Gradient will ease according to this field. Use [these names](https://easings.net/en) as the string.
* Obstacle Color
  * `"_obstacles"` -> `"_customData"`
    * Will only apply to the single wall it's attached to.
    * `"_color"`: (float) `[r, g, b, a]` (float) Array of RGB values (Alpha is optional and will default to 1 if not specified).
* Bomb Color
  * `"_notes"` -> `"_customData"`
    * Can only be applied to notes of type 3.
    * Will only apply to the single bomb it's attached to.
    * `"_color"`: `[r, g, b, a]` (float) Array of RGB values (Alpha is optional and will default to 1 if not specified).
* Note Color
  * When a note with a custom color is cut, the saber will turn its color to that custom color.
  * `"_notes"` -> `"_customData"`
    * Will only apply to the single note it's attached to
    * `"_color"`: `[r, g, b, a]` (float) Array of RGB values (Alpha is optional and will default to 1 if not specified).
  
## Data Events
* Precise Laser Event
  * `"_events"` -> `"_customData"`
    * Can only be applied to laser speed events
    * `"_lockPosition"`: (bool) Set to true and the event it is attached to will not reset laser positions.
    * `"_preciseSpeed"`: (float) Identical to just setting value, but allows for decimals. Will overwrite value (Because the game will randomize laser position on anything other than value 0, a small trick you can do is set value to 1 and _preciseSpeed to 0, creating 0 speed lasers with a randomized position).
    * `"_direction"`: (int) Set the spin direction (0 left lasers spin CCW, 1 left lasers spin CW).
* Precise Rotation Event
  * `"_events"` -> `"_customData"`
    * Can only be applied to ring rotation events.
    * `"_nameFilter"`: (string) Causes event to only affect rings with a listed name (e.g. SmallTrackLaneRings, BigTrackLaneRings).
    * `"_reset"`: (bool) Will reset the rings when set to true (Overwrites values below).
    * `"_rotation"`: (float) Dictates how far the first ring will spin.
    * `"_step"`: (float) Dictates how much rotation is added between each ring.
    * `"_prop"`: (float) Dictates the rate at which rings behind the first one have physics applied to them.  High value makes all rings move simultaneously, low value gives them [significant delay](https://streamable.com/vsdr9).
    * `"_speed"`: (float) Dictates the [speed multiplier of the rings](https://streamable.com/fxlse).
    * `"_direction"`: (int) Direction to spin the rings (1 spins clockwise, 0 spins counter-clockwise).
    * `"_counterSpin"`: (bool) Causes the smaller ring to [spin in the opposite direction](https://streamable.com/4duyy).
    * `"_stepMult"`: (float) Deprecated. Step is multiplied by this value.
    * `"_propMult"`: (float) Deprecated. Prop is multiplied by this value.
    * `"_speedMult"`: (float) Deprecated. Speed is multiplied by this value.
* Hide Note Spawn Effect
  * `"_notes"` -> `"_customData"`
    * `"_disableSpawnEffect"`: (bool) Set to true and the note spawn effect will be hidden.
    
## Animation
See [Noodle Extensions documentation](https://github.com/Aeroluna/NoodleExtensions/blob/master/Documentation/AnimationDocs.md#_color)

## Info.dat Data
* Environment Removal
  * DEPRECATED use _environment.
  * This goes within the per-level _customData, NOT the top level _customData.
  * `"_customData"`
    * `"_environmentRemoval"`: `[List of objects you want to remove goes here, with commas between]` Recommended to use UnityIPADebugger to find the name of the objects. e.g. `RocketCar` for cars in RocketEnvironment, `Logo` for Green Day logo.
