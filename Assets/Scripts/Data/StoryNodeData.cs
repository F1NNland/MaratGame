using System.Collections.Generic;
using MaratGame.Core;
using UnityEngine;

namespace MaratGame.Data
{
    [CreateAssetMenu(fileName = "StoryNode", menuName = "MaratGame/Story Node")]
    public sealed class StoryNodeData : ScriptableObject
    {
        public string id;
        public string locationId;
        public string timeDisplay;
        public DayBlock dayBlock;
        public string chapterLabel;

        [TextArea(3, 8)]
        public string bodyText;

        public string speaker;
        public StoryUiMode uiMode = StoryUiMode.Dialogue;
        public bool isMiniStory;
        public string miniStoryId;
        public MiniStoryRole miniStoryRole;
        public string miniStoryEntryNodeId;
        public string[] miniStoryContentNodeIds = System.Array.Empty<string>();
        public string miniStoryExitNodeId;

        /// <summary>characterId из ASSETS.md — портрет в DialogueView (опционально).</summary>
        public string portraitCharacterId;
        public string[] requiredFlags = System.Array.Empty<string>();
        public StatRequirement[] requiredStats = System.Array.Empty<StatRequirement>();
        public MediaSlotType mediaSlot;
        public string mediaPath;

        public List<StatChangeEntry> onEnterEffects = new();
        public StoryChoice[] choices = System.Array.Empty<StoryChoice>();
    }
}
