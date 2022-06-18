using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.EnvironmentEnhancement
{
    internal class GameObjectInfo
    {
        internal GameObjectInfo(GameObject gameObject)
        {
            List<string> nameList = new();

            Transform transform = gameObject.transform;
            while (true)
            {
                Transform parent = transform.parent;
                bool parentExist = parent != null;

                int index;
                if (parentExist)
                {
                    index = transform.GetSiblingIndex();
                }
                else
                {
                    // Why doesnt GetSiblingIndex work on root objects?
                    GameObject currentObject = transform.gameObject;
                    GameObject[] rootGameObjects = currentObject.scene.GetRootGameObjects();
                    index = Array.IndexOf(rootGameObjects, currentObject);
                }

                nameList.Add($"[{index}]{transform.name}");

                if (!parentExist)
                {
                    break;
                }

                transform = parent!;
            }

            nameList.Add($"{gameObject.scene.name}");
            nameList.Reverse();

            FullID = string.Join(".", nameList);
            GameObject = gameObject;
        }

        internal string FullID { get; }

        internal GameObject GameObject { get; }
    }
}
