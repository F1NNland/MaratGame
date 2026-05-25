using MaratGame.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MaratGame.Editor
{
    static class UiClickDebugSetup
    {
        const string GameScenePath = "Assets/Scenes/Game.unity";

        [MenuItem("MaratGame/Debug/Enable UI Click Logging (Game)")]
        public static void EnableOnGameScene()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var systems = GameObject.Find("GameSystems");
            if (systems == null)
            {
                Debug.LogError("[UiClick] GameSystems not found on Game scene.");
                return;
            }

            if (systems.GetComponent<UiClickDebug>() == null)
                systems.AddComponent<UiClickDebug>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[UiClick] UiClickDebug on GameSystems. Play → кликайте UI → Console, фильтр «UiClick».");
        }

        [MenuItem("MaratGame/Debug/Disable UI Click Logging (Game)")]
        public static void DisableOnGameScene()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var systems = GameObject.Find("GameSystems");
            if (systems == null)
                return;

            var debug = systems.GetComponent<UiClickDebug>();
            if (debug != null)
                Object.DestroyImmediate(debug);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[UiClick] UiClickDebug removed.");
        }
    }
}
