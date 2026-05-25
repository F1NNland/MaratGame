using System;
using System.Collections.Generic;
using System.Text;
using MaratGame.Core;
using MaratGame.Data;
using UnityEngine;

namespace MaratGame.Narrative
{
    /// <summary>
    /// Узел → onEnter → выборы → следующий узел. Без Unity UI.
    /// </summary>
    public sealed class StoryEngine
    {
        readonly GameState _state;
        readonly StoryDatabase _database;

        public StoryNodeData CurrentNode { get; private set; }

        public event Action<StoryNodeData> OnNodeChanged;
        public event Action<StoryNodeData> OnNodeComplete;

        public StoryEngine(GameState state, StoryDatabase database)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public void LoadNode(string nodeId) => LoadNodeInternal(nodeId, allowMiniStoryFallback: true);

        void LoadNodeInternal(string nodeId, bool allowMiniStoryFallback)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                throw new ArgumentException("Node id cannot be empty.", nameof(nodeId));

            var node = _database.GetNode(nodeId);
            if (node == null)
                throw new InvalidOperationException($"Story node not found: {nodeId}");

            if (!IsNodeAvailable(node, out var nodeReason))
            {
                if (allowMiniStoryFallback && TryLoadMiniStoryExit(node, $"Node unavailable: {nodeReason}"))
                    return;

                throw new InvalidOperationException($"Story node '{nodeId}' is unavailable: {nodeReason}");
            }

            ApplyOnEnterEffects(node.onEnterEffects);

            if (!string.IsNullOrWhiteSpace(node.timeDisplay))
                _state.SetTime(node.timeDisplay);

            if (!string.IsNullOrWhiteSpace(node.locationId))
                _state.SetLocation(node.locationId);

            if (node.dayBlock != DayBlock.Unspecified)
                _state.SetDayBlock(node.dayBlock);

            CurrentNode = node;
            ApplyMiniStoryAnalytics(node);
            OnNodeChanged?.Invoke(node);

            if (node.choices == null || node.choices.Length == 0)
            {
                if (allowMiniStoryFallback && TryLoadMiniStoryExit(node, "Mini-story node has no choices."))
                    return;

                OnNodeComplete?.Invoke(node);
            }
        }

