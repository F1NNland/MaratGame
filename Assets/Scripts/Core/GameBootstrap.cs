using MaratGame.Narrative;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MaratGame.Core
{
    /// <summary>
    /// Сцена Game: сброс <see cref="GameState"/> и старт сценария через <see cref="StoryRunner"/>.
    /// </summary>
    /// <remarks>
    /// Step 04 UI handoff — Canvas/GameUIController, дети: HUD, LocationHeader, Background,
    /// DialoguePanel, ChoicesContainer. GameSystems: StoryRunner + GameBootstrap.
    /// </remarks>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] StoryRunner storyRunner;

        void Awake()
        {
            GameState.Instance.Reset();
        }

        void Start()
        {
            if (storyRunner == null)
                storyRunner = FindFirstObjectByType<StoryRunner>();

            if (storyRunner == null)
            {
                Debug.LogError("[GameBootstrap] StoryRunner not found on Game scene.", this);
                return;
            }

            storyRunner.StartStory();
        }

        /// <summary>Сброс прогресса и возврат в главное меню (MVP шаг 11).</summary>
        public static void ResetGame()
        {
            GameState.Instance.Reset();
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
