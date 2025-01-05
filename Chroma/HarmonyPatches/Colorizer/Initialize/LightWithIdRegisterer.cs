using System.Collections.Generic;
using Chroma.Colorizer;
using Chroma.Lighting;
using SiraUtil.Affinity;
using UnityEngine;

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
        List<ILightWithId> lights = _lightWithIdManager._lights[lightId];
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

    [AffinityPrefix]
    [AffinityPatch(typeof(LightWithIdManager), nameof(LightWithIdManager.UnregisterLight))]
    private bool DontClearList(ILightWithId lightWithId)
    {
        lightWithId.__SetIsUnRegistered();
        return false;
    }

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
}
