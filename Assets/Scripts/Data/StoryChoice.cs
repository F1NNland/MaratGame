using System;

namespace MaratGame.Data
{
    [Serializable]
    public sealed class StoryChoice
    {
        public string label;
        public string targetNodeId;
        public StatChangeEntry[] statChanges = Array.Empty<StatChangeEntry>();
        public string[] flagsToSet = Array.Empty<string>();
    }
}
