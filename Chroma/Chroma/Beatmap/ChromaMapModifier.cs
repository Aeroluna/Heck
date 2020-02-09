using Chroma.Beatmap.Events.Legacy;
using Chroma.Settings;
using Chroma.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using IPA.Utilities;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;

namespace Chroma.Beatmap {

    public class ChromaMapModifier {

        public delegate void ModifyCustomBeatmapDelegate(int randSeed, ref CustomBeatmap customBeatmap, ref BeatmapData baseBeatmapData, ref PlayerSpecificSettings playerSettings, ref BaseGameModeType baseGameMode, ref float bpm);
        public static event ModifyCustomBeatmapDelegate ModifyCustomBeatmapEvent;

        public static CustomBeatmap CreateTransformedData(BeatmapData beatmapData, ref ChromaBehaviour chromaBehaviour, ref PlayerSpecificSettings playerSettings, ref BaseGameModeType baseGameMode, ref float bpm) {

            ColourManager.TechnicolourLightsForceDisabled = false;

            if (beatmapData == null) ChromaLogger.Log("Null beatmapData", ChromaLogger.Level.ERROR);
            if (playerSettings == null) ChromaLogger.Log("Null playerSettings", ChromaLogger.Level.ERROR);

            List<CustomBeatmapObject> customBeatmapData = new List<CustomBeatmapObject>();

            beatmapData = beatmapData.GetCopy();
            BeatmapLineData[] beatmapLinesData = beatmapData.beatmapLinesData;
            int[] array = new int[beatmapLinesData.Length];
            for (int i = 0; i < array.Length; i++) {
                array[i] = 0;
            }
            UnityEngine.Random.InitState(0);
            bool flag;
            do {
                flag = false;
                float num = 999999f;
                int num2 = 0;
                for (int j = 0; j < beatmapLinesData.Length; j++) {
                    BeatmapObjectData[] beatmapObjectsData = beatmapLinesData[j].beatmapObjectsData;
                    int num3 = array[j];
                    while (num3 < beatmapObjectsData.Length && beatmapObjectsData[num3].time < num + 0.001f) {
                        flag = true;
                        BeatmapObjectData beatmapObjectData = beatmapObjectsData[num3];
                        float time = beatmapObjectData.time;
                        if (Mathf.Abs(time - num) < 0.001f) {
                            if (beatmapObjectData.beatmapObjectType == BeatmapObjectType.Note) {
                                num2++;
                            }
                        } else if (time < num) {
                            num = time;
                            if (beatmapObjectData.beatmapObjectType == BeatmapObjectType.Note) {
                                num2 = 1;
                            } else {
                                num2 = 0;
                            }
                        }
                        num3++;
                    }
                }

                CustomBeatmapObject customBeatmapObject = null;
                for (int k = 0; k < beatmapLinesData.Length; k++) {
                    BeatmapObjectData[] beatmapObjectsData2 = beatmapLinesData[k].beatmapObjectsData;
                    int num4 = array[k];
                    while (num4 < beatmapObjectsData2.Length && beatmapObjectsData2[num4].time < num + 0.001f) {
                        BeatmapObjectData beatmapObjectData2 = beatmapObjectsData2[num4];
                        if (beatmapObjectData2.beatmapObjectType == BeatmapObjectType.Note) {
                            NoteData noteData = beatmapObjectData2 as NoteData;
                            if (noteData != null) {

                                if (noteData.noteType == NoteType.NoteA || noteData.noteType == NoteType.NoteB) {
                                    customBeatmapObject = new CustomBeatmapNote(beatmapObjectData2 as NoteData);
                                } else if (noteData.noteType == NoteType.Bomb) {
                                    customBeatmapObject = new CustomBeatmapBomb(beatmapObjectData2 as NoteData);
                                }

                            }
                        } else if (beatmapObjectData2.beatmapObjectType == BeatmapObjectType.Obstacle) {
                            ObstacleData obstacle = beatmapObjectData2 as ObstacleData;
                            if (obstacle != null) {
                                customBeatmapObject = new CustomBeatmapBarrier(obstacle);
                            }
                        }
                        array[k]++;
                        num4++;
                        if (customBeatmapObject == null) { ChromaLogger.Log("Null beatmap object! ID:" + beatmapObjectData2.id + " LI:" + beatmapObjectData2.lineIndex + " T:" + beatmapObjectData2.time, ChromaLogger.Level.WARNING); } else customBeatmapData.Add(customBeatmapObject); //CT Added
                    }
                }
            }
            while (flag);
            
            CustomBeatmap customBeatmap = new CustomBeatmap(customBeatmapData);
            
            try {
                ChromaLogger.Log("Modifying map data...");
                if (beatmapData == null) ChromaLogger.Log("Null beatmapData", ChromaLogger.Level.ERROR);
                if (beatmapData.beatmapEventData == null) ChromaLogger.Log("Null beatmapData.beatmapEventData", ChromaLogger.Level.ERROR);
                ModifyCustomBeatmapEvent?.Invoke(beatmapData.notesCount * beatmapData.beatmapEventData.Length, ref customBeatmap, ref beatmapData, ref playerSettings, ref baseGameMode, ref bpm);
                //ModifyCustomBeatmap(beatmapData.notesCount * beatmapData.beatmapEventData.Length, ref customBeatmap, ref beatmapData, ref playerSettings, ref baseGameMode, ref bpm);
            } catch (Exception e) {
                ChromaLogger.Log("Exception modifying map data...", ChromaLogger.Level.ERROR);
                ChromaLogger.Log(e);
            }

            customBeatmapData = customBeatmap.CustomBeatmapObjects;

            //from Tweaks
            int[] array2 = new int[beatmapLinesData.Length];
            for (int l = 0; l < customBeatmapData.Count; l++) {
                BeatmapObjectData beatmapObjectData2 = customBeatmapData[l].Data;
                array2[Mathf.Clamp(beatmapObjectData2.lineIndex, 0, 3)]++; //array2[beatmapObjectData2.lineIndex]++;
            }
            BeatmapLineData[] linesData = new BeatmapLineData[beatmapLinesData.Length];
            for (int m = 0; m < beatmapLinesData.Length; m++) {
                linesData[m] = new BeatmapLineData();
                linesData[m].beatmapObjectsData = new BeatmapObjectData[array2[m]];
                array[m] = 0;
            }
            for (int n = 0; n < customBeatmapData.Count; n++) {
                BeatmapObjectData beatmapObjectData3 = customBeatmapData[n].Data;
                int lineIndex = Mathf.Clamp(beatmapObjectData3.lineIndex, 0, 3); //beatmapObjectData3.lineIndex;
                linesData[lineIndex].beatmapObjectsData[array[lineIndex]] = beatmapObjectData3;
                array[lineIndex]++;
            }
            BeatmapEventData[] eventsData = new BeatmapEventData[beatmapData.beatmapEventData.Length];
            for (int num5 = 0; num5 < beatmapData.beatmapEventData.Length; num5++) {
                BeatmapEventData beatmapEventData = beatmapData.beatmapEventData[num5];
                eventsData[num5] = beatmapEventData;
            }

            if (ChromaConfig.LightshowModifier) {
                foreach (BeatmapLineData b in linesData) {
                    b.beatmapObjectsData = b.beatmapObjectsData.Where((source, index) => b.beatmapObjectsData[index].beatmapObjectType != BeatmapObjectType.Note).ToArray();
                }
                BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("Chroma");
            }

            beatmapData = new BeatmapData(linesData, eventsData);

            //if (chromaInjectmap != null) chromaInjectmap.Inject(beatmapData);

            customBeatmap.BeatmapData = beatmapData;

            
            /*
             * LIGHTING EVENTS
             */
            
            if (ChromaConfig.CustomColourEventsEnabled) {

                BeatmapEventData[] bevData = beatmapData.beatmapEventData;
                ChromaColourEvent unfilledEvent = null;
                for (int i = bevData.Length - 1; i >= 0; i--) {
                    ChromaEvent cLight = ApplyCustomEvent(bevData[i], ref unfilledEvent);
                    if (cLight != null) ColourManager.TechnicolourLightsForceDisabled = true;

                    try {
                        if (ChromaBehaviour.LightingRegistered && bevData[i] is CustomBeatmapEventData customData) {
                            dynamic dynData = customData.customData;
                            if (Trees.at(dynData, "_lightsID") != null) ColourManager.TechnicolourLightsForceDisabled = true;
                        }
                    }
                    catch { }
                }

            }

            return customBeatmap;

        }



