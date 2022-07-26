using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Colorizer;
using Chroma.Extras;
using Chroma.HarmonyPatches.Colorizer.Initialize;
using Chroma.HarmonyPatches.EnvironmentComponent;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;
using static Chroma.ChromaController;
using Logger = IPA.Logging.Logger;
using Object = UnityEngine.Object;

namespace Chroma.EnvironmentEnhancement
{
    // ReSharper disable UnusedMember.Global
    internal enum GeometryType
    {
        Sphere,
        Capsule,
        Cylinder,
        Cube,
        Plane,
        Quad,
        Triangle
    }

    internal enum ShaderType
    {
        Standard,
        OpaqueLight,
        TransparentLight,
        BaseWater,
        BillieWater,
        BTSPillar,
        InterscopeConcrete,
        InterscopeCar,
        Obstacle,
        WaterfallMirror
    }

    internal class GeometryFactory
    {
        private static readonly FieldAccessor<TubeBloomPrePassLight, BoolSO>.Accessor _mainEffectPostProcessEnabledAccessor = FieldAccessor<TubeBloomPrePassLight, BoolSO>.GetAccessor("_mainEffectPostProcessEnabled");
        private static readonly FieldAccessor<TubeBloomPrePassLight, ParametricBoxController>.Accessor _parametricBoxControllerAccessor = FieldAccessor<TubeBloomPrePassLight, ParametricBoxController>.GetAccessor("_parametricBoxController");
        private static readonly FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.Accessor _lightTypeAccessor = FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.GetAccessor("_lightType");
        private static readonly FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.Accessor _registeredWithLightTypeAccessor = FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.GetAccessor("_registeredWithLightType");
        private static readonly FieldAccessor<TubeBloomPrePassLightWithId, TubeBloomPrePassLight>.Accessor _tubeBloomPrePassLightAccessor = FieldAccessor<TubeBloomPrePassLightWithId, TubeBloomPrePassLight>.GetAccessor("_tubeBloomPrePassLight");
        private static readonly FieldAccessor<ParametricBoxController, MeshRenderer>.Accessor _meshRendererAccessor = FieldAccessor<ParametricBoxController, MeshRenderer>.GetAccessor("_meshRenderer");

        private readonly IInstantiator _instantiator;
        private readonly bool _v2;
        private readonly MaterialsManager _materialsManager;
        private readonly LightWithIdRegisterer _lightWithIdRegisterer;
        private readonly ParametricBoxControllerTransformOverride _parametricBoxControllerTransformOverride;
        private readonly IDictionary<Mirror, MeshRenderer> mirrors;
        private TubeBloomPrePassLight? _originalTubeBloomPrePassLight = Resources.FindObjectsOfTypeAll<TubeBloomPrePassLight>().FirstOrDefault();

        [UsedImplicitly]
        private GeometryFactory(
            IInstantiator instantiator,
            IReadonlyBeatmapData beatmapData,
            MaterialsManager materialsManager,
            LightColorizerManager lightColorizerManager,
            LightWithIdRegisterer lightWithIdRegisterer,
            ParametricBoxControllerTransformOverride parametricBoxControllerTransformOverride)
        {
            _instantiator = instantiator;
            _v2 = ((CustomBeatmapData)beatmapData).version2_6_0AndEarlier;
            _materialsManager = materialsManager;
            _lightWithIdRegisterer = lightWithIdRegisterer;
            _parametricBoxControllerTransformOverride = parametricBoxControllerTransformOverride;
            mirrors = Resources.FindObjectsOfTypeAll<Mirror>().ToDictionary(m => m, m => m.GetComponent<MeshRenderer>());
        }

        internal static bool IsLightType(ShaderType shaderType)
        {
            return shaderType is ShaderType.OpaqueLight or ShaderType.TransparentLight or ShaderType.BillieWater;
        }

        internal static bool IsMirrorType(ShaderType shaderType)
        {
            return shaderType is ShaderType.BaseWater or ShaderType.WaterfallMirror;
        }