        public void SelectChoice(int index)
        {
            var node = CurrentNode;
            if (node == null)
                throw new InvalidOperationException("No current node.");

            if (node.choices == null || index < 0 || index >= node.choices.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var choice = node.choices[index];
            if (choice == null)
            {
                if (TryLoadMiniStoryExit(node, $"Choice at index {index} is null."))
                    return;

                throw new InvalidOperationException($"Choice at index {index} is null.");
            }

            if (!IsChoiceAvailable(choice, out var reason))
            {
                if (TryLoadMiniStoryExit(node, $"Choice '{choice.label}' is unavailable: {reason}"))
                    return;

                throw new InvalidOperationException($"Choice '{choice.label}' is unavailable: {reason}");
            }

            ApplyChoiceStatChanges(choice.statChanges);
            ApplyFlags(choice.flagsToSet);
            _state.RecordDecision();

            if (string.IsNullOrWhiteSpace(choice.targetNodeId))
            {
                if (TryLoadMiniStoryExit(node, $"Choice '{choice.label}' has no targetNodeId."))
                    return;

                throw new InvalidOperationException($"Choice '{choice.label}' has no targetNodeId.");
            }

            try
            {
                LoadNodeInternal(choice.targetNodeId, allowMiniStoryFallback: true);
            }
            catch (Exception ex) when (TryLoadMiniStoryExit(node, $"Broken mini-story target '{choice.targetNodeId}': {ex.Message}"))
            {
                // Mini-story recovered by fallback to exit node.
            }
        }

        public StoryChoiceAvailability[] GetChoiceAvailability(StoryNodeData node = null)
        {
            node ??= CurrentNode;
            if (node?.choices == null || node.choices.Length == 0)
                return Array.Empty<StoryChoiceAvailability>();

            var result = new StoryChoiceAvailability[node.choices.Length];
            for (var i = 0; i < node.choices.Length; i++)
            {
                var choice = node.choices[i];
                if (choice == null)
                {
                    result[i] = new StoryChoiceAvailability(i, null, false, "Choice is null.");
                    continue;
                }

                var available = IsChoiceAvailable(choice, out var reason);
                result[i] = new StoryChoiceAvailability(i, choice, available, reason);
            }

            return result;
        }

        bool IsNodeAvailable(StoryNodeData node, out string reason)
        {
            if (node == null)
            {
                reason = "Node is null.";
                return false;
            }

            return CheckConditions(node.requiredFlags, node.requiredStats, null, out reason);
        }

        bool IsChoiceAvailable(StoryChoice choice, out string reason)
        {
            if (choice == null)
            {
                reason = "Choice is null.";
                return false;
            }

            if (!CheckConditions(choice.requiredFlags, choice.requiredStats, choice.unavailableReason, out reason))
                return false;

            if (IsChoiceAlreadyCompleted(choice))
            {
                reason = null;
                return false;
            }

            reason = null;
            return true;
        }

        /// <summary>Хаб-выборы с <see cref="StoryChoice.flagsToSet"/> — один раз (флаг уже стоит).</summary>
        bool IsChoiceAlreadyCompleted(StoryChoice choice)
        {
            if (choice.flagsToSet == null || choice.flagsToSet.Length == 0)
                return false;

            for (var i = 0; i < choice.flagsToSet.Length; i++)
            {
                var flag = choice.flagsToSet[i];
                if (string.IsNullOrWhiteSpace(flag))
                    continue;

                if (_state.Flags.HasFlag(flag))
                    return true;
            }

            return false;
        }

        bool CheckConditions(string[] requiredFlags, StatRequirement[] requiredStats, string fallbackReason, out string reason)
        {
            var missingFlags = GetMissingFlags(requiredFlags);
            var statIssues = GetStatIssues(requiredStats);

            if (missingFlags.Count == 0 && statIssues.Count == 0)
            {
                reason = null;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(fallbackReason))
            {
                reason = fallbackReason;
                return false;
            }

            var sb = new StringBuilder(96);
            if (missingFlags.Count > 0)
                sb.Append("missing flags: ").Append(string.Join(", ", missingFlags));

            if (statIssues.Count > 0)
            {
                if (sb.Length > 0)
                    sb.Append("; ");
                sb.Append("stat conditions: ").Append(string.Join("; ", statIssues));
            }

            reason = sb.ToString();
            return false;
        }

        List<string> GetMissingFlags(string[] requiredFlags)
        {
            var missing = new List<string>();
            if (requiredFlags == null)
                return missing;

            for (var i = 0; i < requiredFlags.Length; i++)
            {
                var flag = requiredFlags[i];
                if (string.IsNullOrWhiteSpace(flag))
                    continue;

                if (!_state.Flags.HasFlag(flag))
                    missing.Add(flag);
            }

            return missing;
        }

        List<string> GetStatIssues(StatRequirement[] requiredStats)
        {
            var issues = new List<string>();
            if (requiredStats == null)
                return issues;

            for (var i = 0; i < requiredStats.Length; i++)
            {
                var req = requiredStats[i];
                var value = GetStatValue(req.stat);
                if (req.useMin && value < req.minValue)
                    issues.Add($"{req.stat}<{req.minValue} (now {value})");
                if (req.useMax && value > req.maxValue)
                    issues.Add($"{req.stat}>{req.maxValue} (now {value})");
            }

            return issues;
        }

        int GetStatValue(StatType stat)
        {
            return stat switch
            {
                StatType.Respect => _state.Stats.Respect,
                StatType.Calm => _state.Stats.Calm,
                StatType.Chaos => _state.Stats.Chaos,
                _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
            };
        }

        void ApplyOnEnterEffects(IReadOnlyList<StatChangeEntry> entries)
        {
            if (entries == null)
                return;

            foreach (var entry in entries)
                _state.ApplyStatChange(entry.ToCore());
        }

        void ApplyChoiceStatChanges(StatChangeEntry[] entries)
        {
            if (entries == null)
                return;

            foreach (var entry in entries)
                _state.ApplyStatChange(entry.ToCore());
        }

        void ApplyFlags(string[] flags)
        {
            if (flags == null)
                return;

            foreach (var flag in flags)
                _state.Flags.SetFlag(flag);
        }

        void ApplyMiniStoryAnalytics(StoryNodeData node)
        {
            if (node == null || !node.isMiniStory || string.IsNullOrWhiteSpace(node.miniStoryId))
                return;

            if (node.miniStoryRole == MiniStoryRole.Entry)
                _state.Flags.SetFlag(MiniStoryFramework.AnalyticsEntryPrefix + node.miniStoryId);
            else if (node.miniStoryRole == MiniStoryRole.Exit)
                _state.Flags.SetFlag(MiniStoryFramework.AnalyticsExitPrefix + node.miniStoryId);
        }

        bool TryLoadMiniStoryExit(StoryNodeData node, string reason)
        {
            if (node == null || !node.isMiniStory)
                return false;

            var exitNodeId = ResolveMiniStoryExitNodeId(node);
            if (string.IsNullOrWhiteSpace(exitNodeId))
            {
                Debug.LogError($"[StoryEngine] {reason}. Mini-story exit is missing for node '{node.id}'.");
                return false;
            }

            if (exitNodeId == node.id)
            {
                Debug.LogError($"[StoryEngine] {reason}. Mini-story exit points to itself: '{node.id}'.");
                return false;
            }

            Debug.LogError($"[StoryEngine] {reason}. Recovering mini-story '{node.miniStoryId}' via exit '{exitNodeId}'.");
            LoadNodeInternal(exitNodeId, allowMiniStoryFallback: false);
            return true;
        }

        static string ResolveMiniStoryExitNodeId(StoryNodeData node)
        {
            if (node == null)
                return null;

            if (!string.IsNullOrWhiteSpace(node.miniStoryExitNodeId))
                return node.miniStoryExitNodeId;

            return null;
        }
    }
}
