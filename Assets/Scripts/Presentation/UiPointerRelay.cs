using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Резервный путь клика: RaycastAll + ExecuteEvents, если InputSystemUIInputModule не шлёт pointer (часто WebGL + touch).
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
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return;

            if (TryRelayTouch(eventSystem))
                return;

            TryRelayMouse(eventSystem);
        }

        static bool TryRelayTouch(EventSystem eventSystem)
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null || !touchscreen.primaryTouch.press.wasPressedThisFrame)
                return false;

            return RelayClick(eventSystem, touchscreen.primaryTouch.position.ReadValue());
        }

        static bool TryRelayMouse(EventSystem eventSystem)
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
                return false;

            return RelayClick(eventSystem, mouse.position.ReadValue());
        }

        static bool RelayClick(EventSystem eventSystem, Vector2 screenPosition)
        {
            var pointerData = new PointerEventData(eventSystem)
            {
                position = screenPosition,
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
                return true;
            }

            return false;
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

                if (!group.interactable || group.alpha < 0.01f || !group.blocksRaycasts)
                    return false;
            }

            return true;
        }
    }
}
