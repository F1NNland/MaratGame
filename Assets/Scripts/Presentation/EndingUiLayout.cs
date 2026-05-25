using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>Раскладка финального экрана: зоны с подложками и без наложений.</summary>
    public static class EndingUiLayout
    {
        public const float PanelWidth = 640f;
        public const float PanelHeight = 780f;

        const float ZoneOuterPad = 28f;
        const float ZoneContentPad = 26f;

        static readonly Color ZoneStats = new(0.1f, 0.13f, 0.18f, 0.92f);
        static readonly Color ZoneAchievements = new(0.08f, 0.11f, 0.15f, 0.88f);
        static readonly Color ZoneActions = new(0.07f, 0.09f, 0.13f, 0.75f);

        public static void Apply(RectTransform panel, EndingUiRefs refs)
        {
            if (panel == null)
                return;

            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = new Vector2(PanelWidth, PanelHeight);

            var title = EnsureTitle(panel, refs?.Title);
            var statsZone = EnsureZone(panel, "StatsZone", ZoneStats);
            var achievementsZone = EnsureZone(panel, "AchievementsZone", ZoneAchievements);
            var actionsZone = EnsureZone(panel, "ActionsZone", ZoneActions);

            Reparent(title, panel, asFirst: true);
            Reparent(statsZone, panel);
            Reparent(achievementsZone, panel);
            Reparent(actionsZone, panel);

            LayoutTitle(title);
            LayoutStatsZone(statsZone, refs);
            LayoutAchievementsZone(achievementsZone, refs);
            LayoutActionsZone(actionsZone, refs);
            CleanupDuplicates(panel, refs);
        }

        static RectTransform EnsureTitle(RectTransform panel, TextMeshProUGUI titleText)
        {
            if (titleText != null)
                return titleText.rectTransform;

            var existing = panel.Find("Title") as RectTransform;
            return existing;
        }

        static RectTransform EnsureZone(RectTransform panel, string zoneName, Color color)
        {
            var zone = panel.Find(zoneName) as RectTransform;
            if (zone == null)
            {
                var go = new GameObject(zoneName, typeof(RectTransform));
                zone = go.GetComponent<RectTransform>();
                zone.SetParent(panel, false);
            }

            var image = zone.GetComponent<Image>();
            if (image == null)
                image = zone.gameObject.AddComponent<Image>();

            image.color = color;
            image.raycastTarget = false;
            image.maskable = true;
            return zone;
        }

        static void Reparent(RectTransform child, RectTransform parent, bool asFirst = false)
        {
            if (child == null || parent == null)
                return;

            child.SetParent(parent, false);
            if (asFirst)
                child.SetAsFirstSibling();
        }

        static void LayoutTitle(RectTransform title)
        {
            if (title == null)
                return;

            StretchTop(title, top: 20f, height: 52f);
            ApplyHorizontalInset(title, ZoneContentPad);
            var tmp = title.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontStyle = FontStyles.Bold;
                tmp.fontSize = 30f;
                tmp.color = UiStyle.TextSpeaker;
            }
        }

        static void LayoutStatsZone(RectTransform zone, EndingUiRefs refs)
        {
            var panel = zone.parent as RectTransform;
            StretchBand(zone, top: 84f, bottom: 430f, horizontalPad: ZoneOuterPad);

            MoveRow(panel, zone, refs?.RespectValue, "RespectRow");
            MoveRow(panel, zone, refs?.CalmValue, "CalmRow");
            MoveRow(panel, zone, refs?.ChaosValue, "ChaosRow");
            MoveRow(panel, zone, refs?.DecisionsValue, "DecisionsRow");

            var rows = new[]
            {
                FindRow(panel, zone, refs?.RespectValue, "RespectRow"),
                FindRow(panel, zone, refs?.CalmValue, "CalmRow"),
                FindRow(panel, zone, refs?.ChaosValue, "ChaosRow"),
                FindRow(panel, zone, refs?.DecisionsValue, "DecisionsRow")
            };

            var top = 14f;
            const float rowHeight = 44f;
            const float gap = 6f;
            for (var i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                if (row == null)
                    continue;

                if (row.name == "ChaosRow" && !row.gameObject.activeInHierarchy)
                    continue;

                LayoutStatRow(row, top, rowHeight);
                top += rowHeight + gap;
            }
        }

        static void LayoutAchievementsZone(RectTransform zone, EndingUiRefs refs)
        {
            var panel = zone.parent as RectTransform;
            StretchBand(zone, top: 360f, bottom: 200f, horizontalPad: ZoneOuterPad);

            var header = zone.Find("AchievementsHeader") as RectTransform;
            if (header == null)
            {
                var headerGo = new GameObject("AchievementsHeader", typeof(RectTransform));
                header = headerGo.GetComponent<RectTransform>();
                header.SetParent(zone, false);
                var label = headerGo.AddComponent<TextMeshProUGUI>();
                label.text = "Достижения";
                label.fontSize = 20f;
                label.fontStyle = FontStyles.Bold;
                label.color = UiStyle.TextMuted;
                label.alignment = TextAlignmentOptions.MidlineLeft;
            }

            StretchTop(header, top: 14f, height: 28f);
            ApplyHorizontalInset(header, ZoneContentPad);

            var body = refs?.AchievementsValue != null
                ? refs.AchievementsValue.rectTransform
                : panel?.Find("AchievementsRow/Value") as RectTransform;

            if (body != null)
            {
                body.SetParent(zone, false);
                StretchTop(body, top: 46f, height: 108f);
                ApplyHorizontalInset(body, ZoneContentPad);

                var tmp = body.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.alignment = TextAlignmentOptions.TopLeft;
                    tmp.textWrappingMode = TextWrappingModes.Normal;
                    tmp.overflowMode = TextOverflowModes.Masking;
                    tmp.fontSize = 17f;
                    tmp.lineSpacing = 2f;
                    tmp.color = UiStyle.TextLight;
                }
            }

            DeactivateLegacyAchievementRows(panel, zone);
            CleanupAchievementTextDuplicates(zone, refs?.AchievementsValue);
        }

        static void LayoutActionsZone(RectTransform zone, EndingUiRefs refs)
        {
            StretchBand(zone, top: 588f, bottom: 24f, horizontalPad: ZoneOuterPad);

            var replay = refs?.ReplayButton != null ? refs.ReplayButton.transform as RectTransform : null;
            replay ??= zone.parent?.Find("ReplayButton") as RectTransform;
            var menu = refs?.MainMenuButton != null ? refs.MainMenuButton.transform as RectTransform : null;
            menu ??= zone.parent?.Find("MainMenuButton") as RectTransform;

            if (replay != null)
            {
                replay.SetParent(zone, false);
                StretchTop(replay, top: 12f, height: 56f);
                replay.offsetMin = new Vector2(0f, replay.offsetMin.y);
                replay.offsetMax = new Vector2(0f, replay.offsetMax.y);
            }

            if (menu != null)
            {
                menu.SetParent(zone, false);
                StretchTop(menu, top: 80f, height: 56f);
                menu.offsetMin = new Vector2(0f, menu.offsetMin.y);
                menu.offsetMax = new Vector2(0f, menu.offsetMax.y);
            }
        }

        static void MoveRow(RectTransform panel, RectTransform zone, TextMeshProUGUI value, string fallbackName)
        {
            var row = FindRow(panel, zone, value, fallbackName);
            if (row != null)
                row.SetParent(zone, false);
        }

        static RectTransform FindRow(RectTransform panel, RectTransform zone, TextMeshProUGUI value, string fallbackName)
        {
            if (value != null)
                return value.transform.parent as RectTransform;

            return zone.Find(fallbackName) as RectTransform
                   ?? panel?.Find(fallbackName) as RectTransform;
        }

        static void LayoutStatRow(RectTransform row, float top, float height)
        {
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(1f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.anchoredPosition = new Vector2(0f, -top);
            row.sizeDelta = new Vector2(0f, height);
            ApplyHorizontalInset(row, ZoneContentPad);

            var label = row.Find("Label")?.GetComponent<TextMeshProUGUI>();
            var val = row.Find("Value")?.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                label.color = UiStyle.TextMuted;
                label.fontSize = 19f;
                label.alignment = TextAlignmentOptions.MidlineLeft;
            }

            if (val != null)
            {
                val.color = UiStyle.TextLight;
                val.fontSize = 24f;
                val.fontStyle = FontStyles.Bold;
                val.alignment = TextAlignmentOptions.MidlineRight;
            }
        }

        static void ApplyHorizontalInset(RectTransform rect, float pad)
        {
            if (rect == null)
                return;

            rect.offsetMin = new Vector2(pad, rect.offsetMin.y);
            rect.offsetMax = new Vector2(-pad, rect.offsetMax.y);
        }

        static void StretchTop(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(0f, height);
        }

        static void StretchBand(RectTransform rect, float top, float bottom, float horizontalPad)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(horizontalPad, bottom);
            rect.offsetMax = new Vector2(-horizontalPad, -top);
        }

        static void CleanupDuplicates(RectTransform panel, EndingUiRefs refs)
        {
            if (panel == null)
                return;

            DeactivateExtraNamedRows(panel, "RespectRow", refs?.RespectValue);
            DeactivateExtraNamedRows(panel, "CalmRow", refs?.CalmValue);
            DeactivateExtraNamedRows(panel, "ChaosRow", refs?.ChaosValue);
            DeactivateExtraNamedRows(panel, "DecisionsRow", refs?.DecisionsValue);

            var achievementsZone = panel.Find("AchievementsZone") as RectTransform;
            DeactivateLegacyAchievementRows(panel, achievementsZone);
            CleanupAchievementTextDuplicates(achievementsZone, refs?.AchievementsValue);

            DeactivateExtraHeaders(panel, achievementsZone, "AchievementsHeader");
        }

        static void DeactivateExtraNamedRows(Transform root, string rowName, TextMeshProUGUI canonicalValue)
        {
            var keep = canonicalValue != null ? canonicalValue.transform.parent : null;
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name != rowName)
                    continue;

                if (keep != null && child == keep)
                    continue;

                child.gameObject.SetActive(false);
            }
        }

        static void DeactivateLegacyAchievementRows(Transform panel, RectTransform achievementsZone)
        {
            if (panel == null)
                return;

            foreach (var child in panel.GetComponentsInChildren<Transform>(true))
            {
                if (child.name != "AchievementsRow")
                    continue;

                if (achievementsZone != null && child.IsChildOf(achievementsZone))
                    continue;

                child.gameObject.SetActive(false);
            }
        }

        static void DeactivateExtraHeaders(Transform panel, RectTransform achievementsZone, string headerName)
        {
            var kept = achievementsZone != null ? achievementsZone.Find(headerName) : null;
            foreach (var child in panel.GetComponentsInChildren<Transform>(true))
            {
                if (child.name != headerName)
                    continue;

                if (kept != null && child == kept)
                    continue;

                child.gameObject.SetActive(false);
            }
        }

        /// <summary>Удаляет дубликаты строк/легаси-узлов в редакторе (сцена Game).</summary>
        public static void DestroyDuplicateEndingObjects(RectTransform panel, EndingUiRefs refs)
        {
            if (panel == null)
                return;

            DestroyExtraNamedRows(panel, "RespectRow", refs?.RespectValue);
            DestroyExtraNamedRows(panel, "CalmRow", refs?.CalmValue);
            DestroyExtraNamedRows(panel, "ChaosRow", refs?.ChaosValue);
            DestroyExtraNamedRows(panel, "DecisionsRow", refs?.DecisionsValue);

            var achievementsZone = panel.Find("AchievementsZone") as RectTransform;
            foreach (var child in panel.GetComponentsInChildren<Transform>(true))
            {
                if (child.name != "AchievementsRow")
                    continue;

                if (achievementsZone != null && child.IsChildOf(achievementsZone))
                    continue;

                Object.DestroyImmediate(child.gameObject);
            }

            DeactivateExtraHeaders(panel, achievementsZone, "AchievementsHeader");
            DestroyAchievementTextDuplicates(achievementsZone, refs?.AchievementsValue);
        }

        public static void CleanupAchievementTextDuplicates(RectTransform zone, TextMeshProUGUI canonical)
        {
            if (zone == null)
                return;

            foreach (var tmp in zone.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (canonical != null && tmp == canonical)
                    continue;

                if (tmp.gameObject.name == "AchievementsHeader")
                    continue;

                tmp.gameObject.SetActive(false);
            }
        }

        static void DestroyAchievementTextDuplicates(RectTransform zone, TextMeshProUGUI canonical)
        {
            if (zone == null)
                return;

            foreach (var tmp in zone.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (canonical != null && tmp == canonical)
                    continue;

                if (tmp.gameObject.name == "AchievementsHeader")
                    continue;

                Object.DestroyImmediate(tmp.gameObject);
            }
        }

        static void DestroyExtraNamedRows(Transform root, string rowName, TextMeshProUGUI canonicalValue)
        {
            var keep = canonicalValue != null ? canonicalValue.transform.parent : null;
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name != rowName)
                    continue;

                if (keep != null && child == keep)
                    continue;

                Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    public sealed class EndingUiRefs
    {
        public TextMeshProUGUI Title;
        public TextMeshProUGUI RespectValue;
        public TextMeshProUGUI CalmValue;
        public TextMeshProUGUI ChaosValue;
        public TextMeshProUGUI DecisionsValue;
        public TextMeshProUGUI AchievementsValue;
        public Button ReplayButton;
        public Button MainMenuButton;
    }
}
