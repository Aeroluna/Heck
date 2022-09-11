using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using static Chroma.ChromaController;
using Logger = IPA.Logging.Logger;
using Object = UnityEngine.Object;

namespace Chroma.EnvironmentEnhancement
{
    internal class MaterialsManager : IDisposable
    {
        private static readonly Material _standardMaterial = InstantiateSharedMaterial(ShaderType.Standard);
        private static readonly Material _opaqueLightMaterial = InstantiateSharedMaterial(ShaderType.OpaqueLight);
        private static readonly Material _transparentLightMaterial = InstantiateSharedMaterial(ShaderType.TransparentLight);
        private static readonly Material _baseWaterMaterial = InstantiateSharedMaterial(ShaderType.BillieWater);

        internal static EnvironmentInfoSO? CurrentEnvironmentSO { get; set; }

        private readonly HashSet<Material> _createdMaterials = new();

        private readonly Dictionary<string, Track> _beatmapTracks;
        private readonly LazyInject<MaterialColorAnimator> _materialColorAnimator;
        private readonly bool _v2;

        private Dictionary<string, MaterialInfo> MaterialInfos { get; } = new();

        private Dictionary<ShaderType, Material> EnvironmentMaterialInfos { get; } = new();

        [UsedImplicitly]
        private MaterialsManager(
            IReadonlyBeatmapData readonlyBeatmap,
            Dictionary<string, Track> beatmapTracks,
            LazyInject<MaterialColorAnimator> materialColorAnimator)
        {
            CustomBeatmapData beatmapData = (CustomBeatmapData)readonlyBeatmap;
            _v2 = beatmapData.version2_6_0AndEarlier;
            _beatmapTracks = beatmapTracks;
            _materialColorAnimator = materialColorAnimator;

            SharedCoroutineStarter.instance.StartCoroutine(SetupEnvironmentMaterials(CurrentEnvironmentSO));
            CurrentEnvironmentSO = null;

            CustomData? materialsData = beatmapData.customData.Get<CustomData>(_v2 ? V2_MATERIALS : MATERIALS);
            if (materialsData == null)
            {
                return;
            }

            foreach ((string key, object? value) in materialsData)
            {
                if (value == null)
                {
                    throw new InvalidOperationException($"[{key}] was null.");
                }

                MaterialInfos.Add(key, CreateMaterialInfo((CustomData)value));
            }
        }

        public void Dispose()
        {
            foreach (Material createdMaterial in _createdMaterials)
            {
                Object.Destroy(createdMaterial);
            }
        }

        private IEnumerator SetupEnvironmentMaterials(EnvironmentInfoSO? currentEnvironment)
        {
            EnvironmentMaterialInfos[ShaderType.Standard] = _standardMaterial;
            EnvironmentMaterialInfos[ShaderType.OpaqueLight] = _opaqueLightMaterial;
            EnvironmentMaterialInfos[ShaderType.TransparentLight] = _transparentLightMaterial;
            EnvironmentMaterialInfos[ShaderType.BaseWater] = _baseWaterMaterial;

            EnvironmentsListSO environmentsListSO = Resources.FindObjectsOfTypeAll<EnvironmentsListSO>().First();

            List<string> loadedScenes = new();

            void LoadEnvironmentScene(string environmentName)
            {
                EnvironmentInfoSO environmentInfoSO =
                    environmentsListSO.GetEnvironmentInfoBySerializedName(environmentName + "Environment");
                if (currentEnvironment != null && (environmentInfoSO == currentEnvironment ||
                                                   currentEnvironment.sceneInfo.sceneName == environmentInfoSO.sceneInfo.sceneName))
                {
                    return;
                }

                string sceneInfoSceneName = environmentInfoSO.sceneInfo.sceneName;
                Log.Logger.Log($"Loading environment {sceneInfoSceneName}");
                SceneManager.LoadScene(sceneInfoSceneName, LoadSceneMode.Additive);
                foreach (GameObject rootGameObject in SceneManager.GetSceneByName(sceneInfoSceneName)
                             .GetRootGameObjects())
                {
                    Object.Destroy(rootGameObject);
                }

                loadedScenes.Add(sceneInfoSceneName);
                //
                // EnvironmentSceneSetupData environmentSceneSetupData = new(environmentInfoSO, null, false);
                // SingleFixedSceneScenesTransitionSetupDataSO? setupData = ScriptableObject.CreateInstance<SingleFixedSceneScenesTransitionSetupDataSO>();
                // setupData.Init(environmentSceneSetupData);
                // gameScenesManager.PushScenes(setupData);
                //
                //
                //
                // return () =>
                // {
                //     gameScenesManager.PopScenes();
                // }
            }

            LoadEnvironmentScene("BTS");
            LoadEnvironmentScene("Billie");
            LoadEnvironmentScene("Interscope");

            // Wait a frame because LoadScene only loads the scene the next frame
            yield return null;

            // This is the only way I could think of for getting materials by name
            Material[] environmentMaterials = Resources.FindObjectsOfTypeAll<Material>();

            void GetEnvironmentMaterial(ShaderType key, string name)
            {
                // I use contains here because I can't be bothered to care about identical names
                Material? material = environmentMaterials.FirstOrDefault(e => e.name.Contains(name));
                if (material != null)
                {
                    EnvironmentMaterialInfos[key] = material;
                    Log.Logger.Log($"Found {key} (realname {name}) material in current environment", Logger.Level.Info);
                }
                else
                {
                    Log.Logger.Log($"Could not find {name} material in current environment", Logger.Level.Info);
                }
            }


            GetEnvironmentMaterial(ShaderType.BTSPillar, "BTSDarkEnvironmentWithHeightFog");
            GetEnvironmentMaterial(ShaderType.BillieWater, "WaterfallFalling");
            GetEnvironmentMaterial(ShaderType.WaterfallMirror, "WaterfallMirror");
            GetEnvironmentMaterial(ShaderType.InterscopeConcrete, "Concrete2");
            GetEnvironmentMaterial(ShaderType.InterscopeCar, "Car");
            GetEnvironmentMaterial(ShaderType.Obstacle, "ObstacleCoreHD");

            // Cleanup
            foreach (string loadedScene in loadedScenes)
            {
                Log.Logger.Log($"Unloading scene {loadedScene}");
                SceneManager.UnloadSceneAsync(loadedScene);
            }
        }