        internal GameObject Create(CustomData customData)
        {
            GeometryType geometryType = customData.GetStringToEnumRequired<GeometryType>(_v2 ? V2_GEOMETRY_TYPE : GEOMETRY_TYPE);
            bool collision = customData.Get<bool?>(_v2 ? V2_COLLISION : COLLISION) ?? false;

            object materialData = customData.GetRequired<object>(_v2 ? V2_MATERIAL : MATERIAL);
            MaterialsManager.MaterialInfo materialInfo = _materialsManager.GetMaterial(materialData);
            ShaderType shaderType = materialInfo.ShaderType;

            PrimitiveType primitiveType = geometryType switch
            {
                GeometryType.Sphere => PrimitiveType.Sphere,
                GeometryType.Capsule => PrimitiveType.Capsule,
                GeometryType.Cylinder => PrimitiveType.Cylinder,
                GeometryType.Cube => PrimitiveType.Cube,
                GeometryType.Plane => PrimitiveType.Plane,
                GeometryType.Quad => PrimitiveType.Quad,
                GeometryType.Triangle => PrimitiveType.Quad,
                _ => throw new ArgumentOutOfRangeException($"Geometry type {geometryType} does not match a primitive!", nameof(geometryType))
            };

            GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
            gameObject.name = $"{geometryType}{shaderType}";
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            gameObject.layer = 14;

            // Disable expensive shadows
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            // Shared material is usually better performance as far as I know
            meshRenderer.sharedMaterial = materialInfo.Material;

            if (geometryType == GeometryType.Triangle)
            {
                Mesh mesh = ChromaUtils.CreateTriangleMesh();
                gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
                if (collision)
                {
                    MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
                    if (meshCollider != null)
                    {
                        meshCollider.sharedMesh = mesh;
                    }
                }
            }

            if (!collision)
            {
                // destroy colliders
                Object.Destroy(gameObject.GetComponent<Collider>());
            }

            if (IsMirrorType(shaderType))
            {
                AddMirror(materialInfo.OriginalMaterial, gameObject);
            }

            // Handle light preset
            if (!IsLightType(shaderType))
            {
                return gameObject;
            }

            // Stop TubeBloomPrePassLight from running OnEnable before I can set the fields
            gameObject.SetActive(false);

            TubeBloomPrePassLight tubeBloomPrePassLight = gameObject.AddComponent<TubeBloomPrePassLight>();
            ParametricBoxController parametricBoxController = gameObject.AddComponent<ParametricBoxController>();
            _parametricBoxControllerTransformOverride.UpdatePosition(parametricBoxController);
            _parametricBoxControllerTransformOverride.UpdateScale(parametricBoxController);
            _meshRendererAccessor(ref parametricBoxController) = meshRenderer;

            if (_originalTubeBloomPrePassLight != null)
            {
                _mainEffectPostProcessEnabledAccessor(ref tubeBloomPrePassLight) =
                    _mainEffectPostProcessEnabledAccessor(ref _originalTubeBloomPrePassLight);

                BloomPrePassLight bloomPrePassLight = tubeBloomPrePassLight;
                BloomPrePassLight originalBloomPrePassLight = _originalTubeBloomPrePassLight;

                _lightTypeAccessor(ref bloomPrePassLight) = _lightTypeAccessor(ref originalBloomPrePassLight);
                _registeredWithLightTypeAccessor(ref bloomPrePassLight) =
                    _registeredWithLightTypeAccessor(ref originalBloomPrePassLight);
            }
            else
            {
                throw new InvalidOperationException($"[{nameof(_originalTubeBloomPrePassLight)}] was null.");
            }

            _parametricBoxControllerAccessor(ref tubeBloomPrePassLight) = parametricBoxController;
            TubeBloomPrePassLightWithId lightWithId = _instantiator.InstantiateComponent<TubeBloomPrePassLightWithId>(gameObject);
            _tubeBloomPrePassLightAccessor(ref lightWithId) = tubeBloomPrePassLight;
            _lightWithIdRegisterer.MarkForTableRegister(lightWithId);

            gameObject.SetActive(true);

            return gameObject;
        }

        internal void AddMirror(Material originalMaterial, GameObject gameObject)
        {
            (Mirror? oldMirror, MeshRenderer? mesh) = mirrors.FirstOrDefault(mirrorMeshPair =>
                mirrorMeshPair.Value.sharedMaterial == originalMaterial);

            if (oldMirror == null || mesh == null)
            {
                Log.Logger.Log($"Unable to place mirror onto {gameObject.name}", Logger.Level.Warning);
                return;
            }

            gameObject.SetActive(false);
            Mirror newMirror = oldMirror.CopyComponent<Mirror>(gameObject);

            newMirror.SetField("_mirrorRenderer",  oldMirror.GetField<MirrorRendererSO, Mirror>("_mirrorRenderer"));
            newMirror.SetField("_mirrorMaterial",  oldMirror.GetField<Material, Mirror>("_mirrorMaterial"));
            newMirror.SetField("_noMirrorMaterial",  oldMirror.GetField<Material, Mirror>("_noMirrorMaterial"));
            newMirror.SetField("_renderer", gameObject.GetComponent<MeshRenderer>());
            gameObject.SetActive(true);
        }
    }
}
