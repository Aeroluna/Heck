using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
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
        private static readonly Material _billieWaterMaterial = InstantiateSharedMaterial(ShaderType.BillieWater);

        private readonly HashSet<Material> _createdMaterials = new();

        private readonly Dictionary<string, Track> _beatmapTracks;
        private readonly LazyInject<MaterialColorAnimator> _materialColorAnimator;
        private readonly bool _v2;

        private Dictionary<string, MaterialInfo> MaterialInfos { get; } = new();

        private Dictionary<string, Material> EnvironmentMaterialInfos { get; } = new();

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

            SetupEnvironmentMaterials();

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

        private void SetupEnvironmentMaterials()
        {
            // TODO: Load environemnt scenes so we can get all possible materials. A necessary evil :)
            // This is the only way I could think of for getting materials by name
            Material[] environmentMaterials = Resources.FindObjectsOfTypeAll<Material>();

            void GetEnvironmentMaterial(string key, string name)
            {
                // I use contains here because I can't be bothered to care about identical names
                Material? material = environmentMaterials.FirstOrDefault(e => e.name.Contains(name));
                if (material != null)
                {
                    EnvironmentMaterialInfos[key] = material;
                    Log.Logger.Log($"Found {name} material in current environment", Logger.Level.Info);
                }
                else
                {
                    Log.Logger.Log($"Could not find {name} material in current environment", Logger.Level.Info);
                }
            }


            GetEnvironmentMaterial("BTSPillar", "BTSDarkEnvironmentWithHeightFog");
            GetEnvironmentMaterial("BillieWater", "WaterfallFalling");
            GetEnvironmentMaterial("InterscopeConcrete", "Concrete2");
            GetEnvironmentMaterial("InterscopeCar", "Car");
            GetEnvironmentMaterial("Obstacle", "ObstacleCoreHD");
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
            string? environmentMaterial = customData.Get<string?>(_v2 ? V2_ENVIRONMENT_MATERIAL : ENVIRONMENT_MATERIAL);

            // if the environment material does not exist
            // it fallbacks to shaderType
            // this is because I have no idea if all materials will be available
            // on all environments
            Material originalMaterial = null!;
            if (environmentMaterial != null)
            {
                if (!EnvironmentMaterialInfos.TryGetValue(environmentMaterial, out originalMaterial))
                {
                    Log.Logger.Log($"Could not find {environmentMaterial} in environment materials, falling back to {shaderType}", Logger.Level.Warning);
                }
            }

            if (originalMaterial == null)
            {
                originalMaterial = shaderType switch
                {
                    ShaderType.OpaqueLight => _opaqueLightMaterial,
                    ShaderType.TransparentLight => _transparentLightMaterial,
                    ShaderType.BillieWater => _billieWaterMaterial,
                    _ => _standardMaterial
                };
            }

            Material material = Object.Instantiate(originalMaterial);
            _createdMaterials.Add(material);
            material.color = color;
            if (shaderKeywords != null)
            {
                material.shaderKeywords = shaderKeywords;
            }

            MaterialInfo materialInfo = new(shaderType, material, track);
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
                ShaderType.BillieWater => "Custom/WaterLit",
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
                    ShaderType.BillieWater => new[]
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
                List<Track>? track)
            {
                ShaderType = shaderType;
                Material = material;
                Track = track;
            }

            internal ShaderType ShaderType { get; }

            internal Material Material { get; }

            internal List<Track>? Track { get; }
        }
    }
}
