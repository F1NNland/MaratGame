using System;

namespace MaratGame.Data
{
    [Serializable]
    public sealed class StoryChoice
    {
        public string label;
        public string targetNodeId;
        public StatChangeEntry[] statChanges = Array.Empty<StatChangeEntry>();
        /// <summary>Флаги после выбора; если любой уже есть — выбор скрыт (хаб chapter2_cabinet_entry и т.п.).</summary>
        public string[] flagsToSet = Array.Empty<string>();
        public string[] requiredFlags = Array.Empty<string>();
        public StatRequirement[] requiredStats = Array.Empty<StatRequirement>();
        public string unavailableReason;
    }
}
