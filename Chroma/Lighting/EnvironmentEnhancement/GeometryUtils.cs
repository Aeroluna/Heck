using UnityEngine;

namespace Chroma.Lighting.EnvironmentEnhancement
{
    public class GeometryUtils
    {
        /// <summary>
        /// https://answers.unity.com/questions/1594750/is-there-a-premade-triangle-asset.html
        /// </summary>
        /// <returns></returns>
        public static Mesh CreateTriangleMesh()
        {
            Vector3[] vertices =
            {
                new(-0.5f, -0.5f, 0),
                new(0.5f, -0.5f, 0),
                new(0f, 0.5f, 0)
            };

            Vector2[] uv =
            {
                new(0, 0),
                new(1, 0),
                new(0.5f, 1)
            };

            int[] triangles = { 0, 1, 2 };

            Mesh mesh = new()
            {
                vertices = vertices,
                uv = uv,
                triangles = triangles
            };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}
