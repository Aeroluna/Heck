using SongCore;
using UnityEngine;

namespace Chroma.Extras
{
    internal static class ChromaUtils
    {
        internal static void SetSongCoreCapability(string capability, bool enabled = true)
        {
            if (enabled)
            {
                Collections.RegisterCapability(capability);
            }
            else
            {
                Collections.DeregisterizeCapability(capability);
            }
        }

        internal static Color MultAlpha(this Color color, float alpha)
        {
            return color.ColorWithAlpha(color.a * alpha);
        }

        /// <summary>
        /// https://answers.unity.com/questions/1594750/is-there-a-premade-triangle-asset.html
        /// </summary>
        internal static Mesh CreateTriangleMesh()
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
