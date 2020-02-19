using Chroma.VFX;
using Harmony;
using UnityEngine;

[HarmonyPriority(Priority.High)]
[HarmonyPatch(typeof(SaberWeaponTrail))]
[HarmonyPatch("color", MethodType.Getter)]
internal class SaberWeaponTrailColor
{
    public static bool Prefix(SaberWeaponTrail __instance, ref Color __result, ref Color ____multiplierSaberColor, ref SaberTypeObject ____saberTypeObject)
    {
        if (TechnicolourController.Instantiated())
        {
            Color? c = TechnicolourController.Instance.rainbowSaberColours[____saberTypeObject.saberType == Saber.SaberType.SaberA ? 0 : 1];
            if (c.HasValue)
            {
                __result = c.Value;
                return false;
            }
        }
        return true;
    }
}