using MaratGame.Core;
using TMPro;
using UnityEngine;

namespace MaratGame.Presentation
{
    /// <summary>
    /// HUD по макету Figma: время, заголовок главы (отдельно), три стата с % справа.
    /// </summary>
    public sealed class GameHudView : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TextMeshProUGUI timeText;
        [SerializeField] TextMeshProUGUI respectText;
        [SerializeField] TextMeshProUGUI calmText;
        [SerializeField] TextMeshProUGUI chaosText;
        [SerializeField] TextMeshProUGUI chapterText;
        [SerializeField] TextMeshProUGUI periodBadgeText;

        int _displayedRespect = GameDefaults.StartRespect;
        int _displayedCalm = GameDefaults.StartCalm;
        int _displayedChaos;

        public CanvasGroup CanvasGroup => canvasGroup;

        void Awake()
        {
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;
        }

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
            _displayedChaos = state.Stats.Chaos;
            ApplyTime(state.CurrentTime);
            ApplyRespectInstant(_displayedRespect);
            ApplyCalmInstant(_displayedCalm);
            ApplyChaosInstant(_displayedChaos);
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

        public void SetTime(string timeDisplay) => ApplyTime(timeDisplay);

        public void AnimateStats(int respect, int calm, int chaos = 0)
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

            if (chaosText == null)
                return;

            if (chaos != _displayedChaos)
            {
                UiTweens.Kill(chaosText);
                UiTweens.Counter(_displayedChaos, chaos, UiTweens.Normal, v =>
                {
                    _displayedChaos = v;
                    ApplyChaosInstant(v);
                });
            }
            else
            {
                _displayedChaos = chaos;
                ApplyChaosInstant(chaos);
            }
        }

        void ApplyTime(string timeDisplay)
        {
            if (timeText != null)
                timeText.text = timeDisplay ?? string.Empty;
        }

        static void ApplyPercent(TextMeshProUGUI text, int value)
        {
            if (text != null)
                text.text = $"{value}%";
        }

        void ApplyRespectInstant(int value) => ApplyPercent(respectText, value);

        void ApplyCalmInstant(int value) => ApplyPercent(calmText, value);

        void ApplyChaosInstant(int value) => ApplyPercent(chaosText, value);

        void OnDestroy()
        {
            if (canvasGroup != null)
                UiTweens.Kill(canvasGroup);
            if (respectText != null)
                UiTweens.Kill(respectText);
            if (calmText != null)
                UiTweens.Kill(calmText);
            if (chaosText != null)
                UiTweens.Kill(chaosText);
        }
    }
}
