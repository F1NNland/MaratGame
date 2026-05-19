using System;
using System.Collections.Generic;
using MaratGame.Data;
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
        [SerializeField] RectTransform container;
        [SerializeField] Button choiceButtonPrefab;

        readonly List<Button> _spawned = new();

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
                Destroy(button.gameObject);
            }

            _spawned.Clear();
        }

        public void BuildChoices(StoryChoice[] choices, Action<int> onSelected)
        {
            ClearChoices();

            if (choices == null || choices.Length == 0 || container == null || choiceButtonPrefab == null)
                return;

            var rects = new RectTransform[choices.Length];

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice == null)
                    continue;

                var index = i;
                var button = Instantiate(choiceButtonPrefab, container);
                button.gameObject.SetActive(true);
                _spawned.Add(button);

                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = choice.label ?? "…";

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onSelected?.Invoke(index));

                rects[i] = button.transform as RectTransform;
            }

            UiTweens.StaggerChoices(rects);
        }

        void OnDestroy()
        {
            ClearChoices();
        }
    }
}
