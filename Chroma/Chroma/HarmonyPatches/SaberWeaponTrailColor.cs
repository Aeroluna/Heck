using Chroma;
using Chroma.Settings;
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
[HarmonyPatch("color", MethodType.Getter)]
class SaberWeaponTrailColor {
    
    public static bool Prefix(SaberWeaponTrail __instance, ref Color __result, ref Color ____multiplierSaberColor, ref SaberTypeObject ____saberTypeObject) {
        if (TechnicolourController.Instantiated()) {
            Color? c = TechnicolourController.Instance.rainbowSaberColours[____saberTypeObject.saberType == Saber.SaberType.SaberA ? 0 : 1];
            if (c != null) {
                __result = (Color)c;
                return false;
            }
        }
        return true;
    }

}

