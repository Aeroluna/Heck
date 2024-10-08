﻿using System;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using Heck.Deserialize;
using Heck.Event;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Chroma.ChromaController;
using Object = UnityEngine.Object;

namespace Chroma.Animation;

[CustomEvent(ASSIGN_FOG_TRACK)]
internal class FogAnimatorV2 : ITickable, IDisposable, ICustomEvent
{
    private readonly BloomFogSO _bloomFog;
    private readonly DeserializedData _deserializedData;

    private readonly BloomFogEnvironmentParams _transitionFogParams;
    private Track? _track;

    [UsedImplicitly]
    private FogAnimatorV2(
        BloomFogSO bloomFog,
        [Inject(Id = ID)] DeserializedData deserializedData)
    {
        _bloomFog = bloomFog;
        _deserializedData = deserializedData;

        _transitionFogParams = ScriptableObject.CreateInstance<BloomFogEnvironmentParams>();
        BloomFogEnvironmentParams defaultParams = bloomFog.defaultForParams;
        _transitionFogParams.attenuation = defaultParams.attenuation;
        _transitionFogParams.offset = defaultParams.offset;
        _transitionFogParams.heightFogStartY = defaultParams.heightFogStartY;
        _transitionFogParams.heightFogHeight = defaultParams.heightFogHeight;
        bloomFog.transitionFogParams = _transitionFogParams;
    }

    public void Callback(CustomEventData customEventData)
    {
        if (_deserializedData.Resolve(customEventData, out ChromaAssignFogEventData? chromaData))
        {
            _track = chromaData.Track;
        }
    }

    public void Dispose()
    {
        _bloomFog.transition = 0;
        _bloomFog.transitionFogParams = null;
        Object.Destroy(_transitionFogParams);
    }

    public void Tick()
    {
        if (_track == null)
        {
            return;
        }

        float? attenuation = _track.GetProperty<float>(V2_ATTENUATION);
        if (attenuation.HasValue)
        {
            _transitionFogParams.attenuation = attenuation.Value;
        }

        float? offset = _track.GetProperty<float>(V2_OFFSET);
        if (offset.HasValue)
        {
            _transitionFogParams.offset = offset.Value;
        }

        float? startY = _track.GetProperty<float>(V2_HEIGHT_FOG_STARTY);
        if (startY.HasValue)
        {
            _transitionFogParams.heightFogStartY = startY.Value;
        }

        float? height = _track.GetProperty<float>(V2_HEIGHT_FOG_HEIGHT);
        if (height.HasValue)
        {
            _transitionFogParams.heightFogHeight = height.Value;
        }

        _bloomFog._transition = 1;
    }
}