        internal MaterialInfo GetMaterial(object o)
        {
            return o switch
            {
                    string name => MaterialInfos.TryGetValue(name, out MaterialInfo info) ? info : throw new InvalidOperationException($"Could not find {name} material"),
                    CustomData data => CreateMaterialInfo(data),
                    _ => throw new InvalidOperationException($"Could not read [{MATERIAL}].")
            };
        }

        internal MaterialInfo CreateMaterialInfo(CustomData customData)
        {
            Color color = CustomDataManager.GetColorFromData(customData, _v2) ?? new Color(0, 0, 0, 0);
            ShaderType shaderType = customData.GetStringToEnumRequired<ShaderType>(_v2 ? V2_SHADER_PRESET : SHADER_PRESET);
            string[]? shaderKeywords = customData.Get<List<object>?>(_v2 ? V2_SHADER_KEYWORDS : SHADER_KEYWORDS)?.Cast<string>().ToArray();
            List<Track>? track = customData.GetNullableTrackArray(_beatmapTracks, _v2)?.ToList();

            // if the environment material does not exist
            // it fallbacks to ShaderType.Standard
            // this is because I have no idea if all materials will be available
            // on all environments
            if (!EnvironmentMaterialInfos.TryGetValue(shaderType, out Material originalMaterial))
            {
                Log.Logger.Log($"Could not find {shaderType} in environment materials, falling back to {nameof(ShaderType.Standard)}", Logger.Level.Warning);
                originalMaterial = EnvironmentMaterialInfos[ShaderType.Standard];
            }

            Material material = Object.Instantiate(originalMaterial);
            _createdMaterials.Add(material);
            material.color = color;
            if (shaderKeywords != null)
            {
                material.shaderKeywords = shaderKeywords;
            }

            MaterialInfo materialInfo = new(shaderType, material, track, originalMaterial);
            if (track != null)
            {
                _materialColorAnimator.Value.Add(materialInfo);
            }

            return materialInfo;
        }

        private static Material InstantiateSharedMaterial(ShaderType shaderType)
        {
            return new Material(Shader.Find(shaderType switch
            {
                ShaderType.OpaqueLight => "Custom/OpaqueNeonLight",
                ShaderType.TransparentLight => "Custom/TransparentNeonLight",
                ShaderType.BaseWater => "Custom/WaterLit",
                _ => "Custom/SimpleLit"
            }))
            {
                globalIlluminationFlags = GeometryFactory.IsLightType(shaderType)
                    ? MaterialGlobalIlluminationFlags.EmissiveIsBlack
                    : MaterialGlobalIlluminationFlags.RealtimeEmissive,
                enableInstancing = true,
                shaderKeywords = shaderType switch
                {
                    // Keywords found in RUE PC in BS 1.24
                    ShaderType.Standard => new[]
                    {
                        "DIFFUSE", "ENABLE_DIFFUSE", "ENABLE_FOG", "ENABLE_HEIGHT_FOG", "ENABLE_SPECULAR", "FOG",
                        "HEIGHT_FOG", "REFLECTION_PROBE_BOX_PROJECTION", "SPECULAR", "_EMISSION",
                        "_ENABLE_FOG_TINT", "_RIMLIGHT_NONE"
                    },
                    ShaderType.BaseWater => new[]
                    {
                        "FOG", "HEIGHT_FOG", "INVERT_RIMLIGHT", "MASK_RED_IS_ALPHA", "NOISE_DITHERING",
                        "NORMAL_MAP", "REFLECTION_PROBE", "REFLECTION_PROBE_BOX_PROJECTION", "_DECALBLEND_ALPHABLEND",
                        "_DISSOLVEAXIS_LOCALX", "_EMISSIONCOLORTYPE_FLAT", "_EMISSIONTEXTURE_NONE",
                        "_RIMLIGHT_NONE", "_ROTATE_UV_NONE", "_VERTEXMODE_NONE", "_WHITEBOOSTTYPE_NONE",
                        "_ZWRITE_ON"
                    },
                    ShaderType.OpaqueLight => new[]
                    {
                        "DIFFUSE", "ENABLE_BLUE_NOISE", "ENABLE_DIFFUSE", "ENABLE_HEIGHT_FOG", "ENABLE_LIGHTNING", "USE_COLOR_FOG"
                    },
                    ShaderType.TransparentLight => new[]
                    {
                        "ENABLE_HEIGHT_FOG", "MULTIPLY_COLOR_WITH_ALPHA", "_ENABLE_MAIN_EFFECT_WHITE_BOOST"
                    },
                    _ => Array.Empty<string>()
                }
            };
        }

        internal readonly struct MaterialInfo
        {
            internal MaterialInfo(
                ShaderType shaderType,
                Material material,
                List<Track>? track, Material originalMaterial)
            {
                ShaderType = shaderType;
                Material = material;
                Track = track;
                OriginalMaterial = originalMaterial;
            }

            internal ShaderType ShaderType { get; }

            internal Material Material { get; }

            internal Material OriginalMaterial { get; }

            internal List<Track>? Track { get; }
        }
    }
}
