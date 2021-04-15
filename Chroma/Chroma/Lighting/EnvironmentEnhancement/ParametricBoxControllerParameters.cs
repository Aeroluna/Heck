namespace Chroma
{
    using System.Collections.Generic;
    using UnityEngine;

    internal class ParametricBoxControllerParameters
    {
        internal static Dictionary<ParametricBoxController, ParametricBoxControllerParameters> TransformParameters { get; } = new Dictionary<ParametricBoxController, ParametricBoxControllerParameters>();

        internal Vector3? Scale { get; private set; }

        internal Vector3? Position { get; private set; }

        internal static void SetTransformScale(ParametricBoxController parametricBoxController, Vector3 scale)
        {
            GetParameters(parametricBoxController).Scale = scale;
        }

        internal static void SetTransformPosition(ParametricBoxController parametricBoxController, Vector3 position)
        {
            GetParameters(parametricBoxController).Position = position;
        }

        private static ParametricBoxControllerParameters GetParameters(ParametricBoxController parametricBoxController)
        {
            if (!TransformParameters.TryGetValue(parametricBoxController, out ParametricBoxControllerParameters parameters))
            {
                parameters = new ParametricBoxControllerParameters();
                TransformParameters.Add(parametricBoxController, parameters);
            }

            return parameters;
        }
    }
}
