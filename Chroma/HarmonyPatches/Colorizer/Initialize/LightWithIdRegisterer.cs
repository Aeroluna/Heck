using System.Collections.Generic;
using System.Linq;
using Chroma.Colorizer;
using Chroma.Lighting;
using SiraUtil.Affinity;
using UnityEngine;
using Array = System.Array;

namespace Chroma.HarmonyPatches.Colorizer.Initialize;

internal class LightWithIdRegisterer : IAffinity
{
    private readonly LightColorizerManager _colorizerManager;
    private readonly LightWithIdManager _lightWithIdManager;
    private readonly HashSet<ILightWithId> _needToRegister = [];
    private readonly Dictionary<ILightWithId, int> _requestedIds = new();
    private readonly LightIDTableManager _tableManager;

    private LightWithIdRegisterer(
        LightColorizerManager colorizerManager,
        LightWithIdManager lightWithIdManager,
        LightIDTableManager tableManager)
    {
        _colorizerManager = colorizerManager;
        _lightWithIdManager = lightWithIdManager;
        _tableManager = tableManager;
    }

    internal void ForceUnregister(ILightWithId lightWithId)
    {
        int lightId = lightWithId.lightId;
#if LATEST
        ILightWithId[] lights = _lightWithIdManager._oldMapping[lightId].lightInstances;
#else
        List<ILightWithId> lights = _lightWithIdManager._lights[lightId];
#endif
        int index = lights.FindIndex(n => n == lightWithId);
        lights[index] = null!; // TODO: handle null
        _tableManager.UnregisterIndex(lightId, index);
        _colorizerManager.CreateLightColorizerContractByLightID(
            lightId,
            n => n.ChromaLightSwitchEventEffect.UnregisterLight(lightWithId));
        lightWithId.__SetIsUnRegistered();
    }

    internal void MarkForTableRegister(ILightWithId lightWithId)
    {
        _needToRegister.Add(lightWithId);
    }

    internal void SetRequestedId(ILightWithId lightWithId, int id)
    {
        _requestedIds[lightWithId] = id;
    }

#if LATEST
    // too lazy to make a transpiler
    [AffinityPrefix]
    [AffinityPatch(
        typeof(LightWithIdManager),
        nameof(LightWithIdManager.SetColorForId),
        AffinityMethodType.Normal,
        null,
        typeof(LightWithIdManager.LightMapData),
        typeof(Color))]
    private bool AllowNull(
        LightWithIdManager.LightMapData lightMapData,
        Color color,
        ref bool ____didChangeSomeColorsThisFrame)
    {
        lightMapData.color = color;
        ____didChangeSomeColorsThisFrame = true;
        foreach (ILightWithId lightWithId in lightMapData.lightInstances)
        {
            if (lightWithId is { isRegistered: true })
            {
                lightWithId.ColorWasSet(color);
            }
        }

        return false;
    }
#else
    // too lazy to make a transpiler
    [AffinityPrefix]
    [AffinityPatch(typeof(LightWithIdManager), nameof(LightWithIdManager.SetColorForId))]
    private bool AllowNull(
        int lightId,
        Color color,
        List<ILightWithId?>?[] ____lights,
        Color?[] ____colors,
        ref bool ____didChangeSomeColorsThisFrame)
    {
        ____colors[lightId] = color;
        ____didChangeSomeColorsThisFrame = true;
        ____lights[lightId]
            ?.ForEach(
                n =>
                {
                    if (n is { isRegistered: true })
                    {
                        n.ColorWasSet(color);
                    }
                });
        return false;
    }
#endif

