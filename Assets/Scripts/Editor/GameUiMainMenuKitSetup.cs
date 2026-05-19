using MaratGame.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MaratGame.Editor
{
  static class GameUiMainMenuKitSetup
  {
    const string MainMenuPath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("MaratGame/UI/Apply UI Kit to MainMenu")]
    public static void ApplyToMainMenu()
    {
      var scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
      var canvas = Object.FindFirstObjectByType<Canvas>();
      if (canvas != null)
        UiKitFactory.EnsureCanvasSupportsProceduralImage(canvas);

      foreach (var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
      {
        if (button == null)
          continue;

        var graphic = UiKitFactory.AddRoundedButtonGraphic(
          button.gameObject,
          UiStyle.AccentButton,
          UiStyle.ButtonCornerRadius);
        button.targetGraphic = graphic;
      }

      EditorSceneManager.MarkSceneDirty(scene);
      EditorSceneManager.SaveScene(scene);
      Debug.Log("[MaratGame] MainMenu buttons use MPImage (UI Kit).");
    }
  }
}
