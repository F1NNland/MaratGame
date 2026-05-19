using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Панель диалога: спикер, портрет и текст.
    /// </summary>
    public sealed class DialogueView : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] RectTransform panel;
        [SerializeField] Image portraitImage;
        [SerializeField] TextMeshProUGUI speakerText;
        [SerializeField] TextMeshProUGUI bodyText;

        public CanvasGroup CanvasGroup => canvasGroup;
        public RectTransform Panel => panel;
        public Image PortraitImage => portraitImage;

        public void SetContent(string speaker, string body)
        {
            if (speakerText != null)
                speakerText.text = speaker ?? string.Empty;
            if (bodyText != null)
                bodyText.text = body ?? string.Empty;
        }

        public void SetPortrait(Sprite sprite)
        {
            if (portraitImage == null)
                return;

            if (sprite == null)
            {
                portraitImage.gameObject.SetActive(false);
                return;
            }

            portraitImage.sprite = sprite;
            portraitImage.preserveAspect = true;
            portraitImage.gameObject.SetActive(true);
        }

        public void OnDestroyCleanup()
        {
            if (canvasGroup != null)
                UiTweens.Kill(canvasGroup);
            if (panel != null)
                UiTweens.Kill(panel);
        }

        void OnDestroy() => OnDestroyCleanup();
    }
}
