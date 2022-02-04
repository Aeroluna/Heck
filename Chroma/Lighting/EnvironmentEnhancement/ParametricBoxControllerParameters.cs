using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Lighting.EnvironmentEnhancement
{
    internal class ParametricBoxControllerParameters
    {
        internal Dictionary<ParametricBoxController, ParametricBoxControllerParameters> TransformParameters { get; } = new();

        internal Vector3? Scale { get; private set; }

        internal Vector3? Position { get; private set; }

        internal void SetTransformScale(ParametricBoxController parametricBoxController, Vector3 scale)
        {
            GetParameters(parametricBoxController).Scale = scale;
        }

        internal void SetTransformPosition(ParametricBoxController parametricBoxController, Vector3 position)
        {
            GetParameters(parametricBoxController).Position = position;
        }

        private ParametricBoxControllerParameters GetParameters(ParametricBoxController parametricBoxController)
        {
            if (TransformParameters.TryGetValue(parametricBoxController, out ParametricBoxControllerParameters parameters))
            {
                return parameters;
            }

            parameters = new ParametricBoxControllerParameters();
            TransformParameters.Add(parametricBoxController, parameters);

            return parameters;
        }
    }
}
