#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EscapeGame.Core;
using EscapeGame.Game;

namespace EscapeGame.EditorTools
{
    // 過去の Editor 再実行で累積した重複 UI GameObject を一括削除する。
    // 保護対象：主要 UI スクリプトの panel/overlay フィールドが指す GameObject とその子孫のみ。
    // それ以外で TargetNames に一致するものを削除する。
    public static class CleanupDuplicateUI
    {
        static readonly string[] TargetNames = {
            "AltarPanel", "DisplayCabinetPanel", "NumberPadPanel",
            "NotePanel", "NoteOverlay",
            "CodeDisplay", "ButtonGrid", "TitleText",
            "ItemDetailPanel", "ClearOverlay", "SubText"
        };

        [MenuItem("EscapeGame/Cleanup/Duplicate UI Panels (Dry Run)")]
        public static void DryRun() => Execute(dryRun: true);

        [MenuItem("EscapeGame/Cleanup/Duplicate UI Panels")]
        public static void Run() => Execute(dryRun: false);

        static void Execute(bool dryRun)
        {
            var keep = new HashSet<int>();

            // 主要 UI の panel/overlay 参照のみ保護対象に追加
            ProtectFromUI<TitleUI>(keep, "overlay");
            ProtectFromUI<AltarUI>(keep, "panel");
            ProtectFromUI<AltarUI>(keep, "codeDisplay");
            ProtectFromUI<NumberPadUI>(keep, "panel");
            ProtectFromUI<NumberPadUI>(keep, "codeDisplay");
            ProtectFromUI<NoteUI>(keep, "overlay");
            ProtectFromUI<ItemDetailUI>(keep, "panel");
            ProtectFromUI<PopupUI>(keep, "panel");
            ProtectFromUI<HintUI>(keep, "panel");
            ProtectFromUI<BookshelfStatusUI>(keep, "panel");
            ProtectFromUI<GameClearUI>(keep, "overlay");
            ProtectFromUI<GameClearUI>(keep, "subText");

            int deleted = 0;
            foreach (var name in TargetNames)
            {
                if (dryRun)
                {
                    var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (var t in all)
                    {
                        if (t == null || t.name != name) continue;
                        if (keep.Contains(t.gameObject.GetInstanceID())) continue;
                        Debug.Log($"[CleanupDuplicateUI] 削除候補: {GetPath(t)}");
                        deleted++;
                    }
                }
                else
                {
                    bool repeat = true;
                    while (repeat)
                    {
                        repeat = false;
                        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                        foreach (var t in all)
                        {
                            if (t == null || t.name != name) continue;
                            if (keep.Contains(t.gameObject.GetInstanceID())) continue;
                            Debug.Log($"[CleanupDuplicateUI] 削除: {GetPath(t)}");
                            Object.DestroyImmediate(t.gameObject);
                            deleted++;
                            repeat = true;
                            break;
                        }
                    }
                }
            }

            if (!dryRun)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
            }
            Debug.Log($"[CleanupDuplicateUI] 完了: {deleted} 個{(dryRun ? "が削除候補（DryRun）" : "削除")}");
        }

        static void ProtectFromUI<T>(HashSet<int> keep, string fieldName) where T : MonoBehaviour
        {
            var ui = Object.FindAnyObjectByType<T>();
            if (ui == null) return;
            var so = new SerializedObject(ui);
            var prop = so.FindProperty(fieldName);
            if (prop == null || prop.objectReferenceValue == null) return;
            var obj = prop.objectReferenceValue;
            GameObject go = obj as GameObject ?? (obj as Component)?.gameObject;
            if (go == null) return;
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
            {
                keep.Add(t.gameObject.GetInstanceID());
            }
        }

        // 祭壇クリックを邪魔する装飾 Collider を無効化＋全 TMP の LiberationSans を NotoSansJP_Fresh に置換
        [MenuItem("EscapeGame/Cleanup/Fix Altar Click & Fonts")]
        public static void FixAltarClickAndFonts()
        {
            int colliderCount = 0;
            var allColliders = Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in allColliders)
            {
                if (c == null) continue;
                var n = c.gameObject.name;
                if (n.StartsWith("AltarFlame") || n.StartsWith("AltarCandle"))
                {
                    if (c.enabled) { c.enabled = false; colliderCount++; }
                }
            }

            int fontCount = 0;
            var jpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPaths.Font_NotoSansJP_Fresh);
            if (jpFont != null)
            {
                var allTMP = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var t in allTMP)
                {
                    if (t == null || t.font == null) continue;
                    if (t.font.name.Contains("LiberationSans"))
                    {
                        t.font = jpFont;
                        fontCount++;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[FixAltar] NotoSansJP_Fresh.asset が見つかりません");
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[FixAltar] 完了: 装飾Collider無効化={colliderCount}個, フォント置換={fontCount}個");
        }

        // 主要 panel/overlay の Image に Unity 標準 UISprite を設定（背景の枠が描画されない問題対策）
        [MenuItem("EscapeGame/Cleanup/Fix UI Backgrounds")]
        public static void FixUIBackgrounds()
        {
            string[] targets = {
                "AltarPanel", "DisplayCabinetPanel", "NumberPadPanel",
                "NotePanel", "NoteOverlay",
                "TitleOverlay", "GameClearOverlay"
            };
            var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (sprite == null)
            {
                Debug.LogError("[FixUIBackgrounds] 標準 UISprite が取得できません");
                return;
            }
            int count = 0;
            var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in allTransforms)
            {
                if (t == null) continue;
                if (System.Array.IndexOf(targets, t.name) < 0) continue;
                var img = t.GetComponent<Image>();
                if (img == null) continue;
                if (img.sprite == null)
                {
                    img.sprite = sprite;
                    count++;
                    Debug.Log($"[FixUIBackgrounds] Sprite 設定: {GetPath(t)}");
                }
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[FixUIBackgrounds] 完了: {count} 個");
        }

        // Fix UI Backgrounds で当てた UISprite を null に戻す（ふんわり効果が発生したため）
        [MenuItem("EscapeGame/Cleanup/Revert UISprite (null に戻す)")]
        public static void RevertUISprite()
        {
            var ui = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            int count = 0;
            var allImages = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var img in allImages)
            {
                if (img == null) continue;
                if (img.sprite == ui)
                {
                    img.sprite = null;
                    count++;
                }
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[RevertUISprite] {count} 個の Image を Sprite=null に戻しました");
        }

        static string GetPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
#endif