    [AffinityPrefix]
    [AffinityPatch(typeof(LightWithIdManager), nameof(LightWithIdManager.UnregisterLight))]
    private bool DontClearList(ILightWithId lightWithId)
    {
        lightWithId.__SetIsUnRegistered();
        return false;
    }

#if LATEST
    [AffinityPrefix]
    [AffinityPatch(typeof(LightWithIdManager), nameof(LightWithIdManager.RegisterLight))]
    private void Prefix(
        ref bool __runOriginal,
        LightWithIdManager __instance,
        ILightWithId lightWithId,
        LightWithIdManager.LightMapData?[] ____oldMapping)
    {
        // for some reason, despite being an affinity patch bound to player, this still runs in the menu scene
        // so quick and dirty fix
        if (__instance.gameObject.scene.name.Contains("Menu"))
        {
            return;
        }

        __runOriginal = false;

        if (lightWithId.isRegistered)
        {
            return;
        }

        int lightId = lightWithId.lightId;
        if (lightId == -1)
        {
            return;
        }

        LightWithIdManager.LightMapData? lightMapData = ____oldMapping[lightId];
        if (lightMapData == null)
        {
            lightMapData = new LightWithIdManager.LightMapData
            {
                lightInstances = [],
                color = Color.clear,
                hasColor = false
            };
            ____oldMapping[lightId] = lightMapData;
        }

        lightWithId.__SetIsRegistered();

        ILightWithId[] lightInstances = lightMapData.lightInstances;
        if (lightInstances.Contains(lightWithId))
        {
            return;
        }

        ILightWithId[] array2 = new ILightWithId[lightInstances.Length + 1];
        Array.Copy(lightInstances, array2, lightInstances.Length);
        array2[lightInstances.Length] = lightWithId;
        lightMapData.lightInstances = array2;

        int index = array2.Length;
        if (_needToRegister.Remove(lightWithId))
        {
            int? tableId = _requestedIds.TryGetValue(lightWithId, out int value) ? value : null;
            _tableManager.RegisterIndex(lightId, index, tableId);
        }

        // this also colors the light
        _colorizerManager.CreateLightColorizerContractByLightID(
            lightId,
            n => n.ChromaLightSwitchEventEffect.RegisterLight(lightWithId, index));

        lightWithId.ColorWasSet(lightMapData.hasColor ? lightMapData.color : Color.clear);
    }
#else
    [AffinityPrefix]
    [AffinityPatch(typeof(LightWithIdManager), nameof(LightWithIdManager.RegisterLight))]
    private void Prefix(
        ref bool __runOriginal,
        LightWithIdManager __instance,
        ILightWithId lightWithId,
        List<ILightWithId>?[] ____lights,
        List<ILightWithId> ____lightsToUnregister,
        Color?[] ____colors)
    {
        // TODO: figure this shit out
        // for some reason, despite being an affinity patch bound to player, this still runs in the menu scene
        // so quick and dirty fix
        if (__instance.gameObject.scene.name.Contains("Menu"))
        {
            return;
        }

        __runOriginal = false;

        if (lightWithId.isRegistered)
        {
            return;
        }

        int lightId = lightWithId.lightId;
        if (lightId == -1)
        {
            return;
        }

        List<ILightWithId>? lights = ____lights[lightId];
        if (lights == null)
        {
            ____lights[lightId] = lights = new List<ILightWithId>(10);
        }

        lightWithId.__SetIsRegistered();

        if (lights.Contains(lightWithId))
        {
            return;
        }

        // TODO: find a better way to register "new" lights to table
        int index = lights.Count;
        if (_needToRegister.Remove(lightWithId))
        {
            int? tableId = _requestedIds.TryGetValue(lightWithId, out int value) ? value : null;
            _tableManager.RegisterIndex(lightId, index, tableId);
        }

        // this also colors the light
        _colorizerManager.CreateLightColorizerContractByLightID(
            lightId,
            n => n.ChromaLightSwitchEventEffect.RegisterLight(lightWithId, index));

        lights.Add(lightWithId);
        ____lightsToUnregister.Remove(lightWithId);
        Color? color = ____colors[lightId];
        lightWithId.ColorWasSet(color ?? Color.clear);
    }
#endif
}
