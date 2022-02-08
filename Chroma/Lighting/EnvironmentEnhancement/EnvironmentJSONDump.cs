using UnityEngine;

namespace Chroma.Lighting.EnvironmentEnhancement
{
    struct Vector3Json
    {
        public float x;
        public float y;
        public float z;

        public Vector3Json(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }
    }

    struct QuaternionJson
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public QuaternionJson(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }
    }

    struct GameObjectJSON
    {
        public readonly Vector3Json position;
        public readonly Vector3Json localPosition;
        public readonly QuaternionJson rotation;
        public readonly QuaternionJson localRotation;
        public readonly Vector3Json localScale;

        public GameObjectJSON(GameObject gameObject)
        {
            Transform transform = gameObject.transform;
            localPosition = new Vector3Json(transform.localPosition);
            position = new Vector3Json(transform.position);
            rotation = new QuaternionJson(transform.rotation);
            localRotation = new QuaternionJson(transform.localRotation);
            localScale = new Vector3Json(transform.localScale);
        }
    }
}
