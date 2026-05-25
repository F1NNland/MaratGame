using System.Text;
using MaratGame.Data;
using MaratGame.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MaratGame.Editor
{
    /// <summary>Привязка фонов и портретов из Assets/Art/Generated/ на сцену Game.</summary>
    static class GameUiPhotoBindingsSetup
    {
        const string GameScenePath = "Assets/Scenes/Game.unity";

        [MenuItem("MaratGame/UI/Refresh All Photo Bindings (Game scene)")]
        public static void RefreshAllPhotoBindings()
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var controller = Object.FindFirstObjectByType<GameUIController>();
            if (controller == null)
            {
                Debug.LogError("[MaratGame] Game scene has no GameUIController.");
                return;
            }

            var report = new StringBuilder();
            report.AppendLine("[MaratGame] Photo bindings audit:");

            RefreshLocationBackgrounds(controller, report);
            RefreshCharacterPortraits(controller, report);

            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log(report.ToString());
        }

        static void RefreshLocationBackgrounds(GameUIController controller, StringBuilder report)
        {
            var hall = LocationPhotoSprites.Load(LocationPhotoPaths.Hall);
            var canteen = LocationPhotoSprites.Load(LocationPhotoPaths.Canteen) ?? hall;
            var toilet = LocationPhotoSprites.Load(LocationPhotoPaths.Toilet) ?? hall;
            var planerka = LocationPhotoSprites.Load(LocationPhotoPaths.Planerka) ?? hall;
            var krrb = LocationPhotoSprites.Load(LocationPhotoPaths.Krrb) ?? hall;
            var cabinet = LocationPhotoSprites.Load(LocationPhotoPaths.Cabinet) ?? hall;
            var elevator = LocationPhotoSprites.Load(LocationPhotoPaths.Elevator) ?? hall;
            var evening = LocationPhotoSprites.Load(LocationPhotoPaths.Evening) ?? hall;

            BindLocation(controller, report, "hall", hall, LocationPhotoPaths.Hall);
            BindLocation(controller, report, "canteen", canteen, LocationPhotoPaths.Canteen);
            BindLocation(controller, report, "toilet", toilet, LocationPhotoPaths.Toilet);
            BindLocation(controller, report, "planerka", planerka, LocationPhotoPaths.Planerka);
            BindLocation(controller, report, "meeting_room", planerka, LocationPhotoPaths.Planerka);
            BindLocation(controller, report, "krrb", krrb, LocationPhotoPaths.Krrb);
            BindLocation(controller, report, "cabinet", cabinet, LocationPhotoPaths.Cabinet);
            BindLocation(controller, report, "elevator", elevator, LocationPhotoPaths.Elevator);
            BindLocation(controller, report, "evening", evening, LocationPhotoPaths.Evening);
        }

        static void RefreshCharacterPortraits(GameUIController controller, StringBuilder report)
        {
            BindPortrait(controller, report, CharacterIds.Marat, CharacterPhotoPaths.Marat);
            BindPortrait(controller, report, CharacterIds.Alevtina, CharacterPhotoPaths.Alevtina);
            BindPortrait(controller, report, CharacterIds.Kozlikhin, CharacterPhotoPaths.Kozlikhin);
            BindPortrait(controller, report, CharacterIds.Nozdrikov, CharacterPhotoPaths.Nozdrikov);
        }

        static void BindLocation(
            GameUIController controller,
            StringBuilder report,
            string locationId,
            Sprite sprite,
            string sourcePath,
            bool stub = false)
        {
            if (sprite == null)
            {
                report.AppendLine($"  [FAIL] {locationId}: no sprite ({sourcePath})");
                return;
            }

            SetLocationBackground(controller, locationId, sprite);
            var tag = stub ? "stub→hall" : "ok";
            report.AppendLine($"  [{tag}] {locationId}: {sprite.name} ({sourcePath})");
        }

        static void BindPortrait(
            GameUIController controller,
            StringBuilder report,
            string characterId,
            string sourcePath,
            bool optional = false)
        {
            var sprite = LocationPhotoSprites.Load(sourcePath);
            if (sprite == null)
            {
                if (!optional)
                    report.AppendLine($"  [FAIL] {characterId}: no sprite ({sourcePath})");
                return;
            }

            SetCharacterPortrait(controller, characterId, sprite);
            report.AppendLine($"  [ok] {characterId}: {sprite.name} ({sourcePath})");
        }

        static void SetLocationBackground(GameUIController controller, string locationId, Sprite sprite)
        {
            var so = new SerializedObject(controller);
            var entries = so.FindProperty("locationBackgrounds");
            var index = FindEntryIndex(entries, "locationId", locationId);

            if (index < 0)
            {
                index = entries.arraySize;
                entries.InsertArrayElementAtIndex(index);
                entries.GetArrayElementAtIndex(index).FindPropertyRelative("locationId").stringValue = locationId;
            }

            entries.GetArrayElementAtIndex(index).FindPropertyRelative("sprite").objectReferenceValue = sprite;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetCharacterPortrait(GameUIController controller, string characterId, Sprite sprite)
        {
            var so = new SerializedObject(controller);
            var entries = so.FindProperty("characterPortraits");
            var index = FindEntryIndex(entries, "characterId", characterId);

            if (index < 0)
            {
                index = entries.arraySize;
                entries.InsertArrayElementAtIndex(index);
                entries.GetArrayElementAtIndex(index).FindPropertyRelative("characterId").stringValue = characterId;
            }

            entries.GetArrayElementAtIndex(index).FindPropertyRelative("sprite").objectReferenceValue = sprite;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static int FindEntryIndex(SerializedProperty array, string idField, string id)
        {
            for (var i = 0; i < array.arraySize; i++)
            {
                if (array.GetArrayElementAtIndex(i).FindPropertyRelative(idField).stringValue == id)
                    return i;
            }

            return -1;
        }
    }
}