        // Colour Events
        //   - Stores two colour values, A and B
        //   - These events have two purposes
        //     - Used to recolour any lighting events placed after them, unless...
        //     - if used immediately after a "data event", their values will influence the data event.
        // 1,900,000,001 = 1900000001 = A/B
        // 1,900,000,002 = 1900000002 = AltA/AltB
        // 1,900,000,003 = 1900000003 = White/Half White
        // 1,900,000,004 = 1900000004 = Technicolor/Technicolor
        // 1,900,000,005 = 1900000005 = RandomColor/RandomColor

        // Data Events
        //   - Require Colour Events before them
        //   - Remember Unity colour uses 0-1, not 0-255
        // 1,950,000,001 = 1950000001 = Note Scale Event        - Scales all future spawned notes by (A.red * 1.5f)
        // 1,950,000,002 = 1950000002 = Health Event            - Alters the player's health by ((A.red - 0.5f) * 2)
        // 1,950,000,003 = 1950000003 = Rotate Event            - Rotates the player on all three axes by (A.red * 360 on x axis, A.green * 360 on y axis, A.blue * 360 on z axis)
        // 1,950,000,004 = 1950000004 = Ambient Light Event     - Immediately changes the colour of ambient lights to (A)
        // 1,950,000,005 = 1950000005 = Barrier Colour Event    - Changes all future spawned barrier colours to (A)

        // > 2,000,000,000 = >2000000000 = RGB (see ColourManager.ColourFromInt)

        public static ChromaEvent ApplyCustomEvent(BeatmapEventData bev, ref ChromaColourEvent unfilledColourEvent) {

            //ChromaLogger.Log("Checking BEV ||| " + bev.time + "s : " + bev.value + "v");

            Color a, b;

            if (bev.value >= ColourManager.RGB_INT_OFFSET) { // > 2,000,000,000 = >2000000000 = RGB (see ColourManager.ColourFromInt)
                a = ColourManager.ColourFromInt(bev.value);
                b = a;
                if (FillColourEvent(bev, ref unfilledColourEvent, a)) return unfilledColourEvent;
            } else {
                return null;
            }

            if (unfilledColourEvent != null) unfilledColourEvent = null;

            return ChromaEvent.SetChromaEvent(bev, new ChromaLightEvent(bev, a, b));
        }

        public static bool FillColourEvent(BeatmapEventData bev, ref ChromaColourEvent unfilledColourEvent, params Color[] colors) {
            if (unfilledColourEvent != null) {
                unfilledColourEvent.Colors = colors;
                if (ChromaConfig.LegacyLighting) ChromaEvent.SetChromaEvent(bev, unfilledColourEvent);
                ChromaLogger.Log("Filled " + unfilledColourEvent.GetType().ToString() + " event.");
                unfilledColourEvent = null;
                return true;
            }
            return false;
        }




    }

}
