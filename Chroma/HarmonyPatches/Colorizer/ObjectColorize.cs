using System.Collections.Generic;
using Chroma.Animation;
using Chroma.Colorizer;
using Chroma.Settings;
using Heck;
using Heck.Animation;
using IPA.Utilities;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace Chroma.HarmonyPatches.Colorizer
{
    internal class ObjectColorize : IAffinity
    {
        private static readonly FieldAccessor<NoteMovement, NoteJump>.Accessor _noteJumpAccessor = FieldAccessor<NoteMovement, NoteJump>.GetAccessor("_jump");
        private static readonly FieldAccessor<NoteJump, float>.Accessor _jumpDurationAccessor = FieldAccessor<NoteJump, float>.GetAccessor("_jumpDuration");

        private readonly BombColorizerManager _bombManager;
        private readonly NoteColorizerManager _noteManager;
        private readonly ObstacleColorizerManager _obstacleManager;
        private readonly IAudioTimeSource _audioTimeSource;
        private readonly CustomData _customData;

        private ObjectColorize(
            BombColorizerManager bombManager,
            NoteColorizerManager noteManager,
            ObstacleColorizerManager obstacleManager,
            IAudioTimeSource audioTimeSource,
            [Inject(Id = ChromaController.ID)] CustomData customData)
        {
            _bombManager = bombManager;
            _noteManager = noteManager;
            _obstacleManager = obstacleManager;
            _audioTimeSource = audioTimeSource;
            _customData = customData;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BombNoteController), nameof(BombNoteController.Init))]
        private void BombColorize(BombNoteController __instance, NoteData noteData)
        {
            // They said it couldn't be done, they called me a madman
            if (_customData.Resolve(noteData, out ChromaObjectData? chromaData))
            {
                _bombManager.Colorize(__instance, chromaData.Color);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(GameNoteController), nameof(GameNoteController.Init))]
        private void NoteColorize(GameNoteController __instance, NoteData noteData)
        {
            if (ChromaConfig.Instance.NoteColoringDisabled)
            {
                return;
            }

            if (_customData.Resolve(noteData, out ChromaObjectData? chromaData))
            {
                _noteManager.Colorize(__instance, chromaData.Color);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.Init))]
        private void ObstacleColorize(ObstacleController __instance, ObstacleData obstacleData)
        {
            if (_customData.Resolve(obstacleData, out ChromaObjectData? chromaData))
            {
                _obstacleManager.Colorize(__instance, chromaData.Color);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(NoteController), nameof(NoteController.ManualUpdate))]
        private void NoteUpdateColorize(NoteController __instance, NoteData ____noteData, NoteMovement ____noteMovement)
        {
            if (ChromaConfig.Instance.NoteColoringDisabled
                || !_customData.Resolve(____noteData, out ChromaObjectData? chromaData))
            {
                return;
            }

            List<Track>? tracks = chromaData.Track;
            PointDefinition? pathPointDefinition = chromaData.LocalPathColor;
            if (tracks == null && pathPointDefinition == null)
            {
                return;
            }

            NoteJump noteJump = _noteJumpAccessor(ref ____noteMovement);

            float jumpDuration = _jumpDurationAccessor(ref noteJump);
            float elapsedTime = _audioTimeSource.songTime - (____noteData.time - (jumpDuration * 0.5f));
            float normalTime = elapsedTime / jumpDuration;

            AnimationHelper.GetColorOffset(pathPointDefinition, tracks, normalTime, out Color? colorOffset);

            if (!colorOffset.HasValue)
            {
                return;
            }

            Color color = colorOffset.Value;
            if (__instance is BombNoteController)
            {
                _bombManager.Colorize(__instance, color);
            }
            else
            {
                _noteManager.Colorize(__instance, color);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.Update))]
        private void ObstacleUpdateColorize(
            ObstacleController __instance,
            ObstacleData ____obstacleData,
            AudioTimeSyncController ____audioTimeSyncController,
            float ____startTimeOffset,
            float ____move1Duration,
            float ____move2Duration,
            float ____obstacleDuration)
        {
            if (!_customData.Resolve(____obstacleData, out ChromaObjectData? chromaData))
            {
                return;
            }

            List<Track>? tracks = chromaData.Track;
            PointDefinition? pathPointDefinition = chromaData.LocalPathColor;
            if (tracks == null && pathPointDefinition == null)
            {
                return;
            }

            float elapsedTime = ____audioTimeSyncController.songTime - ____startTimeOffset;
            float normalTime = (elapsedTime - ____move1Duration) / (____move2Duration + ____obstacleDuration);

            AnimationHelper.GetColorOffset(pathPointDefinition, tracks, normalTime, out Color? colorOffset);

            if (colorOffset.HasValue)
            {
                _obstacleManager.Colorize(__instance, colorOffset.Value);
            }
        }
    }
}
