using UnityEngine;

namespace MaratGame.Agent
{
    static class AgentUiPaths
    {
        public static string GetHierarchyPath(GameObject go)
        {
            if (go == null)
                return "";

            var parts = new System.Collections.Generic.List<string>(8);
            var current = go.transform;
            while (current != null)
            {
                parts.Add(current.name);
                current = current.parent;
            }

            parts.Reverse();
            return string.Join("/", parts);
        }

        public static GameObject FindByPath(Transform sceneRoot, string objectPath)
        {
            if (sceneRoot == null || string.IsNullOrWhiteSpace(objectPath))
                return null;

            var parts = objectPath.Split('/');
            if (parts.Length == 0 || parts[0] != sceneRoot.name)
                return null;

            var current = sceneRoot;
            for (var i = 1; i < parts.Length; i++)
            {
                Transform next = null;
                for (var c = 0; c < current.childCount; c++)
                {
                    var child = current.GetChild(c);
                    if (child.name == parts[i])
                    {
                        next = child;
                        break;
                    }
                }

                if (next == null)
                    return null;

                current = next;
            }

            return current.gameObject;
        }
    }
}
