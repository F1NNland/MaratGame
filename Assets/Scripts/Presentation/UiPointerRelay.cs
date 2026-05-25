using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Резервный путь клика: RaycastAll + ExecuteEvents, если InputSystemUIInputModule не шлёт pointer.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public sealed class UiPointerRelay : MonoBehaviour
    {
        static UiPointerRelay _instance;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null)
                return;

            if (!mouse.leftButton.wasPressedThisFrame)
                return;

            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return;

            var pointerData = new PointerEventData(eventSystem)
            {
                position = mouse.position.ReadValue(),
                button = PointerEventData.InputButton.Left
            };

            var results = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, results);

            for (var i = 0; i < results.Count; i++)
            {
                var target = results[i].gameObject;
                if (target == null)
                    continue;

                var button = target.GetComponent<Button>() ?? target.GetComponentInParent<Button>();
                if (button == null || !IsButtonReachable(button))
                    continue;

                ExecuteEvents.Execute(button.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
                return;
            }
        }

        static bool IsButtonReachable(Button button)
        {
            if (!button.isActiveAndEnabled || !button.interactable)
                return false;

            if (!button.gameObject.activeInHierarchy)
                return false;

            foreach (var group in button.GetComponentsInParent<CanvasGroup>(true))
            {
                if (group == null)
                    continue;

                if (!group.interactable || group.alpha < 0.01f)
                    return false;
            }

            return true;
        }
    }
}
