using System.Collections.Generic;
using UnityEngine;

namespace MaratGame.Data
{
    [CreateAssetMenu(fileName = "StoryDatabase", menuName = "MaratGame/Story Database")]
    public sealed class StoryDatabase : ScriptableObject
    {
        public string startNodeId;
        public List<StoryNodeData> nodes = new();

        public StoryNodeData GetNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId) || nodes == null)
                return null;

            foreach (var node in nodes)
            {
                if (node != null && node.id == nodeId)
                    return node;
            }

            return null;
        }
    }
}
