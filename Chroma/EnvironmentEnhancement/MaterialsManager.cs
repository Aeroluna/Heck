using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Settings;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Chroma.ChromaController;
using Object = UnityEngine.Object;

namespace Chroma.EnvironmentEnhancement
{
    internal class MaterialsManager : IDisposable
    {
        private static readonly Material _standardMaterial = InstantiateSharedMaterial(ShaderType.Standard);
        private static readonly Material _opaqueLightMaterial = InstantiateSharedMaterial(ShaderType.OpaqueLight);
        private static readonly Material _transparentLightMaterial = InstantiateSharedMaterial(ShaderType.TransparentLight);
        private static readonly Material _baseWaterMaterial = InstantiateSharedMaterial(ShaderType.BaseWater);

        private readonly HashSet<Material> _createdMaterials = new();

        private readonly EnvironmentMaterialsManager _environmentMaterialsManager;
        private readonly Dictionary<string, Track> _beatmapTracks;
        private readonly LazyInject<MaterialColorAnimator> _materialColorAnimator;
        private readonly bool _v2;

        [UsedImplicitly]
        private MaterialsManager(
            EnvironmentMaterialsManager environmentMaterialsManager,
            IReadonlyBeatmapData readonlyBeatmap,
            Dictionary<string, Track> beatmapTracks,
            LazyInject<MaterialColorAnimator> materialColorAnimator,
            SavedEnvironmentLoader savedEnvironmentLoader,
            Config config)
        {
            CustomBeatmapData beatmapData = (CustomBeatmapData)readonlyBeatmap;
            _v2 = beatmapData.version2_6_0AndEarlier;
            _environmentMaterialsManager = environmentMaterialsManager;
            _beatmapTracks = beatmapTracks;
            _materialColorAnimator = materialColorAnimator;

            CustomData? materialsData = null;
            if (!config.EnvironmentEnhancementsDisabled)
            {
                materialsData = beatmapData.customData.Get<CustomData>(_v2 ? V2_MATERIALS : MATERIALS);
            }

            if (materialsData == null && config.CustomEnvironmentEnabled)
            {
                materialsData = savedEnvironmentLoader.SavedEnvironment?.Materials;
            }

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

        internal Dictionary<string, MaterialInfo> MaterialInfos { get; } = new();

        public void Dispose()
        {
            foreach (Material createdMaterial in _createdMaterials)
            {
                Object.Destroy(createdMaterial);
            }
        }

        internal MaterialInfo CreateMaterialInfo(CustomData customData)
        {
            Color? color = CustomDataManager.GetColorFromData(customData, _v2);
            ShaderType shaderType = customData.GetStringToEnumRequired<ShaderType>(_v2 ? V2_SHADER_PRESET : SHADER_PRESET);
            string[]? shaderKeywords = customData.Get<List<object>?>(_v2 ? V2_SHADER_KEYWORDS : SHADER_KEYWORDS)?.Cast<string>().ToArray();
            List<Track>? track = customData.GetNullableTrackArray(_beatmapTracks, _v2)?.ToList();

            Material originalMaterial = shaderType switch
            {
                ShaderType.Standard => _standardMaterial,
                ShaderType.OpaqueLight => _opaqueLightMaterial,
                ShaderType.TransparentLight => _transparentLightMaterial,
                ShaderType.BaseWater => _baseWaterMaterial,
                _ => _environmentMaterialsManager.EnvironmentMaterials.TryGetValue(shaderType, out Material foundMat) ? foundMat : throw new InvalidOperationException()
            };
            Material material = Object.Instantiate(originalMaterial);
            _createdMaterials.Add(material);
            if (color != null)
            {
                material.color = color.Value;
            }

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
                        "_ENABLE_FOG_TINT", "_RIMLIGHT_NONE",

                        // Added in BS 1.30
                        "_ACES_APPROACH_AFTER_EMISSIVE", "_DECALBLEND_ALPHABLEND", "_DISSOLVEAXIS_LOCALX", "_EMISSIONCOLORTYPE_FLAT",
                        "EMISSIONTEXTURE_NONE", "_ROTATE_UV_NONE", "_VERTEXMODE_NONE", "WHITEBOOSTTYPE_NONE", "ZWRITE_ON"
                    },
                    ShaderType.OpaqueLight => new[]
                    {
                        "DIFFUSE", "ENABLE_BLUE_NOISE", "ENABLE_DIFFUSE", "ENABLE_HEIGHT_FOG", "ENABLE_LIGHTNING", "USE_COLOR_FOG"
                    },
                    ShaderType.TransparentLight => new[]
                    {
                        "ENABLE_HEIGHT_FOG", "MULTIPLY_COLOR_WITH_ALPHA", "_ENABLE_MAIN_EFFECT_WHITE_BOOST"
                    },
                    ShaderType.BaseWater => new[]
                    {
                        "FOG", "HEIGHT_FOG", "INVERT_RIMLIGHT", "MASK_RED_IS_ALPHA", "NOISE_DITHERING",
                        "NORMAL_MAP", "REFLECTION_PROBE", "REFLECTION_PROBE_BOX_PROJECTION", "_DECALBLEND_ALPHABLEND",
                        "_DISSOLVEAXIS_LOCALX", "_EMISSIONCOLORTYPE_FLAT", "_EMISSIONTEXTURE_NONE",
                        "_RIMLIGHT_NONE", "_ROTATE_UV_NONE", "_VERTEXMODE_NONE", "_WHITEBOOSTTYPE_NONE",
                        "_ZWRITE_ON"
                    },
                    _ => Array.Empty<string>()
                },
                color = new Color(0, 0, 0, 0)
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
