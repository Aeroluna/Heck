using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Colorizer;
using Chroma.Extras;
using Chroma.HarmonyPatches.Colorizer.Initialize;
using Chroma.HarmonyPatches.EnvironmentComponent;
using CustomJSONData.CustomBeatmap;
using Heck;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;
using static Chroma.ChromaController;
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
        Triangle,
        Custom
    }

    internal enum ShaderType
    {
        Standard,
        OpaqueLight,
        TransparentLight
    }

    internal struct MeshData
    {
        public Vector3[] Vertices; // vertex positions
        public Vector2[]? Uv; // texture coordinates UV0. must be in 0-1 range
        public int[]? Triangles; // must be in order of vertices and multiple of 3

        public MeshData(CustomData customData, bool v2)
        {
            Vertices = customData.GetRequiredFromArrayOfFloatArray(v2 ? V2_VERTICES : VERTICES, v => new Vector3(v[0], v[1], v[2])).ToArray();
            Uv = customData.GetFromArrayOfFloatArray(v2 ? V2_UV : UV, v => new Vector2(v[0], v[1]))?.ToArray();
            Triangles = customData.Get<IEnumerable<object>>(v2 ? V2_TRIANGLES : TRIANGLES)?.Select(Convert.ToInt32).ToArray();
        }

        public Mesh ToMesh()
        {
            Mesh mesh = new()
            {
                vertices = Vertices,
            };

            if (Uv != null)
            {
                mesh.uv = Uv;
            }

            if (Triangles != null)
            {
                mesh.triangles = Triangles;
            }

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }
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
        }

        internal static bool IsLightType(ShaderType shaderType)
        {
            return shaderType is ShaderType.OpaqueLight or ShaderType.TransparentLight;
        }

        internal GameObject Create(CustomData customData)
        {
            GeometryType geometryType = customData.GetStringToEnumRequired<GeometryType>(_v2 ? V2_GEOMETRY_TYPE : GEOMETRY_TYPE);
            bool collision = customData.Get<bool?>(_v2 ? V2_COLLISION : COLLISION) ?? false;

            object materialData = customData.GetRequired<object>(_v2 ? V2_MATERIAL : MATERIAL);
            MaterialsManager.MaterialInfo materialInfo = materialData switch
            {
                string name => _materialsManager.MaterialInfos[name],
                CustomData data => _materialsManager.CreateMaterialInfo(data),
                _ => throw new InvalidOperationException($"Could not read [{MATERIAL}].")
            };
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
                GeometryType.Custom => PrimitiveType.Quad,
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

            Mesh? customMesh = geometryType switch
            {
                GeometryType.Custom => new MeshData(customData.GetRequired<CustomData>(_v2 ? V2_MESH : MESH), _v2).ToMesh(),
                GeometryType.Triangle => ChromaUtils.CreateTriangleMesh(),
                _ => null
            };

            if (customMesh != null)
            {
                gameObject.GetComponent<MeshFilter>().sharedMesh = customMesh;

                if (collision)
                {
                    MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
                    if (meshCollider != null)
                    {
                        meshCollider.sharedMesh = customMesh;
                    }
                }
            }

            if (!collision)
            {
                // destroy colliders
                Object.Destroy(gameObject.GetComponent<Collider>());
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
    }
}
