using Chroma;
using Chroma.VFX;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[HarmonyPriority(Priority.High)]
[HarmonyPatch(typeof(SaberWeaponTrail))]
[HarmonyPatch("color", PropertyMethod.Getter)]
class SaberWeaponTrailColor {

    //Default colours
    //
    
    public static bool Prefix(SaberWeaponTrail __instance, ref Color __result, ref Color ____multiplierSaberColor, ref SaberTypeObject ____saberType) {
        if (VFXRainbowSabers.rainbowSaberColours != null) {
            __result = VFXRainbowSabers.rainbowSaberColours[____saberType.saberType == Saber.SaberType.SaberA ? 0 : 1];
            return false;
        }
        return true;
    }

}

