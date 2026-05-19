using System;
using System.Collections.Generic;
using MaratGame.Core;
using MaratGame.Data;

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

        public void LoadNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                throw new ArgumentException("Node id cannot be empty.", nameof(nodeId));

            var node = _database.GetNode(nodeId);
            if (node == null)
                throw new InvalidOperationException($"Story node not found: {nodeId}");

            ApplyOnEnterEffects(node.onEnterEffects);

            if (!string.IsNullOrWhiteSpace(node.timeDisplay))
                _state.SetTime(node.timeDisplay);

            if (!string.IsNullOrWhiteSpace(node.locationId))
                _state.SetLocation(node.locationId);

            CurrentNode = node;
            OnNodeChanged?.Invoke(node);

            if (node.choices == null || node.choices.Length == 0)
                OnNodeComplete?.Invoke(node);
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
                throw new InvalidOperationException($"Choice at index {index} is null.");

            ApplyChoiceStatChanges(choice.statChanges);
            ApplyFlags(choice.flagsToSet);
            _state.RecordDecision();

            if (string.IsNullOrWhiteSpace(choice.targetNodeId))
                throw new InvalidOperationException($"Choice '{choice.label}' has no targetNodeId.");

            LoadNode(choice.targetNodeId);
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
    }
}
