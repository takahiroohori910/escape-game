#if UNITY_EDITOR
using EscapeGame.Core;
using EscapeGame.Game;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public class Phase2BackSetup
{
    [MenuItem("EscapeGame/Setup/Phase2 Back Setup")]
    public static void Run()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("[P2Back] Edit mode で実行してください（Play mode 中は変更が保存されません）");
            return;
        }
        var canvas = GameObject.Find("Canvas_Main");
        if (canvas == null) { Debug.LogError("[P2Back] Canvas_Main が見つかりません"); return; }
        var managers = GameObject.Find("Managers");
        if (managers == null) { Debug.LogError("[P2Back] Managers が見つかりません"); return; }

        FixCanvasScaler(canvas);
        var jpFont = RecreateFontAsset();
        SetupAudioManager(managers);
        SetupTitleUI(canvas, managers);
        SetupClearUI(canvas, managers);   // ClearOverlay を TitleOverlay 削除後に再作成
        SetupTimerUI(canvas, managers);
        SetupSaveManager(managers);
        ResetPuzzleData();
        WireClearTimeText(managers);
        SetupHintButton(canvas, managers);
        SetupClockObject();
        SetupFireplacePhotoObject();
        AddFontWarmup(managers);
        ApplyJpFont(jpFont);

        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(managers);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[P2Back] Phase2後半セットアップ完了");
    }

    // ── フォントアセットをソースから再作成 ───────────────────────────────
    private static TMP_FontAsset RecreateFontAsset()
    {
        const string fontPath  = "Assets/_Project/Fonts/JapaneseFont.ttc";
        const string assetPath = "Assets/_Project/Fonts/NotoSansJP_Dynamic.asset";

        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (sourceFont == null) { Debug.LogWarning("[P2Back] JapaneseFont.ttc が見つかりません"); return null; }

        AssetDatabase.DeleteAsset(assetPath);

        var newAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic);
        newAsset.name = "NotoSansJP_Dynamic";

        // CreateAsset より前にテクスチャ参照を確保（保存後は参照が切れるため）
        Texture2D atlasTexture = (newAsset.atlasTextures != null && newAsset.atlasTextures.Length > 0)
            ? newAsset.atlasTextures[0] : null;

        if (atlasTexture == null)
        {
            atlasTexture = new Texture2D(2048, 2048, TextureFormat.Alpha8, false, true);
            atlasTexture.SetPixels32(new Color32[2048 * 2048]);
            atlasTexture.Apply(false, false);
            newAsset.atlasTextures = new Texture2D[] { atlasTexture };
        }
        atlasTexture.name = "NotoSansJP_Dynamic Atlas";

        // メインアセット保存 → サブアセット追加（Refresh を挟まない）
        AssetDatabase.CreateAsset(newAsset, assetPath);
        AssetDatabase.AddObjectToAsset(atlasTexture, assetPath);
        // マテリアルもサブアセットとして保存
        if (newAsset.material != null)
        {
            newAsset.material.name = "NotoSansJP_Dynamic Material";
            AssetDatabase.AddObjectToAsset(newAsset.material, assetPath);
        }
        EditorUtility.SetDirty(newAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[P2Back] NotoSansJP_Dynamic.asset 再作成完了（atlas + material sub-asset 付き）");
        return newAsset;
    }

    // ── パズルの初期値をコードの正しい値にリセット ──────────────────────
    private static void ResetPuzzleData()
    {
        // BookshelfPuzzle: 色インデックス 0=赤 1=青 2=緑、正解は青→赤→緑
        var bookshelf = Object.FindAnyObjectByType<BookshelfPuzzle>();
        if (bookshelf != null)
        {
            var so = new SerializedObject(bookshelf);
            var correct = so.FindProperty("correctOrder");
            correct.arraySize = 3;
            correct.GetArrayElementAtIndex(0).intValue = 1;
            correct.GetArrayElementAtIndex(1).intValue = 0;
            correct.GetArrayElementAtIndex(2).intValue = 2;
            var current = so.FindProperty("currentOrder");
            current.arraySize = 3;
            current.GetArrayElementAtIndex(0).intValue = 0;
            current.GetArrayElementAtIndex(1).intValue = 1;
            current.GetArrayElementAtIndex(2).intValue = 2;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(bookshelf);
            Debug.Log("[P2Back] BookshelfPuzzle リセット完了");
        }

        // DeskPuzzle: 4桁コード 1130（壁掛け時計 11:30 から）
        var desk = Object.FindAnyObjectByType<DeskPuzzle>();
        if (desk != null)
        {
            var so = new SerializedObject(desk);
            so.FindProperty("correctCode").stringValue = "1130";
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(desk);
            Debug.Log("[P2Back] DeskPuzzle correctCode=1130 設定完了");
        }
    }

    // ── ヒントボタン（右上「？」）と HintPanel を配線 ──────────────────
    private static void SetupHintButton(GameObject canvas, GameObject managers)
    {
        // 既存の HintButton を削除して再作成
        var existingBtn = GameObject.Find("HintButton");
        if (existingBtn != null) Object.DestroyImmediate(existingBtn);

        // 「？」ボタン（右上）
        var hintBtn = CreateButton("HintButton", canvas, "？", Vector2.zero, new Vector2(60, 60));
        var btnRt = hintBtn.GetComponent<RectTransform>();
        if (btnRt == null) btnRt = hintBtn.gameObject.AddComponent<RectTransform>();
        btnRt.anchorMin = btnRt.anchorMax = new Vector2(1f, 1f);
        btnRt.anchoredPosition = new Vector2(-80, -30);

        // HintUI に配線
        var hintUI = managers.GetComponent<HintUI>();
        if (hintUI != null)
        {
            hintBtn.onClick.AddListener(hintUI.Toggle);

            // HintPanel の panel フィールドを確認・配線
            var hintPanelGO = GameObject.Find("HintPanel");
            if (hintPanelGO != null)
            {
                var so = new SerializedObject(hintUI);
                so.FindProperty("panel").objectReferenceValue = hintPanelGO;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(hintUI);
            }
        }

        EditorUtility.SetDirty(canvas);
        Debug.Log("[P2Back] HintButton 追加完了");
    }

    // ── FontWarmup コンポーネント追加 ────────────────────────────────
    private static void AddFontWarmup(GameObject managers)
    {
        if (managers.GetComponent<EscapeGame.Core.FontWarmup>() == null)
            managers.AddComponent<EscapeGame.Core.FontWarmup>();
        Debug.Log("[P2Back] FontWarmup 追加");
    }

    // ── CanvasScaler 修正（PC 1280×720 基準）────────────────────────
    private static void FixCanvasScaler(GameObject canvas)
    {
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) return;
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        EditorUtility.SetDirty(canvas);
        Debug.Log("[P2Back] CanvasScaler を 1280×720 に修正");
    }

    // ── AudioManager ──────────────────────────────────────────────
    private static void SetupAudioManager(GameObject managers)
    {
        var am = managers.GetComponent<AudioManager>() ?? managers.AddComponent<AudioManager>();
        var so = new SerializedObject(am);

        AssignClip(so, "bgmMain",      "Assets/_Project/Audio/BGM/BGM_Main");
        AssignClip(so, "bgmClear",     "Assets/_Project/Audio/BGM/BGM_Clear");
        AssignClip(so, "seClick",      "Assets/_Project/Audio/SE/SE_Click");
        AssignClip(so, "seHover",      "Assets/_Project/Audio/SE/SE_Hover");
        AssignClip(so, "seBookMove",   "Assets/_Project/Audio/SE/SE_BookMove");
        AssignClip(so, "sePuzzleSolve","Assets/_Project/Audio/SE/SE_PuzzleSolve");
        AssignClip(so, "sePuzzleFail", "Assets/_Project/Audio/SE/SE_PuzzleFail");
        AssignClip(so, "seItemPickup", "Assets/_Project/Audio/SE/SE_ItemPickup");
        AssignClip(so, "seCameraMove", "Assets/_Project/Audio/SE/SE_CameraMove");
        AssignClip(so, "seNoteOpen",   "Assets/_Project/Audio/SE/SE_NoteOpen");
        AssignClip(so, "sePhoneRepair","Assets/_Project/Audio/SE/SE_PhoneRepair");
        AssignClip(so, "sePhoneCall",  "Assets/_Project/Audio/SE/SE_PhoneCall");

        so.ApplyModifiedProperties();
        Debug.Log("[P2Back] AudioManager: クリップ自動割り当て完了");
    }

    private static void AssignClip(SerializedObject so, string fieldName, string pathWithoutExt)
    {
        string[] exts = { ".wav", ".mp3", ".ogg", ".aif" };
        foreach (var ext in exts)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(pathWithoutExt + ext);
            if (clip != null)
            {
                so.FindProperty(fieldName).objectReferenceValue = clip;
                return;
            }
        }
        // ファイルが見つからない場合は既存の参照を保持（上書きしない）
    }

    // ── タイトルUI ────────────────────────────────────────────────
    private static void SetupTitleUI(GameObject canvas, GameObject managers)
    {
        var existing = GameObject.Find("TitleOverlay");
        if (existing != null) Object.DestroyImmediate(existing);

        // 全画面オーバーレイ
        var overlay = CreateUIObj("TitleOverlay", canvas, 0, 0);
        var rt = overlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        overlay.AddComponent<Image>().color = new Color(0.04f, 0.03f, 0.08f, 0.97f);

        // タイトル
        var titleGO = CreateUIObj("TitleText", overlay, 700, 110);
        SetAnchorCenter(titleGO, new Vector2(0, 100));
        var titleTmp = titleGO.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "嵐の洋館";
        titleTmp.fontSize = 80;
        titleTmp.color = new Color(0.95f, 0.88f, 0.65f);
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.fontStyle = FontStyles.Bold;

        // サブタイトル
        var subGO = CreateUIObj("SubTitle", overlay, 700, 40);
        SetAnchorCenter(subGO, new Vector2(0, 30));
        var subTmp = subGO.AddComponent<TextMeshProUGUI>();
        subTmp.text = "── 脱出せよ、嵐が夜明けを隠す前に ──";
        subTmp.fontSize = 22;
        subTmp.color = new Color(0.6f, 0.55f, 0.45f);
        subTmp.alignment = TextAlignmentOptions.Center;

        // ゲーム開始ボタン
        var startBtn = CreateButton("StartButton", overlay, "ゲーム開始",
            new Vector2(0, -80), new Vector2(400, 80));

        // 続きからボタン
        var continueBtn = CreateButton("ContinueButton", overlay, "続きから",
            new Vector2(0, -180), new Vector2(340, 66));

        // データ消去ボタン
        var clearBtn = CreateButton("ClearSaveButton", overlay, "データ消去",
            new Vector2(0, -266), new Vector2(280, 56), new Color(0.5f, 0.15f, 0.15f));

        // バージョンテキスト
        var verGO = CreateUIObj("VersionText", overlay, 160, 30);
        var verRt = verGO.GetComponent<RectTransform>();
        verRt.anchorMin = verRt.anchorMax = new Vector2(1f, 0f);
        verRt.anchoredPosition = new Vector2(-16, 16);
        var verTmp = verGO.AddComponent<TextMeshProUGUI>();
        verTmp.fontSize = 18;
        verTmp.color = new Color(0.4f, 0.4f, 0.4f);
        verTmp.alignment = TextAlignmentOptions.Right;

        var ui = managers.GetComponent<TitleUI>() ?? managers.AddComponent<TitleUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("overlay").objectReferenceValue       = overlay;
        so.FindProperty("startButton").objectReferenceValue   = startBtn;
        so.FindProperty("continueButton").objectReferenceValue= continueBtn;
        so.FindProperty("clearSaveButton").objectReferenceValue=clearBtn;
        so.FindProperty("versionText").objectReferenceValue   = verTmp;
        so.ApplyModifiedProperties();
    }

    // ── タイマーUI ────────────────────────────────────────────────
    private static void SetupTimerUI(GameObject canvas, GameObject managers)
    {
        var existing = GameObject.Find("TimerText");
        if (existing != null) Object.DestroyImmediate(existing);

        // ゲーム中タイマー（右上）
        var timerGO = CreateUIObj("TimerText", canvas, 200, 48);
        var rt = timerGO.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-20, -30);
        var timerTmp = timerGO.AddComponent<TextMeshProUGUI>();
        timerTmp.fontSize = 36;
        timerTmp.color = new Color(1f, 1f, 0.9f, 1f);
        timerTmp.alignment = TextAlignmentOptions.Right;

        // クリア画面タイム表示（ClearOverlay内）
        var clearOverlay = GameObject.Find("ClearOverlay");
        TextMeshProUGUI clearTimeTmp = null;
        if (clearOverlay != null)
        {
            var existingCT = clearOverlay.transform.Find("ClearTimeText");
            if (existingCT != null) Object.DestroyImmediate(existingCT.gameObject);
            var ctGO = CreateUIObj("ClearTimeText", clearOverlay, 400, 36);
            SetAnchorCenter(ctGO, new Vector2(0, -60));
            clearTimeTmp = ctGO.AddComponent<TextMeshProUGUI>();
            clearTimeTmp.fontSize = 16;
            clearTimeTmp.color = new Color(0.7f, 0.9f, 0.7f);
            clearTimeTmp.alignment = TextAlignmentOptions.Center;
        }

        var ui = managers.GetComponent<TimerUI>() ?? managers.AddComponent<TimerUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("timerText").objectReferenceValue     = timerTmp;
        so.FindProperty("clearTimeText").objectReferenceValue = clearTimeTmp;
        so.ApplyModifiedProperties();
    }

    // ── SaveManager ───────────────────────────────────────────────
    private static void SetupSaveManager(GameObject managers)
    {
        var sm = managers.GetComponent<SaveManager>() ?? managers.AddComponent<SaveManager>();

        // Assets/_Project/ScriptableObjects/Items 配下の全ItemDataを収集
        var guids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/_Project/ScriptableObjects" });
        var items = new ItemData[guids.Length];
        for (int i = 0; i < guids.Length; i++)
            items[i] = AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guids[i]));

        var so = new SerializedObject(sm);
        var prop = so.FindProperty("allItems");
        prop.arraySize = items.Length;
        for (int i = 0; i < items.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        so.ApplyModifiedProperties();

        Debug.Log($"[P2Back] SaveManager: {items.Length}個のItemData登録");
    }

    // ── ゲームクリアUIを Canvas_Main 直下に再作成 ─────────────────────
    private static void SetupClearUI(GameObject canvas, GameObject managers)
    {
        // TitleOverlay 削除時に消えた ClearOverlay を再作成
        var existing = GameObject.Find("ClearOverlay");
        if (existing != null) Object.DestroyImmediate(existing);

        var overlay = CreateUIObj("ClearOverlay", canvas, 0, 0);
        var rt = overlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        overlay.AddComponent<Image>().color = new Color(0.04f, 0.03f, 0.08f, 0.95f);

        // 「脱出成功！」テキスト
        var clearGO = CreateUIObj("ClearText", overlay, 700, 100);
        SetAnchorCenter(clearGO, new Vector2(0, 80));
        var clearTmp = clearGO.AddComponent<TextMeshProUGUI>();
        clearTmp.text = "脱出成功！";
        clearTmp.fontSize = 72;
        clearTmp.color = new Color(0.95f, 0.88f, 0.65f);
        clearTmp.alignment = TextAlignmentOptions.Center;
        clearTmp.fontStyle = FontStyles.Bold;

        // サブテキスト
        var subGO = CreateUIObj("SubText", overlay, 700, 48);
        SetAnchorCenter(subGO, new Vector2(0, 10));
        var subTmp = subGO.AddComponent<TextMeshProUGUI>();
        subTmp.text = "あなたは嵐の洋館から脱出した！";
        subTmp.fontSize = 28;
        subTmp.color = new Color(0.85f, 0.82f, 0.75f);
        subTmp.alignment = TextAlignmentOptions.Center;

        // GameClearUI コンポーネントに配線
        var clearUI = managers.GetComponent<GameClearUI>() ?? managers.AddComponent<GameClearUI>();
        var so = new SerializedObject(clearUI);
        so.FindProperty("overlay").objectReferenceValue   = overlay;
        so.FindProperty("clearText").objectReferenceValue = clearTmp;
        so.FindProperty("subText").objectReferenceValue   = subTmp;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(clearUI);
        Debug.Log("[P2Back] ClearOverlay 再作成完了");
    }

    // ── GameClearUI に subText・backToTitleButton 配線、TimerUI に clearTimeText 配線 ──
    private static void WireClearTimeText(GameObject managers)
    {
        var clearUI = Object.FindAnyObjectByType<GameClearUI>();
        if (clearUI == null) { Debug.LogWarning("[P2Back] GameClearUI が見つかりません"); return; }

        var clearOverlay = GameObject.Find("ClearOverlay");
        Button backBtn = null;
        TextMeshProUGUI subTmp = null;

        if (clearOverlay != null)
        {
            // SubText を探して参照取得
            var subTextGO = clearOverlay.transform.Find("SubText");
            if (subTextGO != null)
                subTmp = subTextGO.GetComponent<TextMeshProUGUI>();

            // BackToTitleButton を再作成
            var existingBack = clearOverlay.transform.Find("BackToTitleButton");
            if (existingBack != null) Object.DestroyImmediate(existingBack.gameObject);
            backBtn = CreateButton("BackToTitleButton", clearOverlay, "タイトルに戻る",
                new Vector2(0, -220), new Vector2(340, 66));
        }

        // GameClearUI に配線
        var clearSo = new SerializedObject(clearUI);
        if (subTmp   != null) clearSo.FindProperty("subText").objectReferenceValue = subTmp;
        if (backBtn  != null) clearSo.FindProperty("backToTitleButton").objectReferenceValue = backBtn;
        clearSo.ApplyModifiedProperties();
        EditorUtility.SetDirty(clearUI);

        // TimerUI.clearTimeText も再配線（SetupTimerUI で作成済みの ClearTimeText を繋ぐ）
        var timerUI = managers.GetComponent<TimerUI>();
        if (timerUI != null && clearOverlay != null)
        {
            var ctGO = clearOverlay.transform.Find("ClearTimeText");
            if (ctGO != null)
            {
                var timerSo = new SerializedObject(timerUI);
                timerSo.FindProperty("clearTimeText").objectReferenceValue =
                    ctGO.GetComponent<TextMeshProUGUI>();
                timerSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(timerUI);
            }
        }

        Debug.Log("[P2Back] GameClearUI: subText・backToTitleButton 配線完了");
    }

    // ── 壁掛け時計をデスクエリアの壁に配置 ───────────────────────────
    private static void SetupClockObject()
    {
        var existing = GameObject.Find("Clock");
        if (existing != null) Object.DestroyImmediate(existing);

        var clock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        clock.name = "Clock";
        clock.transform.position = new Vector3(2.0f, 2.5f, 5.88f);
        clock.transform.localScale = new Vector3(1.0f, 1.0f, 0.05f); // 0.5→1.0 で2倍に
        ApplyColorMat(clock.GetComponent<MeshRenderer>(), new Color(0.92f, 0.88f, 0.80f), "Mat_Clock"); // クリーム色ベース

        clock.AddComponent<ClockFace>();
        clock.AddComponent<ClockInteractable>();
        clock.AddComponent<HoverHighlight>();

        EditorUtility.SetDirty(clock);
        Debug.Log("[P2Back] Clock 配置完了 (2.0, 2.5, 5.88)");
    }

    // ── 暖炉マントル上の写真（本の並び順ヒント）を配置 ────────────────
    private static void SetupFireplacePhotoObject()
    {
        var existing = GameObject.Find("FireplacePhoto");
        if (existing != null) Object.DestroyImmediate(existing);

        // 写真フレーム（マントル上）
        var photo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        photo.name = "FireplacePhoto";
        photo.transform.position = new Vector3(3.5f, 3.1f, 5.52f);
        photo.transform.localScale = new Vector3(0.5f, 0.45f, 0.03f);
        ApplyColorMat(photo.GetComponent<MeshRenderer>(), new Color(0.88f, 0.82f, 0.70f), "Mat_PhotoFrame");

        // 本の順番を示すカラーストリップ（青→赤→緑）
        AddColorStrip(photo, "Strip_Blue",  new Color(0.20f, 0.40f, 0.85f), new Vector3(-0.30f, 0f, -0.7f), new Vector3(0.25f, 0.70f, 0.3f));
        AddColorStrip(photo, "Strip_Red",   new Color(0.80f, 0.20f, 0.20f), new Vector3(0f,     0f, -0.7f), new Vector3(0.25f, 0.70f, 0.3f));
        AddColorStrip(photo, "Strip_Green", new Color(0.20f, 0.70f, 0.30f), new Vector3(0.30f,  0f, -0.7f), new Vector3(0.25f, 0.70f, 0.3f));

        // リフレクションで追加（エディタアセンブリからの直接参照を回避）
        var photoType = typeof(ClockInteractable).Assembly.GetType("EscapeGame.Game.FireplacePhotoInteractable");
        if (photoType != null) photo.AddComponent(photoType);
        else Debug.LogWarning("[P2Back] FireplacePhotoInteractable が見つかりません");
        photo.AddComponent<HoverHighlight>();

        EditorUtility.SetDirty(photo);
        Debug.Log("[P2Back] FireplacePhoto 配置完了 (3.5, 3.1, 5.52)");
    }

    private static void AddColorStrip(GameObject parent, string name, Color color,
        Vector3 localPos, Vector3 localScale)
    {
        var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = name;
        strip.transform.SetParent(parent.transform);
        strip.transform.localPosition = localPos;
        strip.transform.localScale    = localScale;
        Object.DestroyImmediate(strip.GetComponent<Collider>());
        ApplyColorMat(strip.GetComponent<MeshRenderer>(), color, "Mat_" + name);
    }

    private static void ApplyColorMat(MeshRenderer renderer, Color color, string matName)
    {
        const string matDir = "Assets/_Project/Materials/Generated";
        string matPath = matDir + "/" + matName + ".mat";

        if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials"))
            AssetDatabase.CreateFolder("Assets/_Project", "Materials");
        if (!AssetDatabase.IsValidFolder(matDir))
            AssetDatabase.CreateFolder("Assets/_Project/Materials", "Generated");

        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader) { name = matName };
            AssetDatabase.CreateAsset(mat, matPath);
        }
        mat.color = color;
        EditorUtility.SetDirty(mat);
        renderer.sharedMaterial = mat;
    }

    // ── JP フォント全適用 ────────────────────────────────────────
    private static void ApplyJpFont(TMP_FontAsset fontAsset = null)
    {
        // 引数がない場合はディスクから読み込む（フォールバック）
        if (fontAsset == null)
            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/_Project/Fonts/NotoSansJP_Dynamic.asset");
        if (fontAsset == null) { Debug.LogWarning("[P2Back] JP Font not found"); return; }

        var mat = fontAsset.material;
        int count = 0;
        foreach (var tmp in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include))
        {
            tmp.enabled = true;  // フォント削除時に無効化された場合に復元
            tmp.font = fontAsset;
            if (mat != null) tmp.fontSharedMaterial = mat;
            EditorUtility.SetDirty(tmp);
            count++;
        }
        Debug.Log($"[P2Back] JP Font 適用: {count} objects (asset: {fontAsset.name})");
    }

    // ── ヘルパー ──────────────────────────────────────────────────
    private static Button CreateButton(string name, GameObject parent, string label,
        Vector2 pos, Vector2 size, Color? bgColor = null)
    {
        var go = CreateUIObj(name, parent, size.x, size.y);
        SetAnchorCenter(go, pos);
        var bg = go.AddComponent<Image>();
        bg.color = bgColor ?? new Color(0.2f, 0.18f, 0.28f, 1f);
        var btn = go.AddComponent<Button>();

        var textGO = CreateUIObj("Text", go, size.x, size.y);
        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    private static GameObject CreateUIObj(string name, GameObject parent, float w, float h)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(w, h);
        return go;
    }

    private static void SetAnchorCenter(GameObject go, Vector2 pos)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
    }

    // ── WebGL ビルド設定 ─────────────────────────────────────────
    [MenuItem("EscapeGame/Setup/Configure WebGL Settings")]
    public static void ConfigureWebGL()
    {
        PlayerSettings.companyName   = "EscapeGame";
        PlayerSettings.productName   = "嵐の洋館";
        PlayerSettings.bundleVersion = "0.2.0";

        if (AssetDatabase.IsValidFolder("Assets/WebGLTemplates/Mobile"))
            PlayerSettings.WebGL.template = "PROJECT:Mobile";

        var guids  = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project/Scenes" });
        var scenes = guids
            .Select(g => new EditorBuildSettingsScene(AssetDatabase.GUIDToAssetPath(g), true))
            .ToArray();
        EditorBuildSettings.scenes = scenes;
        AssetDatabase.SaveAssets();

        Debug.Log($"[WebGL] 設定完了: {scenes.Length} シーン, テンプレート={PlayerSettings.WebGL.template}");
        Debug.Log("[WebGL] 次: File > Build Settings > WebGL に Switch Platform → EscapeGame/Build/Build WebGL");
    }

    [MenuItem("EscapeGame/Build/Build WebGL")]
    public static void BuildWebGL()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
        {
            Debug.LogError("[WebGL] File > Build Settings で WebGL に Switch Platform してから実行してください。");
            return;
        }
        string buildPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Builds", "WebGL");
        Directory.CreateDirectory(buildPath);

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0) { Debug.LogError("[WebGL] シーン未登録"); return; }

        var report = BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.WebGL, BuildOptions.None);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[WebGL] ビルド成功 → {buildPath}");
            EditorUtility.RevealInFinder(buildPath);
        }
        else
            Debug.LogError($"[WebGL] ビルド失敗: {report.summary.result}");
    }

    [MenuItem("EscapeGame/Build/Switch to WebGL + Build")]
    public static void SwitchAndBuildWebGL()
    {
        Debug.Log("[WebGL] WebGL プラットフォームに切り替え中...");
        bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.WebGL, BuildTarget.WebGL);

        if (!switched)
        {
            Debug.LogError("[WebGL] 切り替え失敗。Unity Hub で WebGL Build Support モジュールをインストールしてください。");
            return;
        }
        Debug.Log("[WebGL] 切り替え完了。ビルド開始...");
        BuildWebGL();
    }
}
#endif
