using System.Collections.Generic;
using System.Linq;
using Heck.Animation;
using UnityEngine;
using static Chroma.ChromaController;
using static Heck.NullableExtensions;

namespace Chroma.Animation
{
    internal static class AnimationHelper
    {
        // This method runs on each frame in the game scene, so avoid allocations (do NOT use Linq).
        internal static void GetColorOffset(PointDefinition<Vector4>? localColor, IReadOnlyList<Track>? tracks, float time, out Color? color)
        {
            Vector4? pathColor = localColor?.Interpolate(time);
            Vector4? colorVector;
            if (tracks != null)
            {
                if (tracks.Count > 1)
                {
                    Vector4? multPathColor = null;
                    Vector4? multColorVector = null;
                    bool hasPathColor = pathColor.HasValue;

                    foreach (Track track in tracks)
                    {
                        if (!hasPathColor)
                        {
                            multPathColor = MultVector4Nullables(multPathColor, track.GetVector4PathProperty(COLOR, time));
                        }

                        multColorVector = MultVector4Nullables(multColorVector, track.GetProperty<Vector4>(COLOR));
                    }

                    if (!hasPathColor)
                    {
                        pathColor = multPathColor;
                    }

                    colorVector = MultVector4Nullables(multColorVector, pathColor);
                }
                else
                {
                    Track track = tracks.First();
                    pathColor ??= track.GetVector4PathProperty(COLOR, time);
                    colorVector = MultVector4Nullables(track.GetProperty<Vector4>(COLOR), pathColor);
                }
            }
            else
            {
                colorVector = pathColor;
            }

            if (colorVector.HasValue)
            {
                Vector4 vectorValue = colorVector.Value;
                color = new Color(vectorValue.x, vectorValue.y, vectorValue.z, vectorValue.w);
            }
            else
            {
                color = null;
            }
        }
    }
}
