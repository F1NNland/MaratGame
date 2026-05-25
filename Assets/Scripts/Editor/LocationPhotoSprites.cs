using MaratGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MaratGame.Editor
{
    static class LocationPhotoSprites
    {
        public static Sprite Load(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
                return null;

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (asset is Sprite sprite)
                    return sprite;
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            return texture != null
                ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f)
                : null;
        }

        public static Sprite LoadForLocation(string locationId) =>
            Load(LocationPhotoPaths.ResolveAssetPath(locationId));
    }
}
