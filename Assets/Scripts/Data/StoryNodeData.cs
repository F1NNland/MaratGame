using System.Collections.Generic;
using UnityEngine;

namespace MaratGame.Data
{
    [CreateAssetMenu(fileName = "StoryNode", menuName = "MaratGame/Story Node")]
    public sealed class StoryNodeData : ScriptableObject
    {
        public string id;
        public string locationId;
        public string timeDisplay;
        public string chapterLabel;

        [TextArea(3, 8)]
        public string bodyText;

        public string speaker;

        /// <summary>characterId из ASSETS.md — портрет в DialogueView (опционально).</summary>
        public string portraitCharacterId;

        public List<StatChangeEntry> onEnterEffects = new();
        public StoryChoice[] choices = System.Array.Empty<StoryChoice>();
    }
}
