using UnityEngine;

namespace Chroma.Extras;

internal static class ChromaUtils
{
    internal static Mesh? TriangleMesh { get; set; } // used for caching the triangle mesh, for performance reasons.

    /// <summary>
    ///     https://answers.unity.com/questions/1594750/is-there-a-premade-triangle-asset.html
    /// </summary>
    internal static Mesh CreateTriangleMesh()
    {
        if (TriangleMesh != null)
        {
            return TriangleMesh;
        }

        Vector3[] vertices =
        [
            new(-0.5f, -0.5f, 0),
            new(0.5f, -0.5f, 0),
            new(0f, 0.5f, 0)
        ];

        Vector2[] uv =
        [
            new(0, 0),
            new(1, 0),
            new(0.5f, 1)
        ];

        int[] triangles = [0, 1, 2];

        TriangleMesh = new Mesh
        {
            vertices = vertices,
            uv = uv,
            triangles = triangles
        };
        TriangleMesh.RecalculateBounds();
        TriangleMesh.RecalculateNormals();
        TriangleMesh.RecalculateTangents();

        return TriangleMesh;
    }

    internal static Color MultAlpha(this Color color, float alpha)
    {
        return color.ColorWithAlpha(color.a * alpha);
    }
}
