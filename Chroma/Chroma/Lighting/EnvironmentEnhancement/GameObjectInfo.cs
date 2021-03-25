namespace Chroma
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    internal class GameObjectInfo
    {
        internal GameObjectInfo(GameObject gameObject)
        {
            List<string> nameList = new List<string>();

            Transform transform = gameObject.transform;
            while (true)
            {
                int index;
                if (transform.parent != null)
                {
                    index = transform.GetSiblingIndex();
                }
                else
                {
                    // Why doesnt GetSiblingIndex work on root objects?
                    GameObject[] rootGameObjects = transform.gameObject.scene.GetRootGameObjects();
                    index = Array.IndexOf(rootGameObjects, transform.gameObject);
                }

                nameList.Add($"[{index}]{transform.name}");

                if (transform.parent == null)
                {
                    break;
                }

                transform = transform.parent;
            }

            nameList.Add($"{gameObject.scene.name}");
            nameList.Reverse();

            FullID = string.Join(".", nameList);
            if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
            {
                ChromaLogger.Log(FullID);
            }

            GameObject = gameObject;
        }

        internal string FullID { get; }

        internal GameObject GameObject { get; }
    }
}
