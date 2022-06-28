using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Chroma.ChromaController;
using static Heck.HeckController;
using Object = UnityEngine.Object;

namespace Chroma.EnvironmentEnhancement
{
    internal class MaterialsManager
    {
        private static readonly Material _standardMaterial = InstantiateSharedMaterial(ShaderType.Standard);
        private static readonly Material _opaqueLightMaterial = InstantiateSharedMaterial(ShaderType.OpaqueLight);
        private static readonly Material _transparentLightMaterial = InstantiateSharedMaterial(ShaderType.TransparentLight);

        private readonly Dictionary<string, Track> _beatmapTracks;
        private readonly LazyInject<MaterialColorAnimator> _materialColorAnimator;

        [UsedImplicitly]
        private MaterialsManager(
            IReadonlyBeatmapData readonlyBeatmap,
            Dictionary<string, Track> beatmapTracks,
            LazyInject<MaterialColorAnimator> materialColorAnimator)
        {
            CustomBeatmapData beatmapData = (CustomBeatmapData)readonlyBeatmap;
            _beatmapTracks = beatmapTracks;
            _materialColorAnimator = materialColorAnimator;
            if (beatmapData.version2_6_0AndEarlier)
            {
                return;
            }

            IEnumerable<CustomData>? materialsData = beatmapData.customData
                .Get<List<object>>(MATERIALS)?
                .Cast<CustomData>();
            if (materialsData == null)
            {
                return;
            }

            foreach (CustomData customData in materialsData)
            {
                string name = customData.GetRequired<string>(NAME);
                CustomData materialData = customData.GetRequired<CustomData>(MATERIAL);
                MaterialInfos.Add(name, CreateMaterialInfo(materialData));
            }
        }

        internal Dictionary<string, MaterialInfo> MaterialInfos { get; } = new();

        internal MaterialInfo CreateMaterialInfo(CustomData customData)
        {
            Color color = CustomDataManager.GetColorFromData(customData) ?? new Color(0, 0, 0, 0);
            ShaderType shaderType = customData.GetStringToEnumRequired<ShaderType>(SHADER_PRESET);
            string[]? shaderKeywords = customData.Get<List<object>?>(SHADER_KEYWORDS)?.Cast<string>().ToArray();
            List<Track>? track = customData.GetNullableTrackArray(_beatmapTracks, false)?.ToList();

            Material originalMaterial = shaderType switch
            {
                ShaderType.OpaqueLight => _opaqueLightMaterial,
                ShaderType.TransparentLight => _transparentLightMaterial,
                _ => _standardMaterial
            };
            Material material = Object.Instantiate(originalMaterial);
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
                _ => "Custom/SimpleLit"
            }))
            {
                globalIlluminationFlags = GeometryFactory.IsLightType(shaderType)
                    ? MaterialGlobalIlluminationFlags.EmissiveIsBlack
                    : MaterialGlobalIlluminationFlags.RealtimeEmissive,
                enableInstancing = true,
                shaderKeywords = shaderType switch
                {
                    // Keywords found in RUE PC in BS 1.23
                    ShaderType.Standard => new[]
                    {
                        "DIFFUSE", "ENABLE_DIFFUSE", "ENABLE_FOG", "ENABLE_HEIGHT_FOG", "ENABLE_SPECULAR", "FOG",
                        "HEIGHT_FOG", "REFLECTION_PROBE_BOX_PROJECTION", "SPECULAR", "_EMISSION",
                        "_ENABLE_FOG_TINT", "_RIMLIGHT_NONE", "_ZWRITE_ON", "REFLECTION_PROBE", "LIGHT_FALLOFF"
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
