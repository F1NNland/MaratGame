using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Общие UI-твины для VN (docs/DOTWEEN.md, step 04).
    /// </summary>
    public static class UiTweens
    {
        public const float Fast = 0.2f;
        public const float Normal = 0.35f;
        public const float Slow = 0.5f;

        const float ChoiceStagger = 0.06f;
        const float SlideDistance = 40f;

        public static void Kill(Component target)
        {
            if (target != null)
                target.transform.DOKill();
        }

        public static void Kill(Transform target)
        {
            if (target != null)
                target.DOKill();
        }

        public static Tween Fade(CanvasGroup group, float endValue, float duration = Normal, Ease ease = Ease.OutQuad)
        {
            if (group == null)
                return null;

            Kill(group);
            return group.DOFade(endValue, duration).SetEase(ease).SetLink(group.gameObject);
        }

        public static Tween ShowOverlay(CanvasGroup overlay, float duration = Normal)
        {
            if (overlay == null)
                return null;

            overlay.gameObject.SetActive(true);
            overlay.alpha = 0f;
            overlay.interactable = false;
            overlay.blocksRaycasts = true;
            return Fade(overlay, 1f, duration).OnComplete(() =>
            {
                overlay.interactable = true;
            });
        }

        public static Tween HideOverlay(CanvasGroup overlay, float duration = Fast, bool deactivate = true)
        {
            if (overlay == null)
                return null;

            overlay.interactable = false;
            return Fade(overlay, 0f, duration, Ease.InQuad).OnComplete(() =>
            {
                overlay.blocksRaycasts = false;
                if (deactivate)
                    overlay.gameObject.SetActive(false);
            });
        }

        public static Sequence RevealDialogue(RectTransform panel, CanvasGroup group, float duration = Normal)
        {
            if (panel == null)
                return null;

            Kill(panel);

            if (group != null)
            {
                group.alpha = 0f;
                group.gameObject.SetActive(true);
            }

            var anchored = panel.anchoredPosition;
            panel.anchoredPosition = anchored + new Vector2(0f, -SlideDistance);

            var seq = DOTween.Sequence().SetLink(panel.gameObject);
            seq.Join(panel.DOAnchorPos(anchored, duration).SetEase(Ease.OutCubic));
            if (group != null)
                seq.Join(group.DOFade(1f, duration).SetEase(Ease.OutQuad));

            return seq;
        }

        public static void StaggerChoices(RectTransform[] buttons, float duration = Fast, float initialDelay = 0f)
        {
            if (buttons == null)
                return;

            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                if (button == null)
                    continue;

                Kill(button);
                button.localScale = Vector3.one * 0.85f;
                var cg = button.GetComponent<CanvasGroup>();
                if (cg != null)
                    cg.alpha = 0f;

                var delay = initialDelay + i * ChoiceStagger;
                button.DOScale(1f, duration).SetDelay(delay).SetEase(Ease.OutBack).SetLink(button.gameObject);
                if (cg != null)
                    cg.DOFade(1f, duration).SetDelay(delay).SetLink(button.gameObject);
            }
        }

        public static Tween SlideAnchored(RectTransform target, Vector2 endPosition, float duration = Normal)
        {
            if (target == null)
                return null;

            Kill(target);
            return target.DOAnchorPos(endPosition, duration).SetEase(Ease.OutCubic).SetLink(target.gameObject);
        }

        public static Tween PunchScale(Transform target, float strength = 0.15f, float duration = 0.4f)
        {
            if (target == null)
                return null;

            Kill(target);
            return target.DOPunchScale(Vector3.one * strength, duration, 8, 0.5f).SetLink(target.gameObject);
        }

        public static Tween Counter(int from, int to, float duration, Action<int> onValue, Ease ease = Ease.OutQuad)
        {
            if (onValue == null)
                return null;

            var current = (float)from;
            onValue(from);
            return DOTween.To(() => current, x =>
            {
                current = x;
                onValue(Mathf.RoundToInt(current));
            }, to, duration).SetEase(ease);
        }

        /// <summary>
        /// Переход между узлами: затемнить HUD-опционально, сменить контент, показать диалог.
        /// </summary>
        public static Sequence TransitionNode(
            CanvasGroup backgroundGroup,
            CanvasGroup dialogueGroup,
            RectTransform dialoguePanel,
            Action applyContent,
            float duration = Normal)
        {
            var seq = DOTween.Sequence();

            if (backgroundGroup != null)
            {
                seq.Append(Fade(backgroundGroup, 0.65f, duration * 0.4f, Ease.InQuad));
                seq.AppendCallback(() => applyContent?.Invoke());
                seq.Append(Fade(backgroundGroup, 1f, duration * 0.6f, Ease.OutQuad));
            }
            else
            {
                seq.AppendCallback(() => applyContent?.Invoke());
            }

            if (dialoguePanel != null && dialogueGroup != null)
                seq.Append(RevealDialogue(dialoguePanel, dialogueGroup, duration));

            return seq;
        }

        public static Tween CrossFadeImage(Image from, Image to, float duration = Slow)
        {
            if (to == null)
                return null;

            if (from == null)
            {
                to.color = new Color(to.color.r, to.color.g, to.color.b, 1f);
                return null;
            }

            Kill(from);
            Kill(to);

            var toColor = to.color;
            toColor.a = 0f;
            to.color = toColor;
            to.gameObject.SetActive(true);

            var seq = DOTween.Sequence();
            seq.Join(from.DOFade(0f, duration).SetEase(Ease.InQuad));
            seq.Join(to.DOFade(1f, duration).SetEase(Ease.OutQuad));
            seq.OnComplete(() => from.gameObject.SetActive(false));
            return seq;
        }
    }
}
