using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MaratGame.Presentation
{
    /// <summary>
    /// Логирует клики мыши: попадает ли в EventSystem, что в RaycastAll, какие CanvasGroup мешают.
    /// В Play Mode кликайте по UI и смотрите Console (фильтр «UiClick»).
    /// </summary>
    public sealed class UiClickDebug : MonoBehaviour
    {
        [SerializeField] bool logEveryFrameWithButtonHeld;
        [SerializeField] int maxHitsLogged = 12;

        void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null)
                return;

            var pressed = mouse.leftButton.wasPressedThisFrame;
            var held = mouse.leftButton.isPressed;
            if (!pressed && !(logEveryFrameWithButtonHeld && held))
                return;

            LogClick(pressed ? "DOWN" : "HELD", mouse.position.ReadValue(), maxHitsLogged);
        }

        static void LogClick(string phase, Vector2 screenPos, int maxHits)
        {
            var sb = new StringBuilder(512);
            sb.AppendLine($"[UiClick] === {phase} screen={screenPos} ===");

            var es = EventSystem.current;
            if (es == null)
            {
                Debug.LogWarning(sb + "[UiClick] EventSystem.current == null");
                return;
            }

            sb.AppendLine($"EventSystem: {es.name}, enabled={es.enabled}, module={DescribeModule(es.currentInputModule)}");

            var pointerData = new PointerEventData(es) { position = screenPos };
            var hits = new List<RaycastResult>();
            es.RaycastAll(pointerData, hits);

            sb.AppendLine($"RaycastAll hits: {hits.Count}");
            if (hits.Count == 0)
            {
                sb.AppendLine("  (пусто — частые причины: CanvasGroup.blocksRaycasts=false на корневом Canvas, raycastTarget=false на кнопках)");
                LogCanvasRoots(sb);
                LogRaycastableGraphicCount(sb);
                Debug.Log(sb.ToString());
                return;
            }

            var limit = Mathf.Min(hits.Count, Mathf.Max(1, maxHits));
            for (var i = 0; i < limit; i++)
                AppendHit(sb, i, hits[i]);

            if (hits.Count > limit)
                sb.AppendLine($"  … ещё {hits.Count - limit} объектов");

            AppendTopButton(sb, hits);
            Debug.Log(sb.ToString());
        }

        static void AppendHit(StringBuilder sb, int index, RaycastResult hit)
        {
            var go = hit.gameObject;
            var path = BuildPath(go.transform);
            sb.Append($"  [{index}] depth={hit.depth} dist={hit.distance:F1} «{go.name}» path={path}");

            var graphic = go.GetComponent<Graphic>();
            if (graphic != null)
                sb.Append($" graphic={graphic.GetType().Name} raycast={graphic.raycastTarget}");

            var button = go.GetComponent<Button>();
            if (button != null)
                sb.Append($" Button interactable={button.interactable} enabled={button.isActiveAndEnabled}");

            sb.AppendLine();
            AppendParentGroups(sb, go.transform, "    ");
        }

        static void AppendTopButton(StringBuilder sb, List<RaycastResult> hits)
        {
            for (var i = 0; i < hits.Count; i++)
            {
                var button = hits[i].gameObject.GetComponent<Button>();
                if (button == null)
                    button = hits[i].gameObject.GetComponentInParent<Button>();
                if (button == null)
                    continue;

                sb.AppendLine($"Top Button candidate: «{button.name}» path={BuildPath(button.transform)} " +
                               $"active={button.gameObject.activeInHierarchy} interactable={button.interactable}");
                sb.AppendLine($"  IsClickableHeuristic: {IsClickableHeuristic(button)}");
                return;
            }

            sb.AppendLine("Top Button candidate: (нет Button на луче)");
        }

        static bool IsClickableHeuristic(Button button)
        {
            if (button == null || !button.isActiveAndEnabled || !button.interactable)
                return false;
            if (!button.gameObject.activeInHierarchy)
                return false;

            var groups = button.GetComponentsInParent<CanvasGroup>(true);
            foreach (var group in groups)
            {
                if (group == null)
                    continue;
                if (!group.interactable)
                    return false;
                if (group.alpha < 0.01f)
                    return false;
            }

            return true;
        }

        static void AppendParentGroups(StringBuilder sb, Transform t, string indent)
        {
            var groups = t.GetComponentsInParent<CanvasGroup>(true);
            for (var i = groups.Length - 1; i >= 0; i--)
            {
                var g = groups[i];
                if (g == null)
                    continue;
                sb.AppendLine($"{indent}CanvasGroup «{g.gameObject.name}»: alpha={g.alpha:F2} interactable={g.interactable} blocksRaycasts={g.blocksRaycasts}");
            }
        }

        static void LogRaycastableGraphicCount(StringBuilder sb)
        {
            var total = 0;
            var active = 0;
            foreach (var graphic in Object.FindObjectsByType<Graphic>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (graphic == null || graphic.canvas == null)
                    continue;

                if (graphic.canvas.name != "Canvas")
                    continue;

                total++;
                if (graphic.raycastTarget && graphic.gameObject.activeInHierarchy)
                    active++;
            }

            sb.AppendLine($"  Canvas raycastTarget=true (active): {active} / {total} graphics");
        }

        static void LogCanvasRoots(StringBuilder sb)
        {
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (canvas == null)
                    continue;

                var rt = canvas.GetComponent<RectTransform>();
                var scale = rt != null ? rt.localScale : Vector3.zero;
                var raycaster = canvas.GetComponent<GraphicRaycaster>();
                sb.AppendLine($"  Canvas «{canvas.name}» mode={canvas.renderMode} order={canvas.sortingOrder} " +
                              $"scale={scale} raycaster={(raycaster != null && raycaster.enabled ? "on" : "off")} " +
                              $"receivesEvents={canvas.enabled}");
            }
        }

        static string DescribeModule(BaseInputModule module)
        {
            if (module == null)
                return "null (клики не обрабатываются!)";
            return $"{module.GetType().Name} enabled={module.enabled}";
        }

        static string BuildPath(Transform t)
        {
            if (t == null)
                return "";

            var parts = new List<string>(8);
            while (t != null)
            {
                parts.Add(t.name);
                t = t.parent;
            }

            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
