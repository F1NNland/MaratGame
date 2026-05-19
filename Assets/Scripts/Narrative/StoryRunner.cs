using System;
using MaratGame.Core;
using MaratGame.Data;
using UnityEngine;

namespace MaratGame.Narrative
{
    /// <summary>
    /// MonoBehaviour-обёртка над <see cref="StoryEngine"/> для сцены и отладки без Canvas.
    /// </summary>
    public sealed class StoryRunner : MonoBehaviour
    {
        static GUIStyle s_WrapLabel;

        [SerializeField] StoryDatabase database;
        [SerializeField] bool autoStartOnPlay = true;
        [SerializeField] bool resetStateOnStart = true;
        [SerializeField] bool logNodeChanges = true;
        [SerializeField] bool showDebugGui = true;

        StoryEngine _engine;
        bool _storyStarted;

        public StoryEngine Engine => _engine;
        public StoryDatabase Database => database;

        public event Action<StoryNodeData> OnNodeChanged;
        public event Action<StoryNodeData> OnNodeComplete;

        void Awake()
        {
            if (database == null)
            {
                Debug.LogError("[StoryRunner] StoryDatabase is not assigned.", this);
                enabled = false;
                return;
            }

            _engine = new StoryEngine(GameState.Instance, database);
            _engine.OnNodeChanged += HandleNodeChanged;
            _engine.OnNodeComplete += HandleNodeComplete;
        }

        void OnDestroy()
        {
            if (_engine == null)
                return;

            _engine.OnNodeChanged -= HandleNodeChanged;
            _engine.OnNodeComplete -= HandleNodeComplete;
        }

        void Start()
        {
            if (autoStartOnPlay)
                StartStory();
        }

        /// <summary>
        /// Старт сценария с <see cref="StoryDatabase.startNodeId"/>. На сцене Game вызывает <see cref="GameBootstrap"/>.
        /// </summary>
        public void StartStory()
        {
            if (_storyStarted || _engine == null)
                return;

            _storyStarted = true;

            if (resetStateOnStart)
                GameState.Instance.Reset();

            if (string.IsNullOrWhiteSpace(database.startNodeId))
            {
                Debug.LogError("[StoryRunner] StoryDatabase.startNodeId is empty.", this);
                return;
            }

            _engine.LoadNode(database.startNodeId);
        }

        public void LoadNode(string nodeId) => _engine?.LoadNode(nodeId);

        public void SelectChoice(int index) => _engine?.SelectChoice(index);

        void HandleNodeChanged(StoryNodeData node)
        {
            if (logNodeChanges)
                LogNode(node);

            OnNodeChanged?.Invoke(node);
        }

        void HandleNodeComplete(StoryNodeData node)
        {
            if (logNodeChanges)
                Debug.Log($"[StoryRunner] Node complete (no choices): {node.id}", this);

            OnNodeComplete?.Invoke(node);
        }

        void LogNode(StoryNodeData node)
        {
            var state = GameState.Instance;
            Debug.Log(
                $"[StoryRunner] Node={node.id} loc={state.CurrentLocationId} time={state.CurrentTime} " +
                $"respect={state.Stats.Respect}% calm={state.Stats.Calm}% decisions={state.DecisionsCount}\n" +
                $"  {node.speaker}: {node.bodyText}",
                this);
        }

        void OnGUI()
        {
            if (!showDebugGui || _engine?.CurrentNode == null)
                return;

            var node = _engine.CurrentNode;
            var choices = node.choices;
            if (choices == null || choices.Length == 0)
                return;

            const int width = 320;
            GUILayout.BeginArea(new Rect(10, 10, width, 400), GUI.skin.box);
            GUILayout.Label($"{node.chapterLabel} — {node.id}");
            s_WrapLabel ??= new GUIStyle(GUI.skin.label) { wordWrap = true };
            GUILayout.Label($"{node.speaker}: {node.bodyText}", s_WrapLabel);

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice != null && GUILayout.Button(choice.label))
                    SelectChoice(i);
            }

            GUILayout.EndArea();
        }
    }
}
