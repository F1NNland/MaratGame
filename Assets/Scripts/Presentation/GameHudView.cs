using MaratGame.Core;
using TMPro;
using UnityEngine;

namespace MaratGame.Presentation
{
    /// <summary>
    /// HUD: время, уважение, спокойствие, глава.
    /// </summary>
    public sealed class GameHudView : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TextMeshProUGUI timeText;
        [SerializeField] TextMeshProUGUI respectText;
        [SerializeField] TextMeshProUGUI calmText;
        [SerializeField] TextMeshProUGUI chapterText;
        [SerializeField] TextMeshProUGUI periodBadgeText;

        int _displayedRespect = GameDefaults.StartRespect;
        int _displayedCalm = GameDefaults.StartCalm;

        public CanvasGroup CanvasGroup => canvasGroup;

        public void PlayIntroFade()
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = 0f;
            UiTweens.Fade(canvasGroup, 1f, UiTweens.Slow);
        }

        public void BindInitialState(GameState state)
        {
            if (state == null)
                return;

            _displayedRespect = state.Stats.Respect;
            _displayedCalm = state.Stats.Calm;
            ApplyTime(state.CurrentTime);
            ApplyRespectInstant(_displayedRespect);
            ApplyCalmInstant(_displayedCalm);
        }

        public void SetChapter(string chapterLabel)
        {
            if (chapterText != null)
                chapterText.text = chapterLabel ?? string.Empty;
        }

        public void SetPeriodBadge(string badgeText)
        {
            if (periodBadgeText == null)
                return;

            var show = !string.IsNullOrWhiteSpace(badgeText);
            periodBadgeText.gameObject.SetActive(show);
            if (show)
                periodBadgeText.text = badgeText;
        }

        public void SetTime(string timeDisplay)
        {
            ApplyTime(timeDisplay);
        }

        public void AnimateStats(int respect, int calm)
        {
            if (respectText != null && respect != _displayedRespect)
            {
                UiTweens.Kill(respectText);
                UiTweens.Counter(_displayedRespect, respect, UiTweens.Normal, v =>
                {
                    _displayedRespect = v;
                    ApplyRespectInstant(v);
                });
            }
            else
            {
                _displayedRespect = respect;
                ApplyRespectInstant(respect);
            }

            if (calmText != null && calm != _displayedCalm)
            {
                UiTweens.Kill(calmText);
                UiTweens.Counter(_displayedCalm, calm, UiTweens.Normal, v =>
                {
                    _displayedCalm = v;
                    ApplyCalmInstant(v);
                });
            }
            else
            {
                _displayedCalm = calm;
                ApplyCalmInstant(calm);
            }
        }

        void ApplyTime(string timeDisplay)
        {
            if (timeText != null)
                timeText.text = timeDisplay ?? string.Empty;
        }

        void ApplyRespectInstant(int value)
        {
            if (respectText != null)
                respectText.text = $"Уважение {value}%";
        }

        void ApplyCalmInstant(int value)
        {
            if (calmText != null)
                calmText.text = $"Спокойствие {value}%";
        }

        void OnDestroy()
        {
            if (canvasGroup != null)
                UiTweens.Kill(canvasGroup);
            if (respectText != null)
                UiTweens.Kill(respectText);
            if (calmText != null)
                UiTweens.Kill(calmText);
        }
    }
}
