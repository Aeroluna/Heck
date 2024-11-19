using UnityEngine;

namespace NoodleExtensions.Animation;

internal class MirrorParentTransform : MonoBehaviour
{
    private void Update()
    {
        Transform transform1 = transform;
        Transform parent = transform1.parent;
        transform1.localPosition = parent.localPosition;
        transform1.localRotation = parent.localRotation;
    }
}
