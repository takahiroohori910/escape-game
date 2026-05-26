#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EscapeGame.EditorTools
{
    // シーン内 GameObject 名を一括で日本語化する一時 Editor。
    // 実行手順:
    //   1) EscapeGame/Rename/Dry Run で対象件数とリストを確認
    //   2) EscapeGame/Rename/Execute で本番リネーム → シーン保存
    //   3) 実行後、SceneNames.cs の const 値も日本語に書き換え（手動）
    //   4) 役目を終えたら本ファイルごと削除
    public static class RenameToJapanese
    {
        // 旧名 → 新名（日本語）のマッピング。SceneNames.cs と完全に同じセットを揃える。
        static readonly Dictionary<string, string> Mapping = new()
        {
            // ===== 共通 / Managers / Canvas =====
            ["Canvas_Main"]            = "メインキャンバス",
            ["Managers"]               = "マネージャー",
            ["_Prefabs"]               = "_プレハブ群",

            // ===== UI Panel / Overlay =====
            ["HintButton"]             = "ヒントボタン",
            ["HintPanel"]              = "ヒントパネル",
            ["MenuButton"]             = "戻るボタン",
            ["TitleOverlay"]           = "タイトルオーバーレイ",
            ["ClearOverlay"]           = "クリアオーバーレイ",
            ["TimerText"]              = "タイマーテキスト",
            ["SubText"]                = "サブテキスト",
            ["PopupPanel"]             = "ポップアップパネル",
            ["NoteOverlay"]            = "ノートオーバーレイ",
            ["ItemDetailPanel"]        = "アイテム詳細パネル",
            ["BookshelfStatusPanel"]   = "本棚ステータスパネル",
            ["InventoryBar"]           = "インベントリバー",
            ["NumberPadPanel"]         = "テンキーパネル",
            ["DisplayCabinetPanel"]    = "食器棚操作パネル",

            // ===== Room1 主要オブジェクト =====
            ["Bookshelf"]              = "本棚",
            ["DeskTop"]                = "机",
            ["Chest"]                  = "チェスト",
            ["DeskSafe"]               = "机の金庫",
            ["FurnitureCandidates"]    = "家具候補群",
            ["FireplacePhoto"]         = "暖炉跡の写真",
            ["FireplacePointLight"]    = "暖炉ライト",
            ["FireplaceOpening"]       = "暖炉開口部",
            ["FireEmber"]              = "火の燃え種",
            ["Clock"]                  = "時計",
            ["Painting"]               = "絵画",
            ["Telephone"]              = "電話",
            ["TelephoneHandset"]       = "電話受話器",
            ["NoteOnDesk"]             = "机のメモ",
            ["NoteOnBookshelf"]        = "本棚のメモ",
            ["NoteOnFireplace"]        = "暖炉のメモ",
            ["BackWall"]               = "奥壁",

            // ----- Bookshelf 内部 -----
            ["S2_01"]                  = "本棚棚2_書架01",
            ["BS_Left"]                = "本棚_左板",
            ["BS_Right"]               = "本棚_右板",
            ["BS_Bottom"]              = "本棚_底板",
            ["BS_Shelf1"]              = "本棚_棚板1",
            ["BS_Shelf2"]              = "本棚_棚板2",

            // ----- Chest 内部 -----
            ["ChestPoint"]             = "チェストカメラ位置",

            // ===== Room2 主要オブジェクト =====
            ["Room2"]                  = "部屋2",
            ["R2_StainedGlassRoot"]    = "部屋2_ステンドグラス群",
            ["R2_DisplayCabinetRoot"]  = "部屋2_食器棚群",
            ["R2_CandelabraRoot"]      = "部屋2_燭台群",
            ["R2_AltarRoot"]           = "部屋2_祭壇群",
            ["R2_ClickZone_StainedGlass"] = "部屋2_クリック領域_ステンドグラス",
            ["R2_ClickZone_Cabinet"]   = "部屋2_クリック領域_食器棚",
            ["R2_ClickZone_Candelabra"] = "部屋2_クリック領域_燭台",
            ["R2_ClickZone_Altar"]     = "部屋2_クリック領域_祭壇",
            ["CandelabraPuzzle"]       = "燭台パズル",
            ["Altar_Lock"]             = "祭壇の錠前",

            // ===== Camera Anchors / Points =====
            ["CameraAnchors"]          = "カメラアンカー群",
            ["Overview2Point"]         = "部屋2全景カメラ位置",
            ["StainedGlassPoint"]      = "ステンドグラスカメラ位置",
            ["DisplayCabinetPoint"]    = "食器棚カメラ位置",
            ["CandelabraPoint"]        = "燭台カメラ位置",
            ["PortraitPoint"]          = "肖像画カメラ位置",
            ["AltarPoint"]             = "祭壇カメラ位置",

            // ===== Lighting / Post Process =====
            ["Directional Light"]      = "方向ライト",
            ["GlobalPostProcessVolume"] = "ポストプロセス全体ボリューム",

            // ===== Controllers =====
            ["RoomViewController"]     = "部屋ビュー制御",
        };

        [MenuItem("EscapeGame/Rename/Dry Run (件数とリスト表示)")]
        public static void DryRun() => Run(dryRun: true);

        [MenuItem("EscapeGame/Rename/Execute (本実行 + シーン保存)")]
        public static void Execute() => Run(dryRun: false);

        static void Run(bool dryRun)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("[Rename] Edit mode で実行してください");
                return;
            }

            var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var hitsByOldName = new Dictionary<string, int>();
            var renamed = new List<(string oldName, string newName, string path)>();

            foreach (var t in all)
            {
                var name = t.gameObject.name;
                if (!Mapping.TryGetValue(name, out var newName)) continue;

                hitsByOldName.TryGetValue(name, out var c);
                hitsByOldName[name] = c + 1;
                renamed.Add((name, newName, GetHierarchyPath(t)));

                if (!dryRun)
                {
                    Undo.RecordObject(t.gameObject, "Rename to Japanese");
                    t.gameObject.name = newName;
                    EditorUtility.SetDirty(t.gameObject);
                }
            }

            Debug.Log($"[Rename] {(dryRun ? "DRY RUN" : "EXECUTED")}: 対象 {renamed.Count} 個 ({hitsByOldName.Count} 種類)");
            foreach (var kv in hitsByOldName)
                Debug.Log($"  {kv.Key} → {Mapping[kv.Key]}  ({kv.Value} 個)");

            if (!dryRun)
            {
                EditorSceneManager.MarkAllScenesDirty();
                EditorSceneManager.SaveOpenScenes();
                Debug.Log("[Rename] シーンを保存しました");
            }
        }

        static string GetHierarchyPath(Transform t)
        {
            var stack = new Stack<string>();
            var cur = t;
            while (cur != null) { stack.Push(cur.name); cur = cur.parent; }
            return string.Join("/", stack);
        }
    }
}
#endif
