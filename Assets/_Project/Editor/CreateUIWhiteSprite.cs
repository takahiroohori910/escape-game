#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeGame.EditorTools
{
    // 1x1 白 PNG を Project に作成し、Sprite として import → 主要 panel/overlay の Image にだけ明示的に設定する。
    // 影響範囲：UIWhite.png 新規作成 + 当該 5 個の Image のみ。grep ベースの一括書き換えはしない。
    public static class CreateUIWhiteSprite
    {
        const string SpriteDir  = "Assets/_Project/Sprites";
        const string SpritePath = "Assets/_Project/Sprites/UIWhite.png";

        // 設定対象の GameObject 名（厳密一致のみ）
        static readonly string[] TargetGameObjectNames = {
            "AltarPanel", "NumberPadPanel", "NoteOverlay", "NotePanel", "TitleOverlay"
        };

        [MenuItem("EscapeGame/Cleanup/Create UIWhite Sprite & Apply")]
        public static void Run()
        {
            if (!AssetDatabase.IsValidFolder(SpriteDir))
            {
                Directory.CreateDirectory(SpriteDir);
                AssetDatabase.Refresh();
            }

            // 1x1 白 PNG を生成
            if (!File.Exists(SpritePath))
            {
                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                File.WriteAllBytes(SpritePath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(SpritePath);
                Debug.Log($"[CreateUIWhiteSprite] 作成: {SpritePath}");
            }

            // Sprite として import 設定（同期インポート）
            var importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
            AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceSynchronousImport);

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (sprite == null)
            {
                Debug.LogError($"[CreateUIWhiteSprite] Sprite が読めません: {SpritePath} (importer type: {(importer != null ? importer.textureType.ToString() : "null")})");
                return;
            }
            Debug.Log($"[CreateUIWhiteSprite] Sprite ロード成功: {sprite.name}");

            // 明示的に対象 GameObject 名の Image にだけ設定
            int applied = 0;
            var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in allTransforms)
            {
                if (t == null) continue;
                if (System.Array.IndexOf(TargetGameObjectNames, t.name) < 0) continue;
                var img = t.GetComponent<Image>();
                if (img == null) continue;
                img.sprite = sprite;
                applied++;
                Debug.Log($"[CreateUIWhiteSprite] 設定: {GetPath(t)}");
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[CreateUIWhiteSprite] 完了: {applied} 個の Image に UIWhite を設定");
        }

        static string GetPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }
    }
}
#endif
