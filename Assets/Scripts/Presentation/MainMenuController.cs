using MaratGame.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Главное меню MVP (шаг 06): «НАЧАТЬ ИГРУ» → Game; на Game — <see cref="GameBootstrap"/> сбрасывает state.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        const float StartButtonDelay = 0.15f;

        [SerializeField] CanvasGroup menuGroup;
        [SerializeField] RectTransform titleRect;
        [SerializeField] RectTransform startButtonRect;
        [SerializeField] Button startButton;

        Vector2 _titleRestPosition;

        void Awake()
        {
            if (titleRect != null)
                _titleRestPosition = titleRect.anchoredPosition;
        }

        void Start()
        {
            PlayIntro();
        }

        void PlayIntro()
        {
            if (menuGroup != null)
            {
                menuGroup.alpha = 0f;
                UiTweens.Fade(menuGroup, 1f, UiTweens.Slow);
            }

            if (titleRect != null)
            {
                titleRect.anchoredPosition = _titleRestPosition + new Vector2(0f, -40f);
                UiTweens.SlideAnchored(titleRect, _titleRestPosition, UiTweens.Normal);
            }

            if (startButtonRect != null)
                UiTweens.StaggerChoices(new[] { startButtonRect }, UiTweens.Fast, StartButtonDelay);
        }

        /// <summary>Вызывается кнопкой «НАЧАТЬ ИГРУ».</summary>
        public void OnStartGameClicked()
        {
            SceneManager.LoadScene(SceneNames.Game);
        }

        void OnDestroy()
        {
            if (menuGroup != null)
                UiTweens.Kill(menuGroup);
            if (titleRect != null)
                UiTweens.Kill(titleRect);
            if (startButtonRect != null)
                UiTweens.Kill(startButtonRect);
        }
    }
}
