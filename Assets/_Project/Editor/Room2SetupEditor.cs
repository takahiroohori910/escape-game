#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EscapeGame.Core;
using EscapeGame.Game;

public class Room2SetupEditor : EditorWindow
{
    // Room2はRoom1の奥に隣接。Room1のドアがz≈5.85なのでRoom2はz=6から開始。
    private static readonly Vector3 R2 = new Vector3(0f, 0f, 6f);

    [MenuItem("EscapeGame/Setup/Build Room2 Scene")]
    public static void Run()
    {
        if (!GitGuard.RequireCleanGit("Build Room2 Scene")) return;
        CreateScriptableObjects();
        CreateRoom2CameraPoints();
        CreateRoom2Geometry();
        CreateRoom1Additions();
        WireRoom1Clear();
        WireRoom2UIs();
        CreateRoom2UIPanels();

        EditorUtility.SetDirty(GameObject.Find("RoomViewController"));
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Room2Setup] 完了！");
    }

    // 燭台のみ再生成
    [MenuItem("EscapeGame/Setup/Rebuild Candelabra")]
    public static void RebuildCandelabra()
    {
        if (Application.isPlaying) { Debug.LogError("Edit mode で実行してください"); return; }

        var room2Root = GameObject.Find("Room2");
        if (room2Root == null) { Debug.LogError("[Room2Setup] Room2 not found"); return; }

        var existing = GameObject.Find("R2_CandelabraRoot");
        if (existing != null) Object.DestroyImmediate(existing);
        var existingZone = GameObject.Find("R2_ClickZone_Candelabra");
        if (existingZone != null) Object.DestroyImmediate(existingZone);
        var existingPuzzle = GameObject.Find("CandelabraPuzzle");
        if (existingPuzzle != null) Object.DestroyImmediate(existingPuzzle);

        var matGold   = GetOrCreateMatURP("Mat_R2_Gold",   new Color(0.85f,0.68f,0.12f), 0.9f, 0.78f);
        var matCandle = GetOrCreateMatURP("Mat_R2_Candle", new Color(0.95f,0.92f,0.82f), 0f, 0.10f);
        var matFlame  = GetOrCreateMatURP("Mat_R2_Flame",  new Color(1.00f,0.55f,0.05f), 0f, 0f, new Color(6.0f,2.4f,0.16f));

        BuildCandelabra(room2Root, matGold, matCandle, matFlame);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[Room2Setup] Rebuild Candelabra 完了");
    }

    // ステンドグラスのみ再生成
    [MenuItem("EscapeGame/Setup/Rebuild StainedGlass")]
    public static void RebuildStainedGlass()
    {
        if (Application.isPlaying) { Debug.LogError("Edit mode で実行してください"); return; }

        var room2Root = GameObject.Find("Room2");
        if (room2Root == null) { Debug.LogError("[Room2Setup] Room2 not found"); return; }

        var existing = GameObject.Find("R2_StainedGlassRoot");
        if (existing != null) Object.DestroyImmediate(existing);
        var existingZone = GameObject.Find("R2_ClickZone_StainedGlass");
        if (existingZone != null) Object.DestroyImmediate(existingZone);

        var matWood = GetOrCreateMatURP("Mat_R2_Wood", new Color(0.30f,0.18f,0.08f), 0f, 0.18f);

        // ScriptableObject の参照を取り直す（メニュー実行時は CreateScriptableObjects 呼ばないため）
        noteStainedGlassPlaque = AssetDatabase.LoadAssetAtPath<NoteData>("Assets/_Project/ScriptableObjects/Notes/NoteStainedGlassPlaque.asset");

        BuildStainedGlass(room2Root, matWood);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[Room2Setup] Rebuild StainedGlass 完了");
    }

    // 食器棚のみ再生成（破壊的：内部の食器配置や解錠装置も全て再生成される）
    [MenuItem("EscapeGame/Setup/Rebuild DisplayCabinet")]
    public static void RebuildDisplayCabinet()
    {
        if (Application.isPlaying) { Debug.LogError("Edit mode で実行してください"); return; }
        if (!GitGuard.RequireCleanGit("Rebuild DisplayCabinet")) return;

        var room2Root = GameObject.Find("Room2");
        if (room2Root == null) { Debug.LogError("[Room2Setup] Room2 not found"); return; }

        var existing = GameObject.Find("R2_DisplayCabinetRoot");
        if (existing != null) Object.DestroyImmediate(existing);
        var existingZone = GameObject.Find("R2_ClickZone_Cabinet");
        if (existingZone != null) Object.DestroyImmediate(existingZone);
        // 旧テンキーパネルが残っていれば消す
        var oldPanel = GameObject.Find("DisplayCabinetPanel");
        if (oldPanel != null) Object.DestroyImmediate(oldPanel);

        // ヒントノートだけ参照を取り直す
        noteCabinetHint = AssetDatabase.LoadAssetAtPath<NoteData>("Assets/_Project/ScriptableObjects/Notes/NoteCabinetHint.asset");

        var matDarkWood = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Generated/Mat_Wood_Dark.mat")
                          ?? GetOrCreateMatURP("Mat_R2_DarkWood",  new Color(0.18f,0.10f,0.04f), 0f, 0.22f);
        var matGold     = GetOrCreateMatURP("Mat_R2_Gold",  new Color(0.85f,0.68f,0.12f), 0.9f, 0.78f);
        var matStone    = GetOrCreateMatURP("Mat_R2_Stone", new Color(0.45f,0.40f,0.36f), 0f, 0.12f);

        BuildDisplayCabinet(room2Root, matDarkWood, matGold, matStone);

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[Room2Setup] Rebuild DisplayCabinet 完了");
    }

    // ─────────────────────────────────────────
    // 1. ScriptableObjectアセット作成
    // ─────────────────────────────────────────
    static NoteData noteHiddenBook, noteCabinetHint, notePortraitSecret,
                    noteClockHint, noteDrawerMemo, noteStainedGlassPlaque;
    static ItemData itemRoomKey;

    static void CreateScriptableObjects()
    {
        string noteDir = "Assets/_Project/ScriptableObjects/Notes";
        string itemDir = "Assets/_Project/ScriptableObjects/Items";
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "../", noteDir));
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "../", itemDir));

        noteHiddenBook = CreateNote(noteDir, "NoteHiddenBook",
            "家系図の一ページ",
            "この家の紋章の序列\n\n一に指輪（契約）\n二に剣（守護）\n三に王冠（権威）\n四に書物（叡智）\n\n──先祖代々の掟");

        noteCabinetHint = CreateNote(noteDir, "NoteCabinetHint",
            "燭台の管理書",
            "儀式の燭台は奇数の位置に火を灯すこと\n\n●○●○●\n（左から1・3・5番目）\n\n──管理人より");

        notePortraitSecret = CreateNote(noteDir, "NotePortraitSecret",
            "隠し引き出しのメモ",
            "祭壇の封印を解く数字\n\n  7  4  2  9\n\n──この数字は墓まで持っていけ");

        noteClockHint = CreateNote(noteDir, "NoteClockHint",
            "時計台座の刻印",
            "隣の間のステンドグラスを読め\n紋様の数を順に記せ\n\n一：薔薇　二：十字\n三：星　　四：菱形");

        noteDrawerMemo = CreateNote(noteDir, "NoteDrawerMemo",
            "食器棚の上のメモ",
            "燭台　奇の位に灯を──\n(1, 3, 5)\n\n詳しくは錠前を開けよ");

        noteStainedGlassPlaque = CreateNote(noteDir, "NoteStainedGlassPlaque",
            "ステンドグラス解説板",
            "このガラスに刻まれた紋様の数を\n以下の順番で記録せよ\n\n1. 薔薇（ローズ）\n2. 十字（クロス）\n3. 星（スター）\n4. 菱形（ダイヤ）\n\n──紋様の個数が扉の鍵となる");

        itemRoomKey = CreateItem(itemDir, "RoomKey",
            ItemIds.RoomKey, "祭壇の間の鍵",
            "重厚な鉄の鍵。隣の部屋の扉に使えそうだ。");

        AssetDatabase.SaveAssets();
        Debug.Log("[Room2Setup] ScriptableObject作成完了");
    }

    static NoteData CreateNote(string dir, string fileName, string title, string content)
    {
        string path = $"{dir}/{fileName}.asset";
        var note = AssetDatabase.LoadAssetAtPath<NoteData>(path);
        if (note == null)
        {
            note = ScriptableObject.CreateInstance<NoteData>();
            AssetDatabase.CreateAsset(note, path);
        }
        SetPrivate(note, "title", title);
        SetPrivate(note, "content", content);
        SetPrivate(note, "noteId", fileName.ToLower());
        EditorUtility.SetDirty(note);
        return note;
    }

    static ItemData CreateItem(string dir, string fileName, string id, string name, string desc)
    {
        string path = $"{dir}/{fileName}.asset";
        var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        if (item == null)
        {
            item = ScriptableObject.CreateInstance<ItemData>();
            AssetDatabase.CreateAsset(item, path);
        }
        SetPrivate(item, "itemId", id);
        SetPrivate(item, "itemName", name);
        SetPrivate(item, "description", desc);
        EditorUtility.SetDirty(item);
        return item;
    }

    static void SetPrivate(Object obj, string field, object val)
    {
        var f = obj.GetType().GetField(field,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        f?.SetValue(obj, val);
    }

    // ─────────────────────────────────────────
    // 2. Room2カメラポイント
    // ─────────────────────────────────────────
    static void CreateRoom2CameraPoints()
    {
        var anchors = GameObject.Find("CameraAnchors");
        if (anchors == null) { Debug.LogError("CameraAnchorsが見つかりません"); return; }

        // Overview2: 高い位置から部屋全体を見渡す（入口付近・高め）
        EnsureCameraPoint(anchors.transform, "Overview2Point",
            R2 + new Vector3(0f, 3.2f, 0.3f), new Vector3(22f, 0f, 0f));

        // StainedGlass: 右壁を正面から（やや近寄って迫力を出す）
        EnsureCameraPoint(anchors.transform, "StainedGlassPoint",
            R2 + new Vector3(1.5f, 2.5f, 7f), new Vector3(-5f, 90f, 0f));

        // DisplayCabinet: 左壁の食器棚を正面から
        EnsureCameraPoint(anchors.transform, "DisplayCabinetPoint",
            R2 + new Vector3(-1.11f, 1.59f, 7.01f), new Vector3(0f, 270f, 0f));

        // Candelabra: やや後ろから燭台全体を見渡す
        EnsureCameraPoint(anchors.transform, "CandelabraPoint",
            R2 + new Vector3(0f, 2.0f, 1.8f), new Vector3(12f, 0f, 0f));

        // Portrait: 祭壇上空から肖像画を見る
        EnsureCameraPoint(anchors.transform, "PortraitPoint",
            R2 + new Vector3(0f, 3.84f, 5.9f), new Vector3(9.45f, 0f, 0f));

        // Altar: 祭壇を正面から
        EnsureCameraPoint(anchors.transform, "AltarPoint",
            R2 + new Vector3(0f, 2.92f, 5.44f), new Vector3(0f, 0f, 0f));

        var rvc = Object.FindAnyObjectByType<RoomViewController>();
        if (rvc == null) { Debug.LogError("RoomViewControllerが見つかりません"); return; }

        var so = new SerializedObject(rvc);
        so.FindProperty("overview2Point").objectReferenceValue      = GameObject.Find("Overview2Point")?.transform;
        so.FindProperty("stainedGlassPoint").objectReferenceValue   = GameObject.Find("StainedGlassPoint")?.transform;
        so.FindProperty("displayCabinetPoint").objectReferenceValue = GameObject.Find("DisplayCabinetPoint")?.transform;
        so.FindProperty("candelabraPoint").objectReferenceValue     = GameObject.Find("CandelabraPoint")?.transform;
        so.FindProperty("portraitPoint").objectReferenceValue       = GameObject.Find("PortraitPoint")?.transform;
        so.FindProperty("altarPoint").objectReferenceValue          = GameObject.Find("AltarPoint")?.transform;
        so.ApplyModifiedProperties();

        Debug.Log("[Room2Setup] カメラポイント設定完了");
    }

    static void EnsureCameraPoint(Transform parent, string name, Vector3 pos, Vector3 rot)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.eulerAngles = rot;
        }
        else
        {
            Debug.Log($"[Room2Setup] {name} は既存値を保持（リセットしたい場合は EscapeGame/Reset Camera Point/{name}）");
        }
    }

    // ─────────────────────────────────────────
    // CameraPoint デフォルト値定義（リセット用に共通化）
    // ─────────────────────────────────────────
    static (Vector3 pos, Vector3 rot)? GetCameraPointDefault(string name)
    {
        switch (name)
        {
            case "Overview2Point":      return (R2 + new Vector3(0f, 3.2f, 0.3f), new Vector3(22f, 0f, 0f));
            case "StainedGlassPoint":   return (R2 + new Vector3(1.5f, 2.5f, 7f), new Vector3(-5f, 90f, 0f));
            case "DisplayCabinetPoint": return (R2 + new Vector3(-1.11f, 1.59f, 7.01f), new Vector3(0f, 270f, 0f));
            case "CandelabraPoint":     return (R2 + new Vector3(0f, 2.0f, 1.8f), new Vector3(12f, 0f, 0f));
            case "PortraitPoint":       return (R2 + new Vector3(0f, 3.84f, 5.9f), new Vector3(9.45f, 0f, 0f));
            case "AltarPoint":          return (R2 + new Vector3(0f, 2.92f, 5.44f), new Vector3(0f, 0f, 0f));
            default: return null;
        }
    }

    static void ResetCameraPointInternal(string name)
    {
        var anchors = GameObject.Find("CameraAnchors");
        if (anchors == null) { Debug.LogError("CameraAnchorsが見つかりません"); return; }
        var defaults = GetCameraPointDefault(name);
        if (!defaults.HasValue) { Debug.LogError($"[Reset] {name} は未知のカメラポイント"); return; }

        var existing = GameObject.Find(name);
        if (existing != null) Object.DestroyImmediate(existing);

        var go = new GameObject(name);
        go.transform.SetParent(anchors.transform);
        go.transform.position = defaults.Value.pos;
        go.transform.eulerAngles = defaults.Value.rot;

        // RoomViewController の参照を再設定
        var rvc = Object.FindAnyObjectByType<RoomViewController>();
        if (rvc != null)
        {
            var fieldName = char.ToLower(name[0]) + name.Substring(1);
            var so = new SerializedObject(rvc);
            var prop = so.FindProperty(fieldName);
            if (prop != null) { prop.objectReferenceValue = go.transform; so.ApplyModifiedProperties(); }
        }
        Debug.Log($"[Reset] {name} を定数値にリセット: pos={defaults.Value.pos} rot={defaults.Value.rot}");
    }

    [MenuItem("EscapeGame/Reset Camera Point/All")]
    public static void ResetAllCameraPoints()
    {
        if (!GitGuard.RequireCleanGit("Reset All Camera Points")) return;
        var anchors = GameObject.Find("CameraAnchors");
        if (anchors == null) { Debug.LogError("CameraAnchorsが見つかりません"); return; }
        foreach (var n in new[] { "Overview2Point", "StainedGlassPoint", "DisplayCabinetPoint",
                                  "CandelabraPoint", "PortraitPoint", "AltarPoint" })
        {
            var existing = GameObject.Find(n);
            if (existing != null) Object.DestroyImmediate(existing);
        }
        CreateRoom2CameraPoints();
        Debug.Log("[Reset] 全カメラポイントを定数値にリセット");
    }

    [MenuItem("EscapeGame/Reset Camera Point/AltarPoint")]
    public static void ResetAltarPoint()
    { if (!GitGuard.RequireCleanGit("Reset AltarPoint")) return; ResetCameraPointInternal("AltarPoint"); }

    [MenuItem("EscapeGame/Reset Camera Point/PortraitPoint")]
    public static void ResetPortraitPoint()
    { if (!GitGuard.RequireCleanGit("Reset PortraitPoint")) return; ResetCameraPointInternal("PortraitPoint"); }

    [MenuItem("EscapeGame/Reset Camera Point/DisplayCabinetPoint")]
    public static void ResetDisplayCabinetPoint()
    { if (!GitGuard.RequireCleanGit("Reset DisplayCabinetPoint")) return; ResetCameraPointInternal("DisplayCabinetPoint"); }

    [MenuItem("EscapeGame/Reset Camera Point/CandelabraPoint")]
    public static void ResetCandelabraPoint()
    { if (!GitGuard.RequireCleanGit("Reset CandelabraPoint")) return; ResetCameraPointInternal("CandelabraPoint"); }

    [MenuItem("EscapeGame/Reset Camera Point/StainedGlassPoint")]
    public static void ResetStainedGlassPoint()
    { if (!GitGuard.RequireCleanGit("Reset StainedGlassPoint")) return; ResetCameraPointInternal("StainedGlassPoint"); }

    [MenuItem("EscapeGame/Reset Camera Point/Overview2Point")]
    public static void ResetOverview2Point()
    { if (!GitGuard.RequireCleanGit("Reset Overview2Point")) return; ResetCameraPointInternal("Overview2Point"); }

    // ─────────────────────────────────────────
    // 3. Room2の3Dジオメトリ（10倍クオリティ版）
    // ─────────────────────────────────────────
    static void CreateRoom2Geometry()
    {
        var existing = GameObject.Find("Room2");
        if (existing != null) Object.DestroyImmediate(existing);
        foreach (var n in new[] { "PortraitPuzzle", "CandelabraPuzzle" })
        {
            var go = GameObject.Find(n);
            if (go != null) Object.DestroyImmediate(go);
        }

        var room2Root = new GameObject("Room2");
        room2Root.transform.position = R2;

        // ── マテリアル定義 ──
        // 壁・床・天井は Room1 のテクスチャ付き Material（Generated/）を共有して質感を揃える
        var matWall      = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Generated/Mat_Wall.mat")
                          ?? GetOrCreateMatURP("Mat_R2_Wall",      new Color(0.22f,0.18f,0.14f), 0f, 0.06f);
        var matWallLight = GetOrCreateMatURP("Mat_R2_WallLight",  new Color(0.18f,0.14f,0.11f), 0f, 0.08f);
        var matFloor     = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Generated/Mat_Floor.mat")
                          ?? GetOrCreateMatURP("Mat_R2_Floor",     new Color(0.28f,0.18f,0.10f), 0f, 0.12f);
        var matFloorTile = GetOrCreateMatURP("Mat_R2_FloorTile", new Color(0.16f,0.13f,0.10f), 0f, 0.20f);
        var matCeiling   = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Generated/Mat_Ceiling.mat")
                          ?? GetOrCreateMatURP("Mat_R2_Ceiling",   new Color(0.85f,0.82f,0.76f), 0f, 0.05f);
        var matWood      = GetOrCreateMatURP("Mat_R2_Wood",      new Color(0.30f,0.18f,0.08f), 0f, 0.18f);
        // 食器棚など暗木材は Room1 のテクスチャ付き Material（worn_planks）を共有
        var matDarkWood  = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Generated/Mat_Wood_Dark.mat")
                          ?? GetOrCreateMatURP("Mat_R2_DarkWood",  new Color(0.18f,0.10f,0.04f), 0f, 0.22f);
        var matGold      = GetOrCreateMatURP("Mat_R2_Gold",      new Color(0.85f,0.68f,0.12f), 0.9f, 0.78f);
        var matGoldDull  = GetOrCreateMatURP("Mat_R2_GoldDull",  new Color(0.70f,0.55f,0.10f), 0.7f, 0.55f);
        var matStone     = GetOrCreateMatURP("Mat_R2_Stone",     new Color(0.45f,0.40f,0.36f), 0f, 0.12f);
        var matDarkStone = GetOrCreateMatURP("Mat_R2_DarkStone", new Color(0.22f,0.19f,0.17f), 0f, 0.08f);
        var matPortrait  = GetOrCreateMatURP("Mat_R2_Portrait",  new Color(0.14f,0.09f,0.07f), 0f, 0.05f);
        var matCandle    = GetOrCreateMatURP("Mat_R2_Candle",    new Color(0.95f,0.92f,0.82f), 0f, 0.10f);
        var matFlame     = GetOrCreateMatURP("Mat_R2_Flame",     new Color(1.00f,0.55f,0.05f), 0f, 0.0f,
                               new Color(6.0f, 2.4f, 0.16f));

        // ── 外壁（石造り、w=10, h=5, local z=0-12）──
        CreateBox(room2Root, "R2_Floor",    new Vector3(0f,-0.1f, 6f), new Vector3(10f,0.20f,12f), matFloor);
        CreateBox(room2Root, "R2_Ceiling",  new Vector3(0f, 5.1f, 6f), new Vector3(10f,0.20f,12f), matCeiling);
        CreateBox(room2Root, "R2_BackWall", new Vector3(0f, 2.5f,12.1f),new Vector3(10f,5f,0.20f), matWall);
        CreateBox(room2Root, "R2_LeftWall", new Vector3(-5.1f,2.5f,6f), new Vector3(0.20f,5f,12f), matWall);
        CreateBox(room2Root, "R2_RightWall",new Vector3( 5.1f,2.5f,6f), new Vector3(0.20f,5f,12f), matWall);
        // 手前壁（ドア開口部を残す）
        CreateBox(room2Root, "R2_FrontWallL",new Vector3(-3.3f,2.5f,0.1f), new Vector3(3.4f,5f,0.2f), matWall);
        CreateBox(room2Root, "R2_FrontWallR",new Vector3( 3.3f,2.5f,0.1f), new Vector3(3.4f,5f,0.2f), matWall);
        CreateBox(room2Root, "R2_FrontWallT",new Vector3(0f,   4.2f,0.1f), new Vector3(3.4f,1.8f,0.2f),matWall);

        // ── 床タイル目地（十字ライン、暗い色の帯）──
        CreateBox(room2Root, "R2_TileH1", new Vector3(0f,-0.01f,4f),  new Vector3(10f,0.02f,0.08f), matDarkStone);
        CreateBox(room2Root, "R2_TileH2", new Vector3(0f,-0.01f,8f),  new Vector3(10f,0.02f,0.08f), matDarkStone);
        CreateBox(room2Root, "R2_TileH3", new Vector3(0f,-0.01f,10f), new Vector3(10f,0.02f,0.08f), matDarkStone);
        CreateBox(room2Root, "R2_TileV1", new Vector3(-2.5f,-0.01f,6f),new Vector3(0.08f,0.02f,12f),matDarkStone);
        CreateBox(room2Root, "R2_TileV2", new Vector3( 2.5f,-0.01f,6f),new Vector3(0.08f,0.02f,12f),matDarkStone);

        // ── 幅木（石と壁の境目）──
        CreateBox(room2Root, "R2_Base_Back",  new Vector3(0f,0.18f,12.0f), new Vector3(10f,0.36f,0.12f), matDarkStone);
        CreateBox(room2Root, "R2_Base_Left",  new Vector3(-5.0f,0.18f,6f), new Vector3(0.12f,0.36f,12f), matDarkStone);
        CreateBox(room2Root, "R2_Base_Right", new Vector3( 5.0f,0.18f,6f), new Vector3(0.12f,0.36f,12f), matDarkStone);

        // ── 建築装飾（梁・柱・アーチ）──
        BuildArchitecturalDetails(room2Root, matWall, matDarkStone, matGold, matGoldDull);

        // ── 各オブジェクト ──
        BuildStainedGlass(room2Root, matWood);
        BuildDisplayCabinet(room2Root, matDarkWood, matGold, matStone);
        BuildCandelabra(room2Root, matGold, matCandle, matFlame);
        BuildPortrait(room2Root, matGold, matPortrait);
        BuildAltar(room2Root, matStone, matDarkStone, matGold, matCandle, matFlame);
        BuildWallSconces(room2Root, matGoldDull, matCandle, matFlame);

        // ── 燭台ヒントノート（食器棚上）──
        var cabinetNote = new GameObject("R2_CabinetTopNote");
        cabinetNote.transform.SetParent(room2Root.transform, false);
        cabinetNote.transform.localPosition = new Vector3(-4f,2.65f,7f);
        var cabinetNoteBox = CreateBox(cabinetNote, "CabinetNoteDecor",
            Vector3.zero, new Vector3(0.20f,0.15f,0.13f),
            GetOrCreateMatURP("Mat_CabNoteDecor", new Color(0.55f,0.35f,0.12f)));
        AddBoxCollider(cabinetNoteBox, new Vector3(1.2f,1.2f,1.2f));
        var cabinetNoteI = cabinetNoteBox.GetComponent<NoteInteractable>() ?? cabinetNoteBox.AddComponent<NoteInteractable>();
        SetPrivate(cabinetNoteI, "noteData", noteDrawerMemo);
        SetPrivate(cabinetNoteI, "requiredArea", RoomArea.DisplayCabinet);

        // ── 照明（全体を大幅強化）──
        // 主照明: 部屋中央高部、暖白色で全体を明るく
        // R2_MainLight は天井に白い光ドームが目立つため生成しない（暖炉や蝋燭の暖色光のみで照らす）
        // AddPointLight(room2Root, "R2_MainLight",    new Vector3(0f,  4.6f, 6f),   new Color(0.95f,0.88f,0.70f), 6.5f, 24f);
        // 入口エリア照明
        AddPointLight(room2Root, "R2_EntryLight",   new Vector3(0f,  3.6f, 1.5f), new Color(0.90f,0.75f,0.50f), 2.5f,  8f);
        // ステンドグラス側の彩色光（神秘的な紫）
        AddPointLight(room2Root, "R2_SGColorLight", new Vector3(3.2f,2.8f, 7f),   new Color(0.35f,0.10f,0.90f), 0.8f, 10f);
        // 祭壇エリアの神秘的な青白光
        // R2_AltarAmbient は天井に光ドームが出るため非表示
        // AddPointLight(room2Root, "R2_AltarAmbient", new Vector3(0f,  4.0f,10.5f), new Color(0.85f,0.70f,0.45f), 1.2f,  9f);
        // 奥壁（肖像画側）の暖色光
        AddPointLight(room2Root, "R2_BackLight",    new Vector3(0f,  3.5f,11.5f), new Color(0.85f,0.70f,0.45f), 0.6f,  7f);

        Debug.Log("[Room2Setup] Room2ジオメトリ作成完了");
    }

    // ── 建築装飾（梁・石柱・入口アーチ）──
    static void BuildArchitecturalDetails(GameObject room2Root, Material matWall, Material matDarkStone, Material matGold, Material matGoldDull)
    {
        // 天井梁（横方向、3本）
        CreateBox(room2Root, "R2_Beam1", new Vector3(0f,4.82f,3f),  new Vector3(10f,0.28f,0.45f), matDarkStone);
        CreateBox(room2Root, "R2_Beam2", new Vector3(0f,4.82f,7f),  new Vector3(10f,0.28f,0.45f), matDarkStone);
        CreateBox(room2Root, "R2_Beam3", new Vector3(0f,4.82f,10.5f),new Vector3(10f,0.28f,0.45f), matDarkStone);

        // 石柱（4本、左右壁際）
        var matPillar = GetOrCreateMatURP("Mat_R2_Pillar", new Color(0.35f,0.30f,0.26f), 0f, 0.12f);
        CreateCylinder(room2Root, "R2_PillarFL", new Vector3(-4.3f,2.5f,3f),  new Vector3(0.35f,2.5f,0.35f), matPillar);
        CreateCylinder(room2Root, "R2_PillarFR", new Vector3( 4.3f,2.5f,3f),  new Vector3(0.35f,2.5f,0.35f), matPillar);
        CreateCylinder(room2Root, "R2_PillarBL", new Vector3(-4.3f,2.5f,10f), new Vector3(0.35f,2.5f,0.35f), matPillar);
        CreateCylinder(room2Root, "R2_PillarBR", new Vector3( 4.3f,2.5f,10f), new Vector3(0.35f,2.5f,0.35f), matPillar);

        // 柱頭（Cube）
        CreateBox(room2Root, "R2_CapFL", new Vector3(-4.3f,4.9f,3f),  new Vector3(0.55f,0.20f,0.55f), matDarkStone);
        CreateBox(room2Root, "R2_CapFR", new Vector3( 4.3f,4.9f,3f),  new Vector3(0.55f,0.20f,0.55f), matDarkStone);
        CreateBox(room2Root, "R2_CapBL", new Vector3(-4.3f,4.9f,10f), new Vector3(0.55f,0.20f,0.55f), matDarkStone);
        CreateBox(room2Root, "R2_CapBR", new Vector3( 4.3f,4.9f,10f), new Vector3(0.55f,0.20f,0.55f), matDarkStone);

        // 入口アーチ装飾（ドア上部）
        CreateBox(room2Root, "R2_ArchTop",    new Vector3(0f,3.4f,0.08f), new Vector3(1.55f,0.22f,0.18f), matGoldDull);
        CreateBox(room2Root, "R2_ArchLeft",   new Vector3(-0.78f,1.5f,0.08f), new Vector3(0.10f,3.0f,0.12f), matGoldDull);
        CreateBox(room2Root, "R2_ArchRight",  new Vector3( 0.78f,1.5f,0.08f), new Vector3(0.10f,3.0f,0.12f), matGoldDull);

        // 奥壁の装飾帯（肖像画まわりのアーチ感を出す縁取り）
        CreateBox(room2Root, "R2_BackAccent_T", new Vector3(0f,4.5f,12.0f), new Vector3(10f,0.22f,0.12f), matGoldDull);
        CreateBox(room2Root, "R2_BackAccent_L", new Vector3(-4.6f,2.5f,12.0f), new Vector3(0.12f,5f,0.12f), matGoldDull);
        CreateBox(room2Root, "R2_BackAccent_R", new Vector3( 4.6f,2.5f,12.0f), new Vector3(0.12f,5f,0.12f), matGoldDull);
    }

    // ── 壁付き燭台（スコンス）──
    static void BuildWallSconces(GameObject room2Root, Material matGold, Material matCandle, Material matFlame)
    {
        // 左右壁、各2本（前・後）
        var sconcePositions = new (Vector3 pos, Vector3 dir, string tag)[]
        {
            (new Vector3(-4.85f,2.8f,3.5f),  new Vector3(0,90f,0),  "LL"),  // 左前
            (new Vector3(-4.85f,2.8f,9.5f),  new Vector3(0,90f,0),  "LR"),  // 左後
            (new Vector3( 4.85f,2.8f,3.5f),  new Vector3(0,270f,0), "RL"),  // 右前
            (new Vector3( 4.85f,2.8f,9.5f),  new Vector3(0,270f,0), "RR"),  // 右後
        };

        foreach (var (pos, dir, tag) in sconcePositions)
        {
            var sRoot = new GameObject($"R2_Sconce_{tag}");
            sRoot.transform.SetParent(room2Root.transform, false);
            sRoot.transform.localPosition = pos;
            sRoot.transform.localEulerAngles = dir;

            // 壁取付板
            CreateBox(sRoot, $"Sc_{tag}_Wall",   new Vector3(0f,0f,-0.04f), new Vector3(0.22f,0.32f,0.08f), matGold);
            // アーム（手前に伸びる）
            CreateBox(sRoot, $"Sc_{tag}_Arm",    new Vector3(0f,0f,0.12f),  new Vector3(0.05f,0.05f,0.24f), matGold);
            // カップ（ロウソク受け）
            CreateCylinder(sRoot,$"Sc_{tag}_Cup",new Vector3(0f,0f,0.26f),  new Vector3(0.12f,0.04f,0.12f), matGold);
            // ロウソク本体
            CreateCylinder(sRoot,$"Sc_{tag}_Stick",new Vector3(0f,0.10f,0.26f),new Vector3(0.07f,0.10f,0.07f),matCandle);
            // 炎
            var sc_flame = CreateSphere(sRoot,$"Sc_{tag}_Flame",new Vector3(0f,0.23f,0.26f),0.08f,matFlame);
            sc_flame.transform.localScale = new Vector3(0.07f,0.13f,0.07f);
            // 光源
            AddPointLight(sRoot,$"Sc_{tag}_Light",new Vector3(0f,0.25f,0.35f),
                new Color(1.0f,0.65f,0.20f), 1.2f, 4.5f);
        }
    }

    // ── ステンドグラス（大幅拡大・発光強化）──
    static void BuildStainedGlass(GameObject room2Root, Material matWood)
    {
        var sgRoot = new GameObject("R2_StainedGlassRoot");
        sgRoot.transform.SetParent(room2Root.transform, false);
        sgRoot.transform.localPosition = new Vector3(4.85f, 3.0f, 7f);
        sgRoot.transform.localEulerAngles = new Vector3(0, -90f, 0);

        // 重厚な石製額縁（外枠＋内枠のシンプル構造）
        var matStoneFrame = GetOrCreateMatURP("Mat_SG_StoneFrame", new Color(0.20f,0.16f,0.13f), 0f, 0.10f);
        CreateBox(sgRoot, "SG_FrameOuter",  Vector3.zero,           new Vector3(4.0f,2.4f,0.18f), matStoneFrame);
        CreateBox(sgRoot, "SG_FrameInner",  new Vector3(0,0,0.06f), new Vector3(3.7f,2.1f,0.10f), matWood);

        // 4連アーチ窓のテクスチャ画像を貼った Quad（肖像画パズルのヒント：左から指輪→剣→王冠→書物）
        var matWindow = GetOrCreateMatURP("Mat_R2_Window4Arches", Color.white, 0f, 0.1f);
        var windowTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Textures/Window_4Arches.png");
        if (windowTex != null)
        {
            var unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader != null) matWindow.shader = unlitShader;
            matWindow.mainTexture = windowTex;
            if (matWindow.HasProperty("_BaseMap"))   matWindow.SetTexture("_BaseMap", windowTex);
            if (matWindow.HasProperty("_MainTex"))   matWindow.SetTexture("_MainTex", windowTex);
            if (matWindow.HasProperty("_BaseColor")) matWindow.SetColor("_BaseColor", Color.white);
            if (matWindow.HasProperty("_Cull"))      matWindow.SetFloat("_Cull", 0f);
            matWindow.doubleSidedGI = true;
            EditorUtility.SetDirty(matWindow);
        }
        // Quad のデフォルト法線 +Z をカメラ側 (-X 親回転先) に向けるため Y180
        var sgCanvas = CreateQuad(sgRoot, "SG_Canvas", new Vector3(0, 0, 0.12f), new Vector3(3.6f, 2.0f, 1f), matWindow);
        sgCanvas.transform.localEulerAngles = new Vector3(0f, 180f, 0f);

        // クリックエリア（ズーム後にメモ表示）
        AddBoxCollider(sgCanvas, new Vector3(1f, 1f, 0.3f));
        var sgNote = sgCanvas.GetComponent<NoteInteractable>() ?? sgCanvas.AddComponent<NoteInteractable>();
        SetPrivate(sgNote, "noteData", noteStainedGlassPlaque);
        SetPrivate(sgNote, "requiredArea", RoomArea.StainedGlass);

        // Overview → StainedGlass クリックゾーン
        var sgZone = new GameObject("R2_ClickZone_StainedGlass");
        sgZone.transform.SetParent(room2Root.transform, false);
        sgZone.transform.localPosition = new Vector3(4.5f,2.8f,7f);
        AddBoxCollider(sgZone, new Vector3(0.5f,3.5f,3.0f));
        var sgCZ = sgZone.GetComponent<AreaClickZone>() ?? sgZone.AddComponent<AreaClickZone>();
        SetPrivate(sgCZ, "targetArea", RoomArea.StainedGlass);
    }

    // ── 食器棚（ディスプレイキャビネット）──
    static void BuildDisplayCabinet(GameObject room2Root, Material matDarkWood, Material matGold, Material matStone)
    {
        // 食器棚のガラスは透明な Mat_WindowGlass を使用（内部の食器が見えるように）
        var matGlass = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Generated/Mat_WindowGlass.mat")
                       ?? GetOrCreateMatURP("Mat_R2_Glass", new Color(0.35f,0.42f,0.55f), 0.85f, 0.20f);

        var cabRoot = new GameObject("R2_DisplayCabinetRoot");
        cabRoot.transform.SetParent(room2Root.transform, false);
        cabRoot.transform.localPosition = new Vector3(-4.1f, 0f, 7f);
        cabRoot.transform.localEulerAngles = new Vector3(0f, 90f, 0f); // 扉を部屋中心(+X)に向ける

        // 本体（内部空洞化のため Cab_Body は廃止、左右側板で外枠を構成）
        CreateBox(cabRoot,"Cab_SidePanelL", new Vector3(-1.05f,1.4f, 0f), new Vector3(0.10f,2.8f,0.70f), matDarkWood);
        CreateBox(cabRoot,"Cab_SidePanelR", new Vector3( 1.05f,1.4f, 0f), new Vector3(0.10f,2.8f,0.70f), matDarkWood);
        CreateBox(cabRoot,"Cab_TopPlank",  new Vector3(0f,2.85f,0f),    new Vector3(2.3f,0.14f,0.76f), matDarkWood);
        CreateBox(cabRoot,"Cab_BotPlank",  new Vector3(0f,0.06f,0f),    new Vector3(2.3f,0.14f,0.76f), matDarkWood);
        CreateBox(cabRoot,"Cab_BackPanel", new Vector3(0f,1.4f,-0.30f), new Vector3(2.2f,2.8f,0.10f), matDarkWood);

        // 棚板（3段）
        CreateBox(cabRoot,"Cab_ShelfUpper", new Vector3(0f,2.05f,0f), new Vector3(2.0f,0.06f,0.65f), matDarkWood);
        CreateBox(cabRoot,"Cab_ShelfMid",   new Vector3(0f,1.45f,0f), new Vector3(2.0f,0.06f,0.65f), matDarkWood);
        CreateBox(cabRoot,"Cab_ShelfLower", new Vector3(0f,0.85f,0f), new Vector3(2.0f,0.06f,0.65f), matDarkWood);

        // 上部装飾コーニス
        CreateBox(cabRoot,"Cab_Cornice",   new Vector3(0f,2.92f,0.05f), new Vector3(2.4f,0.10f,0.85f), matDarkWood);
        CreateBox(cabRoot,"Cab_CorniceTrim", new Vector3(0f,3.00f,0.05f), new Vector3(2.5f,0.04f,0.90f), matGold);

        // 脚（4本）
        var matLeg = GetOrCreateMatURP("Mat_R2_CabLeg", new Color(0.13f,0.07f,0.02f), 0f, 0.18f);
        CreateBox(cabRoot,"Cab_Leg_FL",  new Vector3(-0.95f,-0.25f, 0.30f), new Vector3(0.11f,0.50f,0.11f), matLeg);
        CreateBox(cabRoot,"Cab_Leg_FR",  new Vector3( 0.95f,-0.25f, 0.30f), new Vector3(0.11f,0.50f,0.11f), matLeg);
        CreateBox(cabRoot,"Cab_Leg_BL",  new Vector3(-0.95f,-0.25f,-0.30f), new Vector3(0.11f,0.50f,0.11f), matLeg);
        CreateBox(cabRoot,"Cab_Leg_BR",  new Vector3( 0.95f,-0.25f,-0.30f), new Vector3(0.11f,0.50f,0.11f), matLeg);

        // ガラス扉（上下2パネル）
        CreateBox(cabRoot,"Cab_GlassUpper", new Vector3(0f,2.18f,0.36f), new Vector3(1.8f,0.90f,0.05f), matGlass);
        CreateBox(cabRoot,"Cab_GlassLower", new Vector3(0f,1.10f,0.36f), new Vector3(1.8f,0.88f,0.05f), matGlass);

        // 金属フレーム
        CreateBox(cabRoot,"Cab_FrameTop",   new Vector3(0f,2.68f,0.37f),     new Vector3(1.90f,0.09f,0.07f), matGold);
        CreateBox(cabRoot,"Cab_FrameMid",   new Vector3(0f,1.63f,0.37f),     new Vector3(1.90f,0.09f,0.07f), matGold);
        CreateBox(cabRoot,"Cab_FrameBot",   new Vector3(0f,0.57f,0.37f),     new Vector3(1.90f,0.09f,0.07f), matGold);
        CreateBox(cabRoot,"Cab_FrameLeft",  new Vector3(-0.96f,1.63f,0.37f), new Vector3(0.09f,2.20f,0.07f), matGold);
        CreateBox(cabRoot,"Cab_FrameRight", new Vector3( 0.96f,1.63f,0.37f), new Vector3(0.09f,2.20f,0.07f), matGold);
        CreateBox(cabRoot,"Cab_FrameHMid",  new Vector3(0f,1.63f,0.37f),     new Vector3(0.09f,2.20f,0.07f), matGold);

        // 錠前（飾り）— 南京錠形状。新仕様ではクリック不可、扉脇の解錠装置で解く
        var cabLockGO = CreateBox(cabRoot,"Cab_Lock",
            new Vector3(0f,1.55f,0.42f), new Vector3(0.42f,0.50f,0.14f), matGold);
        // 旧 Collider と旧 Interactable が残っていれば除去（クリック不可にする）
        var oldCol = cabLockGO.GetComponent<BoxCollider>();
        if (oldCol != null) Object.DestroyImmediate(oldCol);
        if (cabLockGO.GetComponent<DisplayCabinetPuzzle>() == null)
            cabLockGO.AddComponent<DisplayCabinetPuzzle>();
        // シャックル（U字バー部分）
        CreateBox(cabRoot,"Cab_LockShackleTop",
            new Vector3(0f,1.86f,0.42f), new Vector3(0.30f,0.06f,0.10f), matGold);
        CreateBox(cabRoot,"Cab_LockShackleL",
            new Vector3(-0.13f,1.78f,0.42f), new Vector3(0.06f,0.20f,0.10f), matGold);
        CreateBox(cabRoot,"Cab_LockShackleR",
            new Vector3( 0.13f,1.78f,0.42f), new Vector3(0.06f,0.20f,0.10f), matGold);
        // 鍵穴（黒い縦穴）
        var matKeyhole = GetOrCreateMatURP("Mat_R2_Keyhole", new Color(0.02f,0.02f,0.02f), 0.4f, 0.15f);
        CreateBox(cabRoot,"Cab_Keyhole",
            new Vector3(0f,1.55f,0.50f), new Vector3(0.08f,0.20f,0.02f), matKeyhole);

        // 棚の中：Pandazole 食器プレハブで構成（上段=皿7・中段=カップ4・下段=ボウル2 → 正解 742）
        PlaceCabinetDishes(cabRoot);

        // 扉の右脇に解錠装置（3つのカウンタボタン + 確定/リセットレバー）
        BuildCabinetUnlockDevice(cabRoot, cabLockGO.GetComponent<DisplayCabinetPuzzle>(), matDarkWood, matGold);

        // 取っ手（凝った金色ノブ、扉中央）：ベース円盤+支柱+装飾球
        CreateCylinder(cabRoot,"Cab_KnobBase_U", new Vector3(0f,2.20f,0.42f), new Vector3(0.10f,0.02f,0.10f), matGold);
        CreateCylinder(cabRoot,"Cab_KnobStem_U", new Vector3(0f,2.20f,0.45f), new Vector3(0.025f,0.05f,0.025f), matGold);
        CreateSphere  (cabRoot,"Cab_Knob_U",     new Vector3(0f,2.20f,0.50f), 0.07f, matGold);
        CreateCylinder(cabRoot,"Cab_KnobBase_L", new Vector3(0f,1.10f,0.42f), new Vector3(0.10f,0.02f,0.10f), matGold);
        CreateCylinder(cabRoot,"Cab_KnobStem_L", new Vector3(0f,1.10f,0.45f), new Vector3(0.025f,0.05f,0.025f), matGold);
        CreateSphere  (cabRoot,"Cab_Knob_L",     new Vector3(0f,1.10f,0.50f), 0.07f, matGold);

        // 4角のコーナーオーナメント（金の装飾球）
        float[] cornerXs = { -0.96f, 0.96f };
        float[] cornerYs = {  2.68f, 0.57f };
        for (int cx = 0; cx < 2; cx++)
            for (int cy = 0; cy < 2; cy++)
                CreateSphere(cabRoot, $"Cab_CornerGem_{cx}{cy}",
                    new Vector3(cornerXs[cx], cornerYs[cy], 0.40f), 0.06f, matGold);

        // フレーム内側の彫刻ライン（細い金縁モール、上下のガラスパネル内側）
        // 上扉の内側
        CreateBox(cabRoot,"Cab_FrameInsetTop_U",  new Vector3(0f, 2.58f, 0.38f), new Vector3(1.78f, 0.03f, 0.04f), matGold);
        CreateBox(cabRoot,"Cab_FrameInsetBot_U",  new Vector3(0f, 1.73f, 0.38f), new Vector3(1.78f, 0.03f, 0.04f), matGold);
        CreateBox(cabRoot,"Cab_FrameInsetLeft_U", new Vector3(-0.90f, 2.155f, 0.38f), new Vector3(0.03f, 0.85f, 0.04f), matGold);
        CreateBox(cabRoot,"Cab_FrameInsetRight_U",new Vector3( 0.90f, 2.155f, 0.38f), new Vector3(0.03f, 0.85f, 0.04f), matGold);
        // 下扉の内側
        CreateBox(cabRoot,"Cab_FrameInsetTop_L",  new Vector3(0f, 1.53f, 0.38f), new Vector3(1.78f, 0.03f, 0.04f), matGold);
        CreateBox(cabRoot,"Cab_FrameInsetBot_L",  new Vector3(0f, 0.67f, 0.38f), new Vector3(1.78f, 0.03f, 0.04f), matGold);
        CreateBox(cabRoot,"Cab_FrameInsetLeft_L", new Vector3(-0.90f, 1.10f, 0.38f), new Vector3(0.03f, 0.86f, 0.04f), matGold);
        CreateBox(cabRoot,"Cab_FrameInsetRight_L",new Vector3( 0.90f, 1.10f, 0.38f), new Vector3(0.03f, 0.86f, 0.04f), matGold);

        // 錠前周辺の装飾彫刻
        CreateBox(cabRoot,"Cab_LockHaloT", new Vector3(0f, 1.85f, 0.41f), new Vector3(0.55f, 0.04f, 0.03f), matGold);
        CreateBox(cabRoot,"Cab_LockHaloB", new Vector3(0f, 1.25f, 0.41f), new Vector3(0.55f, 0.04f, 0.03f), matGold);

        // コーニス多層化（既存 Cornice 上下に追加レイヤー）
        CreateBox(cabRoot,"Cab_CorniceLayer1", new Vector3(0f, 3.04f, 0.05f), new Vector3(2.45f, 0.03f, 0.86f), matDarkWood);
        CreateBox(cabRoot,"Cab_CorniceLayer2", new Vector3(0f, 3.07f, 0.05f), new Vector3(2.55f, 0.02f, 0.92f), matGold);
        // コーニス中央の紋章（菱形風）
        CreateBox(cabRoot,"Cab_CrestPlate", new Vector3(0f, 2.97f, 0.46f), new Vector3(0.18f, 0.12f, 0.02f), matGold);
        CreateBox(cabRoot,"Cab_CrestGem",   new Vector3(0f, 2.97f, 0.48f), new Vector3(0.06f, 0.06f, 0.02f), matStone);

        // 食器棚内部のライト（食器をガラス越しに見えるように照らす）
        AddPointLight(cabRoot,"Cab_InnerLight_U", new Vector3(0f,2.30f,0.0f), new Color(1.0f,0.92f,0.75f), 0.15f, 1.5f);
        AddPointLight(cabRoot,"Cab_InnerLight_M", new Vector3(0f,1.70f,0.0f), new Color(1.0f,0.92f,0.75f), 0.10f, 1.3f);
        AddPointLight(cabRoot,"Cab_InnerLight_L", new Vector3(0f,1.10f,0.0f), new Color(1.0f,0.92f,0.75f), 0.10f, 1.3f);

        // Overview → DisplayCabinet クリックゾーン
        var cabZone = new GameObject("R2_ClickZone_Cabinet");
        cabZone.transform.SetParent(room2Root.transform, false);
        cabZone.transform.localPosition = new Vector3(-4.1f,1.5f,7f);
        AddBoxCollider(cabZone, new Vector3(1.2f,3.5f,1.8f));
        var cabCZ = cabZone.GetComponent<AreaClickZone>() ?? cabZone.AddComponent<AreaClickZone>();
        SetPrivate(cabCZ, "targetArea", RoomArea.DisplayCabinet);
    }

    // ── 食器棚内：Pandazole プレハブ配置（上段=皿7・中段=カップ4・下段=ボウル2）──
    static void PlaceCabinetDishes(GameObject cabRoot)
    {
        const string baseDir = "Assets/Pandazole_Ultimate_Pack/Pandazole Kitchen Food/Prefabs/";
        var platePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(baseDir + "Prop_Plate_01.prefab");
        var cupPrefab   = AssetDatabase.LoadAssetAtPath<GameObject>(baseDir + "Prop_Cup_01.prefab");
        var bowlPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(baseDir + "Prop_Bowel_01.prefab");
        if (platePrefab == null || cupPrefab == null || bowlPrefab == null)
        {
            Debug.LogError("[Room2Setup] Pandazole プレハブが見つかりません。Package を再インポートしてください");
            return;
        }

        // 上段（y≈2.10、Cab_ShelfUpper の上）：皿 7 枚を1列に並べる
        // 棚内寸 x: -0.90〜+0.90、step=0.27 でも余裕がある（皿スケール 0.4 想定）
        for (int i = 0; i < 7; i++)
        {
            float x = -0.84f + i * 0.28f;
            var plate = (GameObject)PrefabUtility.InstantiatePrefab(platePrefab, cabRoot.transform);
            plate.name = $"Cab_Dish_Plate_{i}";
            plate.transform.localPosition = new Vector3(x, 2.12f, -0.05f);
            plate.transform.localScale = Vector3.one * 0.4f;
        }

        // 中段（y≈1.52）：カップ 4 個を1列に並べる
        for (int i = 0; i < 4; i++)
        {
            float x = -0.60f + i * 0.40f;
            var cup = (GameObject)PrefabUtility.InstantiatePrefab(cupPrefab, cabRoot.transform);
            cup.name = $"Cab_Dish_Cup_{i}";
            cup.transform.localPosition = new Vector3(x, 1.52f, -0.05f);
            cup.transform.localScale = Vector3.one * 0.5f;
        }

        // 下段（y≈0.92）：ボウル 2 個を並べる
        for (int i = 0; i < 2; i++)
        {
            float x = -0.30f + i * 0.60f;
            var bowl = (GameObject)PrefabUtility.InstantiatePrefab(bowlPrefab, cabRoot.transform);
            bowl.name = $"Cab_Dish_Bowl_{i}";
            bowl.transform.localPosition = new Vector3(x, 0.92f, -0.05f);
            bowl.transform.localScale = Vector3.one * 0.6f;
        }
    }

    // ── 食器棚解錠装置：3 つのカウンタボタン + 確定 / リセットレバー ──
    static void BuildCabinetUnlockDevice(GameObject cabRoot, DisplayCabinetPuzzle puzzle, Material matDarkWood, Material matGold)
    {
        if (puzzle == null) { Debug.LogError("[Room2Setup] DisplayCabinetPuzzle が null"); return; }

        // 装置全体の親（食器棚の右側面、player の右側に張り出す位置）
        // cabRoot は Y=90 回転済み。local -X 方向が player の右側に対応
        var deviceRoot = new GameObject("Cab_UnlockDevice");
        deviceRoot.transform.SetParent(cabRoot.transform, false);
        deviceRoot.transform.localPosition = new Vector3(-1.25f, 1.40f, 0.30f);

        // 背板（縦長の真鍮プレート）
        CreateBox(deviceRoot, "UD_BackPlate", Vector3.zero, new Vector3(0.20f, 1.80f, 0.04f), matDarkWood);
        CreateBox(deviceRoot, "UD_GoldTrim", new Vector3(0f, 0f, 0.02f), new Vector3(0.16f, 1.74f, 0.01f), matGold);

        // 3 つのボタン（上段用 / 中段用 / 下段用）— 棚段の高さに合わせて配置
        // 食器棚棚段 y: 上段=2.10, 中段=1.52, 下段=0.92
        // deviceRoot の local y=1.40 を基準にすると相対 y: 上段=+0.70, 中段=+0.12, 下段=-0.48
        CreateCabinetButton(deviceRoot, "UD_Button_Top", new Vector3(0.04f,  0.70f, 0.04f), matGold,
            puzzle, CabinetCounterButton.CounterTarget.Top);
        CreateCabinetButton(deviceRoot, "UD_Button_Mid", new Vector3(0.04f,  0.12f, 0.04f), matGold,
            puzzle, CabinetCounterButton.CounterTarget.Mid);
        CreateCabinetButton(deviceRoot, "UD_Button_Bot", new Vector3(0.04f, -0.48f, 0.04f), matGold,
            puzzle, CabinetCounterButton.CounterTarget.Bot);

        // 確定 / リセットレバー（下部）
        CreateCabinetLever(deviceRoot, "UD_Lever_Submit", new Vector3(0.04f, -0.78f, 0.04f),
            new Color(0.20f, 0.55f, 0.15f), matGold, puzzle, CabinetLever.LeverMode.Submit);
        CreateCabinetLever(deviceRoot, "UD_Lever_Reset", new Vector3(0.04f, -0.86f, 0.04f),
            new Color(0.55f, 0.20f, 0.15f), matGold, puzzle, CabinetLever.LeverMode.Reset);
    }

    static void CreateCabinetButton(GameObject deviceRoot, string name, Vector3 localPos, Material matBtn,
        DisplayCabinetPuzzle puzzle, CabinetCounterButton.CounterTarget target)
    {
        // ボタン本体（小さな球）
        var btn = CreateSphere(deviceRoot, name, localPos, 0.06f, matBtn);
        AddBoxCollider(btn, new Vector3(2.5f, 2.5f, 2.5f)); // クリックしやすい大きめの当たり判定

        // ボタンの左側（プレート内側）にカウンタ表示
        var dispGO = new GameObject(name + "_Disp");
        dispGO.transform.SetParent(deviceRoot.transform, false);
        // ボタン位置から x = -0.10（プレート内側）、z = 0.045（プレート表面より少し前）
        dispGO.transform.localPosition = new Vector3(localPos.x - 0.10f, localPos.y, 0.045f);
        var tmp = dispGO.AddComponent<TextMeshPro>();
        tmp.text = "0";
        tmp.fontSize = 1.6f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1.0f, 0.95f, 0.80f);
        tmp.rectTransform.sizeDelta = new Vector2(0.10f, 0.10f);
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        // フォント自動適用（プロジェクトの日本語フォントが使われる）

        // スクリプト追加＋SerializedField 配線
        var ccb = btn.GetComponent<CabinetCounterButton>() ?? btn.AddComponent<CabinetCounterButton>();
        var so = new SerializedObject(ccb);
        so.FindProperty("target").enumValueIndex = (int)target;
        so.FindProperty("puzzle").objectReferenceValue = puzzle;
        so.FindProperty("displayText").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(ccb);
    }

    static void CreateCabinetLever(GameObject deviceRoot, string name, Vector3 localPos, Color color, Material matGold,
        DisplayCabinetPuzzle puzzle, CabinetLever.LeverMode mode)
    {
        // レバーの本体（横長の小さなバー）
        var matLever = GetOrCreateMatURP("Mat_R2_Lever_" + mode, color, 0.4f, 0.30f);
        var lever = CreateBox(deviceRoot, name, localPos, new Vector3(0.10f, 0.04f, 0.04f), matLever);
        AddBoxCollider(lever, new Vector3(1.8f, 2.0f, 2.0f));

        // スクリプト
        var cl = lever.GetComponent<CabinetLever>() ?? lever.AddComponent<CabinetLever>();
        var so = new SerializedObject(cl);
        so.FindProperty("mode").enumValueIndex = (int)mode;
        so.FindProperty("puzzle").objectReferenceValue = puzzle;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(cl);
    }

    // ── 燭台（カンデラブラ）── アンティーク装飾的なゴシック燭台
    static void BuildCandelabra(GameObject room2Root, Material matGold, Material matCandle, Material matFlame)
    {
        var candleRoot = new GameObject("R2_CandelabraRoot");
        candleRoot.transform.SetParent(room2Root.transform, false);
        candleRoot.transform.localPosition = new Vector3(0f, 0f, 4f);

        // ── 4 段ベース（くびれのある装飾的なベース）──
        CreateCylinder(candleRoot,"Cand_BaseBottom", new Vector3(0f,0.02f,0f), new Vector3(1.8f,0.04f,1.8f), matGold);
        CreateCylinder(candleRoot,"Cand_BaseMid",    new Vector3(0f,0.10f,0f), new Vector3(1.5f,0.06f,1.5f), matGold);
        CreateCylinder(candleRoot,"Cand_BaseTop",    new Vector3(0f,0.18f,0f), new Vector3(1.2f,0.08f,1.2f), matGold);
        CreateCylinder(candleRoot,"Cand_BaseRim",    new Vector3(0f,0.26f,0f), new Vector3(1.0f,0.04f,1.0f), matGold);
        CreateSphere  (candleRoot,"Cand_BaseGem",    new Vector3(0f,0.32f,0f), 0.10f, matGold);

        // ── 装飾彫刻柱（中央ポール、複数段とノード装飾）──
        CreateCylinder(candleRoot,"Cand_StemBase",   new Vector3(0f,0.42f,0f), new Vector3(0.22f,0.10f,0.22f), matGold);
        CreateCylinder(candleRoot,"Cand_Stem1",      new Vector3(0f,0.57f,0f), new Vector3(0.12f,0.20f,0.12f), matGold);
        CreateSphere  (candleRoot,"Cand_StemNode1",  new Vector3(0f,0.78f,0f), 0.16f, matGold);
        CreateCylinder(candleRoot,"Cand_Stem2",      new Vector3(0f,0.98f,0f), new Vector3(0.10f,0.20f,0.10f), matGold);
        CreateSphere  (candleRoot,"Cand_StemNode2",  new Vector3(0f,1.20f,0f), 0.14f, matGold);
        CreateCylinder(candleRoot,"Cand_Stem3",      new Vector3(0f,1.38f,0f), new Vector3(0.09f,0.16f,0.09f), matGold);

        // ── ハブ（アーム接続点、装飾的な 2 段球）──
        CreateSphere  (candleRoot,"Cand_HubBig",     new Vector3(0f,1.54f,0f), 0.22f, matGold);

        // ── 水平アーム（既存より太い） + 両端装飾キャップ ──
        var armGO = CreateCylinder(candleRoot,"Cand_Arm",
            new Vector3(0f,1.54f,0f), new Vector3(0.09f,1.30f,0.09f), matGold);
        armGO.transform.localEulerAngles = new Vector3(0f,0f,90f);
        // アームの両端に装飾ボール（先端キャップ）
        CreateSphere(candleRoot,"Cand_ArmCapL", new Vector3(-1.30f,1.54f,0f), 0.08f, matGold);
        CreateSphere(candleRoot,"Cand_ArmCapR", new Vector3( 1.30f,1.54f,0f), 0.08f, matGold);

        // ── 中央上の尖塔（ゴシック風） ──
        CreateCylinder(candleRoot,"Cand_Finial",    new Vector3(0f,1.78f,0f), new Vector3(0.05f,0.16f,0.05f), matGold);
        CreateSphere  (candleRoot,"Cand_FinialTop", new Vector3(0f,1.92f,0f), 0.06f, matGold);

        // ── 5 本のロウソクと装飾受け皿 ──
        float[] candleX = { -1.10f, -0.55f, 0f, 0.55f, 1.10f };
        var matWick = GetOrCreateMatURP("Mat_R2_Wick", new Color(0.1f,0.05f,0.02f), 0f, 0.15f);
        for (int i = 0; i < 5; i++)
        {
            float x = candleX[i];
            // 装飾受け皿（3 段で円錐風）
            CreateCylinder(candleRoot,$"Cand_DishBase_{i}", new Vector3(x,1.44f,0f), new Vector3(0.20f,0.03f,0.20f), matGold);
            CreateCylinder(candleRoot,$"Cand_DishMid_{i}",  new Vector3(x,1.49f,0f), new Vector3(0.14f,0.04f,0.14f), matGold);
            CreateCylinder(candleRoot,$"Cand_DishRim_{i}",  new Vector3(x,1.54f,0f), new Vector3(0.18f,0.02f,0.18f), matGold);

            // ロウソク本体
            CreateCylinder(candleRoot,$"Candle_{i}_Stick",
                new Vector3(x,1.76f,0f), new Vector3(0.08f,0.20f,0.08f), matCandle);

            // 蝋滴り（小さな球で表現）
            CreateSphere(candleRoot,$"Cand_Drip_{i}_1", new Vector3(x+0.05f,1.62f,0f), 0.03f, matCandle);
            CreateSphere(candleRoot,$"Cand_Drip_{i}_2", new Vector3(x-0.04f,1.65f,0f), 0.025f, matCandle);

            // 芯（黒い細い棒）
            CreateBox(candleRoot,$"Candle_{i}_Wick",
                new Vector3(x,1.93f,0f), new Vector3(0.012f,0.04f,0.012f), matWick);

            // 炎（2層構造 + 個別 Point Light で発光感を強化）
            // 親 GameObject: SetActive(false) で炎全体を消灯
            var flame = new GameObject($"Candle_{i}_Flame");
            flame.transform.SetParent(candleRoot.transform, false);
            flame.transform.localPosition = new Vector3(x, 2.00f, 0f);
            // 外側オレンジ（縦長楕円）
            var flameOuter = CreateSphere(flame,$"Flame_{i}_Outer",
                Vector3.zero, 0.10f, matFlame);
            flameOuter.transform.localScale = new Vector3(0.11f, 0.22f, 0.11f);
            // 内側コア（白っぽく超高輝度）
            var matFlameCore = GetOrCreateMatURP("Mat_R2_FlameCore", new Color(1.0f,0.95f,0.8f), 0f, 0f,
                new Color(12.0f, 8.0f, 2.5f));
            var flameCore = CreateSphere(flame,$"Flame_{i}_Core",
                new Vector3(0f, 0.015f, 0f), 0.06f, matFlameCore);
            flameCore.transform.localScale = new Vector3(0.05f, 0.13f, 0.05f);
            // 個別 Point Light（蝋燭周辺を照らす、温かいオレンジ光）
            var flameLightGO = new GameObject($"Flame_{i}_Light");
            flameLightGO.transform.SetParent(flame.transform, false);
            flameLightGO.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            var flameLight = flameLightGO.AddComponent<Light>();
            flameLight.type = LightType.Point;
            flameLight.color = new Color(1.0f, 0.6f, 0.15f);
            flameLight.intensity = 0.8f;
            flameLight.range = 1.6f;
            flame.SetActive(false); // 全消灯（パズルで点ける）

            // Collider + CandleInteractable
            AddBoxCollider_Go(GameObject.Find($"Candle_{i}_Stick"), new Vector3(0.30f,0.65f,0.30f));
            var stick = GameObject.Find($"Candle_{i}_Stick");
            if (stick != null)
            {
                var ci = stick.GetComponent<CandleInteractable>() ?? stick.AddComponent<CandleInteractable>();
                SetPrivate(ci, "candleIndex", i);
                SetPrivate(ci, "flameObject", flame);
            }
        }

        // 燭台の暖色ライト（点灯時の演出用、初期は弱め）
        AddPointLight(candleRoot,"Cand_Light", new Vector3(0f,2.5f,0f),
            new Color(1.0f,0.65f,0.15f), 2.5f, 7f);

        // CandelabraPuzzle
        var candPuzzleGO = new GameObject("CandelabraPuzzle");
        candPuzzleGO.transform.SetParent(room2Root.transform, false);
        if (candPuzzleGO.GetComponent<CandelabraPuzzle>() == null)
            candPuzzleGO.AddComponent<CandelabraPuzzle>();

        // Overview → Candelabra クリックゾーン（Y範囲を狭めて祭壇クリックを阻害しない）
        var candZone = new GameObject("R2_ClickZone_Candelabra");
        candZone.transform.SetParent(room2Root.transform, false);
        candZone.transform.localPosition = new Vector3(0f,1.0f,4f);
        AddBoxCollider(candZone, new Vector3(2.8f,1.6f,1.8f));
        var candCZ = candZone.GetComponent<AreaClickZone>() ?? candZone.AddComponent<AreaClickZone>();
        SetPrivate(candCZ, "targetArea", RoomArea.Candelabra);
    }

    // ── 肖像画（拡大・詳細化）──
    static void BuildPortrait(GameObject room2Root, Material matGold, Material matPortrait)
    {
        var portRoot = new GameObject("R2_PortraitRoot");
        portRoot.transform.SetParent(room2Root.transform, false);
        portRoot.transform.localPosition = new Vector3(0f, 3.2f, 9.95f);

        // 豪華な三重枠フレーム
        var matOuterFrame = GetOrCreateMatURP("Mat_Port_OuterFrame", new Color(0.55f,0.42f,0.05f), 0.9f, 0.85f);
        CreateBox(portRoot,"Port_FrameOuter",  Vector3.zero,              new Vector3(2.6f,3.4f,0.16f), matOuterFrame);
        CreateBox(portRoot,"Port_FrameMid",    new Vector3(0,0,0.04f),    new Vector3(2.3f,3.1f,0.10f), matGold);
        CreateBox(portRoot,"Port_FrameInner",  new Vector3(0,0,0.08f),    new Vector3(1.95f,2.75f,0.08f),
            GetOrCreateMatURP("Mat_R2_DarkWood",new Color(0.18f,0.10f,0.04f)));
        // キャンバス（外部画像 Portrait.png を貼る）
        const string portraitPath = "Assets/_Project/Textures/Portrait.png";
        AssetDatabase.ImportAsset(portraitPath, ImportAssetOptions.ForceUpdate);
        var portraitTex = AssetDatabase.LoadAssetAtPath<Texture2D>(portraitPath);
        if (portraitTex != null)
        {
            // ライティング無視で画像をそのまま表示するため URP Unlit に切替
            var unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader != null) matPortrait.shader = unlitShader;
            matPortrait.mainTexture = portraitTex;
            if (matPortrait.HasProperty("_BaseMap"))   matPortrait.SetTexture("_BaseMap", portraitTex);
            if (matPortrait.HasProperty("_MainTex"))   matPortrait.SetTexture("_MainTex", portraitTex);
            if (matPortrait.HasProperty("_BaseColor")) matPortrait.SetColor("_BaseColor", Color.white);
            matPortrait.color = Color.white;
            // 両面表示にして法線方向に関係なく見えるように
            if (matPortrait.HasProperty("_Cull")) matPortrait.SetFloat("_Cull", 0f);
            matPortrait.doubleSidedGI = true;
            EditorUtility.SetDirty(matPortrait);
            AssetDatabase.SaveAssetIfDirty(matPortrait);
            Debug.Log($"[Room2Setup] 肖像画テクスチャを Mat_R2_Portrait に適用 (Unlit): {portraitPath}");
        }
        else Debug.LogWarning($"[Room2Setup] Portrait.png が見つかりません: {portraitPath}");
        // Quad の確定値:
        // - localPosition (0, 0.1, -0.10): Frame3層(z=-0.08～+0.12)の手前に出して隠されない
        // - localEulerAngles (0, 180, 0): Quadのデフォルト法線+Zを-Z(カメラ側)に向ける
        // - localScale (2.6, 3.4, 1): FrameOuterサイズと一致でフレーム全面を覆う
        var portCanvas = CreateQuad(portRoot,"Port_Canvas", new Vector3(0,0.1f,-0.10f), new Vector3(2.6f,3.4f,1f), matPortrait);
        portCanvas.transform.localEulerAngles = new Vector3(0f, 180f, 0f);

        // 4紋章のクリック判定（透明Collider のみ、見た目は画像内に描かれている前提）
        // Quad は Y180 回転でテクスチャが左右反転して表示されるため、Symbol X は画像位置と逆になる。
        // Z=-0.14 で Quad(z=-0.10) より手前に出して Ray が確実に当たるようにする。
        string[] symbolNames = { "指輪","剣","王冠","書物" };
        Vector3[] symbolPositions = {
            new Vector3( 0.55f, 0.95f,-0.14f),  // 画像左上の指輪 → 画面右上
            new Vector3(-0.55f, 0.95f,-0.14f),  // 画像右上の剣   → 画面左上
            new Vector3( 0.55f,-0.75f,-0.14f),  // 画像左下の王冠 → 画面右下
            new Vector3(-0.55f,-0.75f,-0.14f),  // 画像右下の書物 → 画面左下
        };
        for (int i = 0; i < 4; i++)
        {
            var sym = new GameObject($"Port_Symbol_{i}_{symbolNames[i]}");
            sym.transform.SetParent(portRoot.transform, false);
            sym.transform.localPosition = symbolPositions[i];
            var col = sym.AddComponent<BoxCollider>();
            col.size = new Vector3(0.6f, 0.6f, 0.1f);
            var psi = sym.GetComponent<PortraitSymbolInteractable>() ?? sym.AddComponent<PortraitSymbolInteractable>();
            SetPrivate(psi, "symbolIndex", i);
        }

        // 四隅の装飾
        Vector3[] cornerPos = {
            new Vector3(-1.22f, 1.6f,0.14f), new Vector3( 1.22f, 1.6f,0.14f),
            new Vector3(-1.22f,-1.4f,0.14f), new Vector3( 1.22f,-1.4f,0.14f),
        };
        for (int i = 0; i < 4; i++)
        {
            CreateBox(portRoot,$"Port_Corner_{i}", cornerPos[i], new Vector3(0.16f,0.16f,0.07f), matOuterFrame);
            // 中央飾り（金の丸）
            CreateSphere(portRoot,$"Port_CornerGem_{i}", cornerPos[i] + new Vector3(0,0,0.04f), 0.06f, matGold);
        }

        // 肖像画上の照明（スポット風ライト）
        AddPointLight(portRoot,"Port_Light", new Vector3(0f,1.8f,0.6f),
            new Color(1.0f,0.88f,0.65f), 2.0f, 4.5f);

        // PortraitPuzzle GO
        var portPuzzleGO = new GameObject("PortraitPuzzle");
        portPuzzleGO.transform.SetParent(room2Root.transform, false);
        if (portPuzzleGO.GetComponent<PortraitPuzzle>() == null)
            portPuzzleGO.AddComponent<PortraitPuzzle>();

        // Overview → Portrait クリックゾーン
        var portZone = new GameObject("R2_ClickZone_Portrait");
        portZone.transform.SetParent(room2Root.transform, false);
        portZone.transform.localPosition = new Vector3(0f,3.2f,9.6f);
        AddBoxCollider(portZone, new Vector3(3.0f,3.5f,1.0f));
        var portCZ = portZone.GetComponent<AreaClickZone>() ?? portZone.AddComponent<AreaClickZone>();
        SetPrivate(portCZ, "targetArea", RoomArea.Portrait);
    }

    // ── 祭壇（壮大版）──
    static void BuildAltar(GameObject room2Root, Material matStone, Material matDarkStone, Material matGold, Material matCandle, Material matFlame)
    {
        var altarRoot = new GameObject("R2_AltarRoot");
        altarRoot.transform.SetParent(room2Root.transform, false);
        altarRoot.transform.localPosition = new Vector3(0f, 0f, 8f);

        // 4段の石段（広→狭）
        CreateBox(altarRoot,"Altar_Step1", new Vector3(0f,0.15f,0f), new Vector3(3.0f,0.30f,1.6f), matStone);
        CreateBox(altarRoot,"Altar_Step2", new Vector3(0f,0.45f,0f), new Vector3(2.6f,0.30f,1.4f), matStone);
        CreateBox(altarRoot,"Altar_Step3", new Vector3(0f,0.75f,0f), new Vector3(2.2f,0.30f,1.2f), matStone);
        CreateBox(altarRoot,"Altar_Step4", new Vector3(0f,1.05f,0f), new Vector3(1.9f,0.30f,1.0f), matStone);

        // 本体ブロック（重厚）
        CreateBox(altarRoot,"Altar_Base",     new Vector3(0f,1.60f,0f),  new Vector3(1.7f,1.10f,0.92f), matDarkStone);
        // 天板
        CreateBox(altarRoot,"Altar_Top",      new Vector3(0f,2.22f,0f),  new Vector3(1.88f,0.15f,1.00f), matStone);

        // 後部装飾壁（十字架の台）— 肖像画の視認性のため削除
        // CreateBox(altarRoot,"Altar_BackPanel",new Vector3(0f,2.8f,0.45f),new Vector3(1.1f,1.2f,0.10f), matDarkStone);
        // 装飾帯（金縁）— 肖像画の視認性のため削除
        // CreateBox(altarRoot,"Altar_BP_T",     new Vector3(0f,3.42f,0.45f),new Vector3(1.2f,0.08f,0.12f), matGold);
        // CreateBox(altarRoot,"Altar_BP_B",     new Vector3(0f,2.20f,0.45f),new Vector3(1.2f,0.08f,0.12f), matGold);
        // CreateBox(altarRoot,"Altar_BP_L",     new Vector3(-0.56f,2.80f,0.45f),new Vector3(0.08f,1.2f,0.12f), matGold);
        // CreateBox(altarRoot,"Altar_BP_R",     new Vector3( 0.56f,2.80f,0.45f),new Vector3(0.08f,1.2f,0.12f), matGold);

        // 十字架 — 肖像画の視認性のため削除
        // var matCrossGlow = GetOrCreateMatURP("Mat_AltarCrossGlow",new Color(1f,0.9f,0.3f),0.95f,0.92f,
        //     new Color(1.5f,1.2f,0.2f));
        // CreateBox(altarRoot,"Altar_Cross_V", new Vector3(0f,3.5f,0.50f),  new Vector3(0.12f,1.4f,0.07f), matGold);
        // CreateBox(altarRoot,"Altar_Cross_H", new Vector3(0f,3.80f,0.50f), new Vector3(0.72f,0.12f,0.07f), matGold);
        // CreateBox(altarRoot,"Altar_CrossGlow_V",new Vector3(0f,3.5f,0.54f),  new Vector3(0.05f,1.35f,0.03f), matCrossGlow);
        // CreateBox(altarRoot,"Altar_CrossGlow_H",new Vector3(0f,3.80f,0.54f), new Vector3(0.67f,0.05f,0.03f), matCrossGlow);

        // 祭壇の入力盤（クリッカブル）：金フレーム + 中央の暗い凹みで「ここに入力する」雰囲気
        // Z=-0.5 はカメラ側（AltarPoint=Z9.5 から見て手前）に配置
        var altarLockGO = CreateBox(altarRoot,"Altar_Lock",
            new Vector3(0f,1.7f,-0.5f), new Vector3(0.6f,0.5f,0.1f), matGold);
        AddBoxCollider(altarLockGO, new Vector3(1f,1f,1f));
        if (altarLockGO.GetComponent<AltarInteractable>() == null)
            altarLockGO.AddComponent<AltarInteractable>();
        if (altarLockGO.GetComponent<AltarPuzzle>() == null)
            altarLockGO.AddComponent<AltarPuzzle>();
        // 中央の暗い凹み（装飾のみ、クリック判定は親側）
        CreateBox(altarRoot,"Altar_LockInsert",
            new Vector3(0f,1.7f,-0.56f), new Vector3(0.45f,0.35f,0.04f), matDarkStone);

        // 天板上のロウソク（左右各2本）
        float[] candleXs = { -0.68f, -0.35f, 0.35f, 0.68f };
        for (int i = 0; i < 4; i++)
        {
            float x = candleXs[i];
            CreateCylinder(altarRoot,$"AltarCandle_{i}",
                new Vector3(x,2.38f,0.1f), new Vector3(0.06f,0.12f,0.06f), matCandle);
            var altFlame = CreateSphere(altarRoot,$"AltarFlame_{i}",
                new Vector3(x,2.57f,0.1f), 0.09f, matFlame);
            altFlame.transform.localScale = new Vector3(0.08f,0.14f,0.08f);
            altFlame.SetActive(true);
        }

        // 聖水鉢（左右）
        var matBasin = GetOrCreateMatURP("Mat_R2_Basin", new Color(0.65f,0.62f,0.58f), 0.3f, 0.5f);
        CreateCylinder(altarRoot,"Altar_BasinL",new Vector3(-0.7f,1.20f, 0.3f),new Vector3(0.22f,0.10f,0.22f),matBasin);
        CreateCylinder(altarRoot,"Altar_BasinR",new Vector3( 0.7f,1.20f, 0.3f),new Vector3(0.22f,0.10f,0.22f),matBasin);

        // 祭壇ライト（十字架の発光＋神秘的な青白光）
        AddPointLight(altarRoot,"Altar_Light",     new Vector3(0f,4.2f,0.0f),
            new Color(0.55f,0.68f,1.00f), 2.0f, 6.5f);
        // Altar_CrossLight も十字架と一緒に削除
        // AddPointLight(altarRoot,"Altar_CrossLight", new Vector3(0f,3.5f,0.7f),
        //     new Color(1.0f,0.95f,0.70f), 1.2f, 3.5f);

        // Overview → Altar クリックゾーン
        // Y範囲を祭壇本体の高さに合わせて狭め、肖像画クリック範囲との重なりを最小化
        var altZone = new GameObject("R2_ClickZone_Altar");
        altZone.transform.SetParent(room2Root.transform, false);
        altZone.transform.localPosition = new Vector3(0f,1.2f,8f);
        AddBoxCollider(altZone, new Vector3(2.5f,2.2f,2.2f));
        var altCZ = altZone.GetComponent<AreaClickZone>() ?? altZone.AddComponent<AreaClickZone>();
        SetPrivate(altCZ, "targetArea", RoomArea.Altar);
        SetPrivate(altCZ, "requiredFlag", Flags.PortraitSolved);
        SetPrivate(altCZ, "blockedMessage", "祭壇には封印が施されている。\nまだ近づかない方が良さそうだ。");
    }

    // ─────────────────────────────────────────
    // 4. Room1追加要素
    // ─────────────────────────────────────────
    static void CreateRoom1Additions()
    {
        // 隠し本（本棚エリア）
        var hiddenBook = CreateBox(null, "HiddenBook",
            new Vector3(-3.2f, 1.4f, 5.8f), new Vector3(0.08f, 0.20f, 0.15f),
            GetOrCreateMatURP("Mat_HiddenBook", new Color(0.6f,0.4f,0.1f)));
        AddBoxCollider(hiddenBook, Vector3.one);
        var hbi = hiddenBook.GetComponent<HiddenBookInteractable>() ?? hiddenBook.AddComponent<HiddenBookInteractable>();
        SetPrivate(hbi, "hiddenBookNote", noteHiddenBook);
        SetPrivate(hbi, "bookVisual", hiddenBook);

        // 時計台座ヒント
        var clockHintGO = CreateBox(null, "ClockBaseHint",
            new Vector3(1.5f, 0.85f, 6.9f), new Vector3(0.4f, 0.05f, 0.3f),
            GetOrCreateMatURP("Mat_ClockBase", new Color(0.25f,0.15f,0.05f)));
        AddBoxCollider(clockHintGO, Vector3.one);
        var cni = clockHintGO.GetComponent<NoteInteractable>() ?? clockHintGO.AddComponent<NoteInteractable>();
        SetPrivate(cni, "noteData", noteClockHint);
        SetPrivate(cni, "requiredArea", RoomArea.Desk);

        // Room1扉
        var door = CreateBox(null, "Room1Door",
            new Vector3(0f, 1.5f, 5.85f), new Vector3(1.42f, 3.0f, 0.05f),
            GetOrCreateMatURP("Mat_Room1Door", new Color(0.20f,0.12f,0.05f), 0f, 0.22f));
        if (door.GetComponent<BoxCollider>() == null) door.AddComponent<BoxCollider>();
        if (door.GetComponent<Room1DoorInteractable>() == null)
            door.AddComponent<Room1DoorInteractable>();
        if (door.GetComponent<HoverHighlight>() == null)
            door.AddComponent<HoverHighlight>();
        door.SetActive(true);

        // 扉ノブ
        CreateBox(door,"Door_Knob",
            new Vector3(0.55f,0f,0.5f), new Vector3(0.05f,0.12f,0.04f),
            GetOrCreateMatURP("Mat_DoorKnob",new Color(0.85f,0.68f,0.12f),0.9f,0.82f));

        // Room1ClearManagerにroomKeyItemをセット
        var clearMgr = Object.FindAnyObjectByType<Room1ClearManager>();
        if (clearMgr != null)
            SetPrivate(clearMgr, "roomKeyItem", itemRoomKey);
        else
            Debug.LogWarning("[Room2Setup] Room1ClearManagerが見つかりません");

        Debug.Log("[Room2Setup] Room1追加要素完了");
    }

    // ─────────────────────────────────────────
    // 5. WireRoom1Clear
    // ─────────────────────────────────────────
    static void WireRoom1Clear()
    {
        var phone = Object.FindAnyObjectByType<PhoneRepairPuzzle>();
        if (phone != null)
        {
            phone.OnGameClear.RemoveAllListeners();
            EditorUtility.SetDirty(phone);
        }
        Debug.Log("[Room2Setup] WireRoom1Clear完了");
    }

    // ─────────────────────────────────────────
    // 6. Room2パズルコンポーネント wire
    // ─────────────────────────────────────────
    static void WireRoom2UIs()
    {
        var cabinet = Object.FindAnyObjectByType<DisplayCabinetPuzzle>();
        if (cabinet == null)
        {
            var cabLock = GameObject.Find("Cab_Lock");
            if (cabLock != null)
                cabinet = cabLock.GetComponent<DisplayCabinetPuzzle>() ?? cabLock.AddComponent<DisplayCabinetPuzzle>();
            else Debug.LogWarning("[Room2Setup] Cab_Lockが見つかりません");
        }
        if (cabinet != null) { SetPrivate(cabinet, "cabinetHintNote", noteCabinetHint); EditorUtility.SetDirty(cabinet); }

        var altar = Object.FindAnyObjectByType<AltarPuzzle>();
        if (altar == null)
        {
            var altarLock = GameObject.Find("Altar_Lock");
            if (altarLock != null)
                altar = altarLock.GetComponent<AltarPuzzle>() ?? altarLock.AddComponent<AltarPuzzle>();
            else Debug.LogWarning("[Room2Setup] Altar_Lockが見つかりません");
        }
        if (altar != null) EditorUtility.SetDirty(altar);

        var portrait = Object.FindAnyObjectByType<PortraitPuzzle>();
        if (portrait == null)
        {
            var portGO = EnsureGO("PortraitPuzzle", null);
            portrait = portGO.GetComponent<PortraitPuzzle>() ?? portGO.AddComponent<PortraitPuzzle>();
        }
        SetPrivate(portrait, "portraitSecretNote", notePortraitSecret);
        EditorUtility.SetDirty(portrait);

        var saveMgr = Object.FindAnyObjectByType<SaveManager>();
        if (saveMgr != null && itemRoomKey != null)
        {
            var saveSO = new SerializedObject(saveMgr);
            var itemsProp = saveSO.FindProperty("allItems");
            bool hasKey = false;
            for (int i = 0; i < itemsProp.arraySize; i++)
                if (itemsProp.GetArrayElementAtIndex(i).objectReferenceValue == itemRoomKey) { hasKey = true; break; }
            if (!hasKey)
            {
                itemsProp.InsertArrayElementAtIndex(itemsProp.arraySize);
                itemsProp.GetArrayElementAtIndex(itemsProp.arraySize - 1).objectReferenceValue = itemRoomKey;
                saveSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(saveMgr);
                Debug.Log("[Room2Setup] SaveManager: RoomKey追加");
            }
        }
        else if (saveMgr == null) Debug.LogWarning("[Room2Setup] SaveManagerが見つかりません");

        Debug.Log("[Room2Setup] Room2 UI wire完了");
    }

    // ─────────────────────────────────────────
    // ヘルパー
    // ─────────────────────────────────────────
    static GameObject EnsureGO(string name, Transform parent)
    {
        var go = GameObject.Find(name);
        if (go == null) { go = new GameObject(name); if (parent != null) go.transform.SetParent(parent); }
        return go;
    }

    static GameObject CreateBox(GameObject parent, string name, Vector3 localPos, Vector3 size, Material mat)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
        }
        if (parent != null) go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = size;
        var mr = go.GetComponent<MeshRenderer>();
        if (mr == null) mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        return go;
    }

    static GameObject CreateCylinder(GameObject parent, string name, Vector3 localPos, Vector3 scale, Material mat)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
        }
        if (parent != null) go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        var mr = go.GetComponent<MeshRenderer>();
        if (mr == null) mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        return go;
    }

    static GameObject CreateSphere(GameObject parent, string name, Vector3 localPos, float diameter, Material mat)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
        }
        if (parent != null) go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * diameter;
        var mr = go.GetComponent<MeshRenderer>();
        if (mr == null) mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        return go;
    }

    static GameObject CreateQuad(GameObject parent, string name, Vector3 localPos, Vector3 scale, Material mat)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
        }
        if (parent != null) go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        var mr = go.GetComponent<MeshRenderer>();
        if (mr == null) mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        return go;
    }

    static void AddBoxCollider(GameObject go, Vector3 size)
    {
        var col = go.GetComponent<BoxCollider>();
        if (col == null) col = go.AddComponent<BoxCollider>();
        if (col != null) col.size = size;
    }

    static void AddBoxCollider_Go(GameObject go, Vector3 size)
    {
        if (go == null) return;
        var col = go.GetComponent<BoxCollider>();
        if (col == null) col = go.AddComponent<BoxCollider>();
        if (col != null) col.size = size;
    }

    static void AddPointLight(GameObject parent, string name, Vector3 localPos, Color color, float intensity, float range)
    {
        var existing = GameObject.Find(name);
        if (existing != null) Object.DestroyImmediate(existing);

        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;

        var light = go.AddComponent<Light>();
        if (light == null) { Debug.LogWarning($"[Room2Setup] Light追加失敗: {name}"); return; }
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
    }

    static Material GetOrCreateMatURP(string name, Color color,
        float metallic = 0f, float smoothness = 0.3f, Color? emission = null)
    {
        string path = $"Assets/_Project/Materials/{name}.mat";
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "../Assets/_Project/Materials"));
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.name = name;
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        if (mat.HasProperty("_BaseColor"))  mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (emission.HasValue)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", emission.Value);
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material GetOrCreateMat(string name, Color color)
        => GetOrCreateMatURP(name, color);

    // ─────────────────────────────────────────
    // 7. Room2 Canvas UIパネル作成
    // ─────────────────────────────────────────
    static void CreateRoom2UIPanels()
    {
        var canvas = GameObject.Find("Canvas_Main");
        if (canvas == null) { Debug.LogError("[Room2Setup] Canvas_Mainが見つかりません"); return; }
        var managers = GameObject.Find("Managers");
        if (managers == null) { Debug.LogError("[Room2Setup] Managersが見つかりません"); return; }

        // 食器棚はカウンタ式に変更されたため、テンキーパネルは作らない
        // 旧 DisplayCabinetPanel が残っていれば消す
        var oldCabPanel = GameObject.Find("DisplayCabinetPanel");
        if (oldCabPanel != null) Object.DestroyImmediate(oldCabPanel);

        var altarUI = managers.GetComponent<AltarUI>() ?? managers.AddComponent<AltarUI>();
        CreateCodePadPanel(canvas, "AltarPanel", "祭壇の封印", altarUI);

        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(managers);
        Debug.Log("[Room2Setup] Room2 UIパネル作成完了");
    }

    static void CreateCodePadPanel(GameObject canvas, string panelName, string titleText, MonoBehaviour uiComp)
    {
        var existing = GameObject.Find(panelName);
        if (existing != null) Object.DestroyImmediate(existing);

        var panel = CreateUIObj(panelName, canvas, 320, 440);
        SetUIAnchorCenter(panel);
        AddUIImage(panel, new Color(0.1f,0.1f,0.1f,0.92f));

        var titleGO = CreateUIObj("TitleText", panel, 280, 36);
        SetUIAnchorTop(titleGO, 30f);
        var title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text = titleText;
        title.fontSize = 16;
        title.color = new Color(1f,0.9f,0.6f);
        title.alignment = TextAlignmentOptions.Center;

        var displayGO = CreateUIObj("CodeDisplay", panel, 280, 60);
        SetUIAnchorTop(displayGO, 130f);
        var display = displayGO.AddComponent<TextMeshProUGUI>();
        display.text = "—  —  —  —";
        display.fontSize = 36;
        display.color = Color.white;
        display.alignment = TextAlignmentOptions.Center;

        var grid = CreateUIObj("ButtonGrid", panel, 300, 240);
        SetUIAnchorBottom(grid, 20f);
        var layout = grid.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(88,54);
        layout.spacing = new Vector2(8,8);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;
        layout.childAlignment = TextAnchor.UpperCenter;

        string[] labels = { "7","8","9","4","5","6","1","2","3","CLR","0","" };
        foreach (var lbl in labels)
        {
            if (lbl == "") { CreateUIObj("Spacer", grid, 88, 54); continue; }
            CreateRoom2DigitButton(grid, lbl);
        }

        var so = new SerializedObject(uiComp);
        so.FindProperty("panel").objectReferenceValue       = panel;
        so.FindProperty("codeDisplay").objectReferenceValue = display;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(uiComp);

        panel.SetActive(false);
    }

    static void CreateRoom2DigitButton(GameObject parent, string label)
    {
        var go = CreateUIObj("Btn_" + label, parent, 88, 54);
        AddUIImage(go, new Color(0.25f,0.25f,0.3f,1f));
        if (go.GetComponent<Button>() == null) go.AddComponent<Button>();

        var textGO = CreateUIObj("Text", go, 88, 54);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = label == "CLR" ? 18 : 26;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var btn = go.GetComponent<EscapeGame.Game.Room2CodeButton>() ?? go.AddComponent<EscapeGame.Game.Room2CodeButton>();
        var so = new SerializedObject(btn);
        so.FindProperty("digit").stringValue = label;
        so.FindProperty("isClear").boolValue = (label == "CLR");
        so.ApplyModifiedProperties();
    }

    static GameObject CreateUIObj(string name, GameObject parent, float w, float h)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(w, h);
        return go;
    }

    static void SetUIAnchorCenter(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = Vector2.zero;
    }

    static void SetUIAnchorTop(GameObject go, float offsetY)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f,1f);
        rt.anchoredPosition = new Vector2(0,-offsetY);
    }

    static void SetUIAnchorBottom(GameObject go, float offsetY)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0f);
        rt.anchoredPosition = new Vector2(0,offsetY);
    }

    static void AddUIImage(GameObject go, Color color)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = color;
    }
}
#endif
