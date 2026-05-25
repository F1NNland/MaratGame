using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Гарантирует рабочий UI-клик: EventSystem + Input System UI + резервный <see cref="UiPointerRelay"/>.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class UiInputBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AfterSceneLoad() => EnsureUiInput();

        void Awake() => EnsureUiInput();

        public static void EnsureUiInput()
        {
            EnsureEventSystem();
            EnsureRootCanvasGroups();
            EnsureButtonRaycastTargets();
            DisableBackgroundRaycastBlockers();
            RefreshCanvasScalers();
        }

        static void EnsureEventSystem()
        {
            var systems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            EventSystem primary = null;

            foreach (var system in systems)
            {
                if (system == null)
                    continue;

                if (primary == null)
                {
                    primary = system;
                    continue;
                }

                system.gameObject.SetActive(false);
            }

            if (primary == null)
            {
                var go = new GameObject("EventSystem");
                primary = go.AddComponent<EventSystem>();
                go.AddComponent<InputSystemUIInputModule>();
            }

            primary.enabled = true;
            primary.gameObject.SetActive(true);

            var module = primary.GetComponent<InputSystemUIInputModule>();
            if (module == null)
                module = primary.gameObject.AddComponent<InputSystemUIInputModule>();

            module.enabled = true;

            if (module.actionsAsset == null || module.leftClick == null)
                module.AssignDefaultActions();

            var asset = module.actionsAsset;
            if (asset != null)
            {
                if (!asset.enabled)
                    asset.Enable();

                var uiMap = asset.FindActionMap("UI", false);
                if (uiMap != null && !uiMap.enabled)
                    uiMap.Enable();
            }

            if (primary.GetComponent<UiPointerRelay>() == null)
                primary.gameObject.AddComponent<UiPointerRelay>();

            foreach (var device in InputSystem.devices)
            {
                if (device == null)
                    continue;

                if (device is Mouse or Pen or Touchscreen)
                    InputSystem.EnableDevice(device);
            }
        }

        static void EnsureRootCanvasGroups()
        {
            foreach (var group in Object.FindObjectsByType<CanvasGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (group == null)
                    continue;

                switch (group.gameObject.name)
                {
                    case "Canvas":
                    case "HUD":
                        group.blocksRaycasts = true;
                        group.interactable = true;
                        break;
                    case "DialoguePanel":
                    case "Background":
                        group.blocksRaycasts = false;
                        group.interactable = false;
                        break;
                }
            }
        }

        static void EnsureButtonRaycastTargets()
        {
            foreach (var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (button == null)
                    continue;

                var graphic = button.targetGraphic;
                if (graphic != null)
                    graphic.raycastTarget = true;
            }
        }

        static void DisableBackgroundRaycastBlockers()
        {
            var background = GameObject.Find("BackgroundCanvas/Background");
            if (background == null)
                return;

            var group = background.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.blocksRaycasts = false;
                group.interactable = false;
            }

            foreach (var graphic in background.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
        }

        static void RefreshCanvasScalers()
        {
            foreach (var scaler in Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (scaler == null || !scaler.isActiveAndEnabled)
                    continue;

                var root = scaler.GetComponent<RectTransform>();
                if (root == null || root.localScale.sqrMagnitude >= 0.0001f)
                    continue;

                scaler.enabled = false;
                root.localScale = Vector3.one;
                scaler.enabled = true;
            }
        }
    }
}
