using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Editor
{
    /// <summary>
    /// Шаг 05 MVP: миграция MainMenu на TMP; Game — через пересборку Step 04.
    /// </summary>
    static class GameUiTmpSetup
    {
        const string MainMenuPath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("MaratGame/UI/Setup Step 05 (TextMeshPro)")]
        public static void SetupStep05()
        {
            GameUiShellSetup.SetupStep04();
            MigrateMainMenuToTmp();
            GameUiMainMenuKitSetup.ApplyToMainMenu();
            AssetDatabase.SaveAssets();
            Debug.Log("[MaratGame] Step 05: Game UI (TMP + UI Kit), MainMenu TMP + MPImage buttons.");
        }

        static void MigrateMainMenuToTmp()
        {
            var scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
            var legacyTexts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var legacy in legacyTexts)
            {
                if (legacy == null)
                    continue;

                var go = legacy.gameObject;
                var content = legacy.text;
                var fontSize = legacy.fontSize;
                var color = legacy.color;
                var alignment = TmpUiFactory.FromLegacyAnchor(legacy.alignment);
                var rect = go.GetComponent<RectTransform>();

                Object.DestroyImmediate(legacy);

                var tmp = go.GetComponent<TextMeshProUGUI>();
                if (tmp == null)
                    tmp = go.AddComponent<TextMeshProUGUI>();

                tmp.font = TmpUiFactory.DefaultFont;
                tmp.text = content;
                tmp.fontSize = fontSize;
                tmp.color = color;
                tmp.alignment = alignment;
                tmp.textWrappingMode = TextWrappingModes.Normal;

                if (rect != null)
                    LayoutRebuilder.MarkLayoutForRebuild(rect);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
