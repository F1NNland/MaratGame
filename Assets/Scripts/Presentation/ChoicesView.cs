using System;
using System.Collections.Generic;
using MaratGame.Narrative;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Контейнер кнопок выбора; пересоздаёт кнопки при смене узла.
    /// </summary>
    public sealed class ChoicesView : MonoBehaviour
    {
        enum ChoiceLayoutMode
        {
            SingleRow,
            TwoRowsCentered,
            Grid
        }

        [SerializeField] RectTransform container;
        [SerializeField] Button choiceButtonPrefab;

        readonly List<Button> _spawned = new();

        void Awake()
        {
            if (container == null)
                container = transform as RectTransform;

            StripLegacyLayoutOnContainer();
            ApplyBottomAnchoredContainer();
        }

        public void RefreshLayoutForViewport() => ApplyBottomAnchoredContainer();

        public void ClearChoices()
        {
            foreach (var button in _spawned)
            {
                if (button == null)
                    continue;

                var cg = button.GetComponent<CanvasGroup>();
                if (cg != null)
                    UiTweens.Kill(cg);
                UiTweens.Kill(button.transform);
            }

            _spawned.Clear();
            ClearChoiceChildren();
            ClearLayoutComponents();
        }

        void ClearChoiceChildren()
        {
            if (container == null)
                return;

            for (var i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

        public void BuildChoices(StoryChoiceAvailability[] choices, Action<int> onSelected)
        {
            ClearChoices();

            if (choices == null || choices.Length == 0 || container == null || choiceButtonPrefab == null)
                return;

            var count = CountValidChoices(choices);
            if (count == 0)
                return;

            var mode = ResolveLayoutMode(count);
            var rowHeight = ResolveRowHeight(mode, count);
            ApplyBottomAnchoredContainer(mode);
            Canvas.ForceUpdateCanvases();
            var cellWidth = ResolveCellWidth(mode, count);
            PrepareContainerLayout(mode, count, rowHeight);
            var buttonSize = ResolveButtonSize(mode, rowHeight);

            var rects = new RectTransform[choices.Length];
            var spawnIndex = 0;

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice.Choice == null || !choice.IsAvailable)
                    continue;

                var index = choice.Index;
                var button = Instantiate(choiceButtonPrefab, container);
                spawnIndex++;
                button.gameObject.SetActive(true);
                var buttonRect = button.transform as RectTransform;
                ResetButtonRectForLayout(buttonRect, mode);
                if (mode == ChoiceLayoutMode.Grid)
                    ApplyGridCellSize(buttonRect, count, rowHeight, cellWidth);
                else if (mode == ChoiceLayoutMode.TwoRowsCentered)
                    ApplyGridCellSize(buttonRect, 3, rowHeight, cellWidth);
                EnsureChoiceLayout(button, buttonSize, mode, count, cellWidth);
                ApplyChoiceButtonChrome(button);
                _spawned.Add(button);

                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = choice.Choice.label ?? "…";
                    if (!choice.IsAvailable && !string.IsNullOrWhiteSpace(choice.Reason))
                        label.text = $"{label.text}\n{choice.Reason}";
                    ConfigureChoiceLabel(label, mode);
                }

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onSelected?.Invoke(index));
                button.interactable = choice.IsAvailable;

                rects[i] = buttonRect;
            }

            if (mode == ChoiceLayoutMode.TwoRowsCentered)
                ApplyTwoRowsCenteredPositions(rowHeight, cellWidth);
            else
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(container);
                if (container.parent is RectTransform parentRect)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }

            foreach (var button in _spawned)
            {
                ApplyChoiceButtonRectAfterLayout(button, mode, count, cellWidth);
                RefreshChoiceLabelAfterLayout(button, mode);
            }

            UiTweens.StaggerChoices(rects);
            EnsureChoiceButtonsInteractable();
        }

        void EnsureChoiceButtonsInteractable()
        {
            foreach (var button in _spawned)
            {
                if (button == null)
                    continue;

                var cg = button.GetComponent<CanvasGroup>();
                if (cg == null)
                    continue;

                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }

        static int CountValidChoices(StoryChoiceAvailability[] choices)
        {
            var count = 0;
            foreach (var choice in choices)
            {
                if (choice.Choice != null && choice.IsAvailable)
                    count++;
            }

            return count;
        }

        static ChoiceLayoutMode ResolveLayoutMode(int count) =>
            count switch
            {
                <= 3 => ChoiceLayoutMode.SingleRow,
                5 => ChoiceLayoutMode.TwoRowsCentered,
                _ => ChoiceLayoutMode.Grid
            };

        static int GetGridColumnCount(int count) =>
            count switch
            {
                4 => 2,
                5 => 3,
                6 => 3,
                7 => 3,
                _ => count >= 8 ? 4 : 3
            };

        static void ConfigureChoiceLabel(TextMeshProUGUI label, ChoiceLayoutMode mode)
        {
            if (label == null)
                return;

            var isGrid = mode is ChoiceLayoutMode.Grid or ChoiceLayoutMode.TwoRowsCentered;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Truncate;
            label.enableAutoSizing = true;
            label.fontSizeMin = isGrid ? UiLayout.FontChoiceGridAutoMin : UiLayout.FontChoiceAutoMin;
            label.fontSizeMax = isGrid ? UiLayout.FontChoiceGridAutoMax : UiLayout.FontChoiceAutoMax;
            label.fontSize = label.fontSizeMax;
            label.lineSpacing = isGrid ? -4f : -2f;
            label.alignment = TextAlignmentOptions.Center;
            label.horizontalAlignment = HorizontalAlignmentOptions.Center;
            label.verticalAlignment = VerticalAlignmentOptions.Middle;

            var rect = label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(UiLayout.ChoiceLabelPadH, UiLayout.ChoiceLabelPadV);
            rect.offsetMax = new Vector2(-UiLayout.ChoiceLabelPadH, -UiLayout.ChoiceLabelPadV);
        }

        static void ApplyChoiceButtonRectAfterLayout(
            Button button,
            ChoiceLayoutMode mode,
            int choiceCount,
            float cellWidth)
        {
            if (button == null || button.transform is not RectTransform rect)
                return;

            var layout = button.GetComponent<LayoutElement>();
            if (layout == null)
                return;

            var height = layout.preferredHeight > 0f
                ? layout.preferredHeight
                : UiLayout.SizeChoiceButton.y;

            if (mode == ChoiceLayoutMode.TwoRowsCentered)
                return;

            if (mode != ChoiceLayoutMode.SingleRow)
            {
                var width = layout.preferredWidth > 0f ? layout.preferredWidth : rect.sizeDelta.x;
                if (width > 0f)
                    rect.sizeDelta = new Vector2(width, height);
                return;
            }

            var buttonWidth = layout.preferredWidth > 0f
                ? layout.preferredWidth
                : cellWidth;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(buttonWidth, height);
        }

        static void RefreshChoiceLabelAfterLayout(Button button, ChoiceLayoutMode mode)
        {
            if (button == null)
                return;

            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label == null)
                return;

            ConfigureChoiceLabel(label, mode);
            label.ForceMeshUpdate(true, true);
        }

        void ApplyBottomAnchoredContainer(ChoiceLayoutMode mode = ChoiceLayoutMode.SingleRow)
        {
            if (container == null)
                return;

            var maxY = mode == ChoiceLayoutMode.SingleRow
                ? UiLayout.ChoicesAreaMaxY
                : UiLayout.ChoicesAreaMaxYInsideChrome;

            container.anchorMin = new Vector2(UiLayout.ContentMinX, UiLayout.ChoicesAreaMinY);
            container.anchorMax = new Vector2(UiLayout.ContentMaxX, maxY);
            container.pivot = new Vector2(0.5f, 0f);
            container.anchoredPosition = Vector2.zero;
            container.sizeDelta = Vector2.zero;
        }

        void PrepareContainerLayout(ChoiceLayoutMode mode, int count, float rowHeight)
        {
            if (container == null)
                return;

            ClearLayoutComponents();

            if (mode == ChoiceLayoutMode.TwoRowsCentered)
                return;

            if (mode == ChoiceLayoutMode.Grid)
            {
                var columns = GetGridColumnCount(count);
                var rows = Mathf.CeilToInt(count / (float)columns);
                var grid = AddContainerLayout<GridLayoutGroup>();
                if (grid == null)
                    return;
                grid.cellSize = new Vector2(ComputeGridCellWidth(columns), rowHeight);
                grid.spacing = new Vector2(UiLayout.ChoiceGridSpacingX, UiLayout.ChoiceGridSpacingY);
                grid.padding = new RectOffset(
                    (int)UiLayout.ChoiceGridPaddingH,
                    (int)UiLayout.ChoiceGridPaddingH,
                    (int)UiLayout.ChoiceGridPaddingV,
                    (int)UiLayout.ChoiceGridPaddingV);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = columns;
                grid.childAlignment = TextAnchor.LowerCenter;
                EnsureChoicesClipMask();
                return;
            }

            var row = AddContainerLayout<HorizontalLayoutGroup>();
            if (row == null)
                return;

            row.spacing = UiLayout.ChoiceGridSpacingX;
            row.padding = new RectOffset(
                (int)UiLayout.ChoiceGridPaddingH,
                (int)UiLayout.ChoiceGridPaddingH,
                (int)UiLayout.ChoiceGridPaddingV,
                (int)UiLayout.ChoiceGridPaddingV);
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = false;
            row.childControlHeight = false;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
        }

        float MeasureContainerWidth()
        {
            if (container == null)
                return UiLayout.ChoicesAreaWidth;

            return Mathf.Max(100f, container.rect.width);
        }

        float ComputeGridCellWidth(int columns)
        {
            var cols = Mathf.Max(1, columns);
            var area = MeasureContainerWidth();
            var totalSpacing = UiLayout.ChoiceGridSpacingX * (cols - 1);
            var cell = (area - UiLayout.ChoiceGridPaddingH * 2f - totalSpacing) / cols;
            return Mathf.Clamp(cell, 180f, UiLayout.SizeChoiceButton.x);
        }

        float ResolveCellWidth(ChoiceLayoutMode mode, int choiceCount) =>
            mode switch
            {
                ChoiceLayoutMode.TwoRowsCentered => ComputeGridCellWidth(3),
                ChoiceLayoutMode.Grid => ComputeGridCellWidth(GetGridColumnCount(choiceCount)),
                _ => choiceCount switch
                {
                    >= 3 => ComputeGridCellWidth(3),
                    2 => ComputeGridCellWidth(2),
                    _ => UiLayout.SizeChoiceButton.x
                }
            };

        void ApplyTwoRowsCenteredPositions(float rowHeight, float cellWidth)
        {
            if (_spawned.Count < 5 || container == null)
                return;

            var gapX = UiLayout.ChoiceGridSpacingX;
            var gapY = UiLayout.ChoiceGridSpacingY;
            var padV = UiLayout.ChoiceGridPaddingV;
            var bottomY = padV;
            var topY = padV + rowHeight + gapY;

            PlaceChoiceRow(0, 3, cellWidth, rowHeight, gapX, topY);
            PlaceChoiceRow(3, 2, cellWidth, rowHeight, gapX, bottomY);
        }

        void PlaceChoiceRow(int startIndex, int count, float cellWidth, float rowHeight, float gapX, float y)
        {
            var rowWidth = cellWidth * count + gapX * (count - 1);
            var x = -rowWidth * 0.5f + cellWidth * 0.5f;

            for (var i = 0; i < count; i++)
            {
                var index = startIndex + i;
                if (index >= _spawned.Count)
                    return;

                var button = _spawned[index];
                if (button == null || button.transform is not RectTransform rect)
                    continue;

                rect.SetParent(container, false);
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.sizeDelta = new Vector2(cellWidth, rowHeight);
                rect.anchoredPosition = new Vector2(x + i * (cellWidth + gapX), y);
            }
        }

        void ClearLayoutComponents()
        {
            foreach (var grid in container.GetComponents<GridLayoutGroup>())
                DestroyLayoutComponent(grid);

            foreach (var horizontal in container.GetComponents<HorizontalLayoutGroup>())
                DestroyLayoutComponent(horizontal);

            foreach (var vertical in container.GetComponents<VerticalLayoutGroup>())
                DestroyLayoutComponent(vertical);

            foreach (var fitter in container.GetComponents<ContentSizeFitter>())
                DestroyLayoutComponent(fitter);

            foreach (var mask in container.GetComponents<RectMask2D>())
                DestroyLayoutComponent(mask);
        }

        void StripLegacyLayoutOnContainer()
        {
            if (container == null)
                return;

            foreach (var layout in container.GetComponents<LayoutGroup>())
                DestroyLayoutComponent(layout);
        }

        T AddContainerLayout<T>() where T : Component
        {
            if (container == null)
                return null;

            return container.gameObject.AddComponent<T>();
        }

        /// <summary>Снять layout с контейнера сразу — иначе AddComponent в том же кадре даёт null.</summary>
        static void DestroyLayoutComponent(Component component)
        {
            if (component == null)
                return;

            DestroyImmediate(component);
        }

        static float ResolveRowHeight(ChoiceLayoutMode mode, int count)
        {
            if (mode == ChoiceLayoutMode.TwoRowsCentered)
                return UiLayout.ComputeChoiceRowHeight(2);

            if (mode == ChoiceLayoutMode.Grid)
            {
                var columns = GetGridColumnCount(count);
                var rows = Mathf.CeilToInt(count / (float)columns);
                return UiLayout.ComputeChoiceRowHeight(rows);
            }

            return UiLayout.SizeChoiceButtonRow.y;
        }

        static Vector2 ResolveButtonSize(ChoiceLayoutMode mode, float rowHeight) =>
            mode is ChoiceLayoutMode.Grid or ChoiceLayoutMode.TwoRowsCentered
                ? new Vector2(-1f, rowHeight)
                : UiLayout.SizeChoiceButtonRow;

        void EnsureChoicesClipMask()
        {
            if (container == null || container.GetComponent<RectMask2D>() != null)
                return;

            container.gameObject.AddComponent<RectMask2D>();
        }

        static void ResetButtonRectForLayout(RectTransform rect, ChoiceLayoutMode mode)
        {
            if (rect == null)
                return;

            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            if (mode == ChoiceLayoutMode.SingleRow)
                rect.sizeDelta = new Vector2(UiLayout.SizeChoiceButton.x, UiLayout.SizeChoiceButton.y);
        }

        void ApplyGridCellSize(RectTransform rect, int columns, float rowHeight, float cellWidth)
        {
            if (rect == null)
                return;

            rect.sizeDelta = new Vector2(cellWidth, rowHeight);
        }

        void EnsureChoiceLayout(Button button, Vector2 size, ChoiceLayoutMode mode, int choiceCount, float cellWidth)
        {
            if (button == null)
                return;

            StripButtonAutoSizeFitter(button);

            var layout = button.GetComponent<LayoutElement>();
            if (layout == null)
                layout = button.gameObject.AddComponent<LayoutElement>();

            var height = size.y > 0f ? size.y : UiLayout.SizeChoiceButton.y;
            layout.minHeight = height;
            layout.preferredHeight = height;
            layout.flexibleHeight = 0f;

            if (mode == ChoiceLayoutMode.SingleRow)
            {
                var width = cellWidth;
                layout.flexibleWidth = 0f;
                layout.preferredWidth = width;
                layout.minWidth = width;
            }
            else
            {
                var width = mode == ChoiceLayoutMode.TwoRowsCentered
                    ? cellWidth
                    : ComputeGridCellWidth(GetGridColumnCount(choiceCount));
                layout.flexibleWidth = 0f;
                layout.flexibleHeight = 0f;
                layout.minWidth = width;
                layout.preferredWidth = width;
            }

            if (button.transform is RectTransform rect && mode != ChoiceLayoutMode.SingleRow)
            {
                var width = layout.preferredWidth > 0f ? layout.preferredWidth : 0f;
                rect.sizeDelta = new Vector2(width, height);
            }
        }

        static void ApplyChoiceButtonChrome(Button button)
        {
            if (button == null)
                return;

            var image = button.GetComponent<Image>();
            if (image != null)
                image.preserveAspect = false;

            EnsureChoiceTextMask(button);
        }

        static void StripButtonAutoSizeFitter(Button button)
        {
            if (button == null)
                return;

            var fitter = button.GetComponent<ContentSizeFitter>();
            if (fitter == null)
                return;

            if (Application.isPlaying)
                Destroy(fitter);
            else
                DestroyImmediate(fitter);
        }

        static void EnsureChoiceTextMask(Button button)
        {
            if (button == null)
                return;

            if (button.GetComponent<RectMask2D>() == null)
                button.gameObject.AddComponent<RectMask2D>();
        }

        void OnDestroy() => ClearChoices();
    }
}
