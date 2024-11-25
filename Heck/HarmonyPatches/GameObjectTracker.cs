using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck.Animation;
using Heck.Deserialize;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace Heck.HarmonyPatches;

[HeckPatch(PatchType.Features)]
internal class GameObjectTracker : IAffinity, IDisposable
{
    private readonly DeserializedData _deserializedData;
    private readonly CodeInstruction _despawnMirroredObject;

    private NoteData? _noteDebrisNoteData;

    private GameObjectTracker(
        [Inject(Id = HeckController.ID)] DeserializedData deserializedData,
        BeatmapObjectManager beatmapObjectManager)
    {
        _deserializedData = deserializedData;
        beatmapObjectManager.noteWasSpawnedEvent += n => AddObject(n.noteData, n);
        beatmapObjectManager.noteWasDespawnedEvent += n => RemoveObject(n.noteData, n);
        beatmapObjectManager.obstacleWasSpawnedEvent += n => AddObject(n.obstacleData, n);
        beatmapObjectManager.obstacleWasDespawnedEvent += n => RemoveObject(n.obstacleData, n);
        beatmapObjectManager.sliderWasSpawnedEvent += n => AddObject(n.sliderData, n);
        beatmapObjectManager.sliderWasDespawnedEvent += n => RemoveObject(n.sliderData, n);
        _despawnMirroredObject =
            InstanceTranspilers.EmitInstanceDelegate<Func<MonoBehaviour, MonoBehaviour>>(DespawnMirroredObject);
    }

    public void Dispose()
    {
        InstanceTranspilers.DisposeDelegate(_despawnMirroredObject);
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(MirroredGameNoteController), nameof(MirroredObstacleController.Mirror))]
    [AffinityPatch(typeof(MirroredNoteController<INoteMirrorable>), nameof(MirroredObstacleController.Mirror))]
    private void AddMirroredNote(NoteControllerBase __instance)
    {
        AddObject(__instance.noteData, __instance);
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(MirroredObstacleController), nameof(MirroredObstacleController.Mirror))]
    private void AddMirroredObstacle(MirroredObstacleController __instance, ObstacleController obstacleController)
    {
        AddObject(obstacleController.obstacleData, __instance);
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(MirroredSliderController), nameof(MirroredObstacleController.Mirror))]
    private void AddMirroredSlider(MirroredSliderController __instance, SliderController sliderController)
    {
        AddObject(sliderController.sliderData, __instance);
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(NoteDebris), nameof(NoteDebris.Init))]
    private void AddNoteDebris(NoteDebris __instance)
    {
        if (_noteDebrisNoteData == null || !TryGetTrack(_noteDebrisNoteData, out IReadOnlyList<Track>? tracks))
        {
            return;
        }

        GameObject gameObject = __instance.gameObject;
        NoteDebrisTracker tracker = new(tracks, gameObject);
        foreach (Track track in tracks)
        {
            track.AddGameObject(gameObject);
        }

        __instance.didFinishEvent.Add(tracker);
    }

    private void AddObject(BeatmapObjectData objectData, MonoBehaviour behaviour)
    {
        if (!TryGetTrack(objectData, out IReadOnlyList<Track>? tracks))
        {
            return;
        }

        GameObject gameObject = behaviour.gameObject;
        foreach (Track track in tracks)
        {
            track.AddGameObject(gameObject);
        }
    }

    private MonoBehaviour DespawnMirroredObject(MonoBehaviour behaviour)
    {
        switch (behaviour)
        {
            case NoteControllerBase note:
                RemoveObject(note.noteData, note);
                break;

            case MirroredObstacleController obstacle:
                RemoveObject(obstacle._followedObstacle.obstacleData, obstacle);
                break;

            case MirroredSliderController slider:
                RemoveObject(slider._followedSlider.sliderData, slider);
                break;
        }

        return behaviour;
    }

    private void RemoveObject(BeatmapObjectData objectData, MonoBehaviour behaviour)
    {
        if (!TryGetTrack(objectData, out IReadOnlyList<Track>? tracks))
        {
            return;
        }

        GameObject gameObject = behaviour.gameObject;
        foreach (Track track in tracks)
        {
            track.RemoveGameObject(gameObject);
        }
    }

    [AffinityTranspiler]
    [AffinityPatch(typeof(MirroredBeatmapObjectManager), nameof(MirroredBeatmapObjectManager.HandleNoteWasDespawned))]
    [AffinityPatch(
        typeof(MirroredBeatmapObjectManager),
        nameof(MirroredBeatmapObjectManager.HandleObstacleWasDespawned))]
    [AffinityPatch(typeof(MirroredBeatmapObjectManager), nameof(MirroredBeatmapObjectManager.HandleSliderWasDespawned))]
    private IEnumerable<CodeInstruction> ReplaceConditionTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            /*
             * -- this._mirroredObstaclePoolContainer.Despawn(item);
             * ++ this._mirroredObstaclePoolContainer.Despawn(DespawnMirroredObject(item));
             */
            .MatchForward(
                false,
                new CodeMatch(n => n.opcode == OpCodes.Callvirt && ((MethodInfo)n.operand).Name == "Despawn"))
            .Repeat(
                n => n
                    .Insert(_despawnMirroredObject)
                    .Advance(2))
            .InstructionEnumeration();
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(NoteCutCoreEffectsSpawner), nameof(NoteCutCoreEffectsSpawner.SpawnNoteCutEffect))]
    private void SaveNoteDebrisNoteData(NoteController noteController)
    {
        _noteDebrisNoteData = noteController.noteData;
    }

    private bool TryGetTrack(BeatmapObjectData objectData, [NotNullWhen(true)] out IReadOnlyList<Track>? track)
    {
        if (!_deserializedData.Resolve(objectData, out HeckObjectData? heckData) || heckData.Track == null)
        {
            track = null;
            return false;
        }

        track = heckData.Track;
        return true;
    }

    private class NoteDebrisTracker : INoteDebrisDidFinishEvent
    {
        private readonly GameObject _gameObject;
        private readonly IReadOnlyList<Track> _tracks;

        internal NoteDebrisTracker(IReadOnlyList<Track> tracks, GameObject gameObject)
        {
            _tracks = tracks;
            _gameObject = gameObject;
        }

        public void HandleNoteDebrisDidFinish(NoteDebris noteDebris)
        {
            noteDebris.didFinishEvent.Remove(this);
            foreach (Track track in _tracks)
            {
                track.RemoveGameObject(_gameObject);
            }
        }
    }
}
