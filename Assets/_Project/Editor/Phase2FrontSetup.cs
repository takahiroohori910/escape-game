#if UNITY_EDITOR
using EscapeGame.Core;
using EscapeGame.Game;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Phase2FrontSetup
{
    [MenuItem("EscapeGame/Setup/Phase2 Front Setup")]
    public static void Run()
    {
        var canvas = GameObject.Find("Canvas_Main");
        if (canvas == null) { Debug.LogError("[P2Front] Canvas_Main が見つかりません"); return; }
        var managers = GameObject.Find("Managers");
        if (managers == null) { Debug.LogError("[P2Front] Managers が見つかりません"); return; }

        SetupHintUI(canvas, managers);
        SetupPopupUI(canvas, managers);
        SetupNoteUI(canvas, managers);
        SetupItemDetailUI(canvas, managers);
        SetupNoteAssets();
        SetupNoteObjects();
        AddHoverHighlights();
        WireInventoryUI(managers);
        WireGameClearSubText(managers);

        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(managers);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[P2Front] Phase2前半セットアップ完了");
    }

    // ── ヒントUI ─────────────────────────────────────────────────
    private static void SetupHintUI(GameObject canvas, GameObject managers)
    {
        var existing = GameObject.Find("HintPanel");
        if (existing != null) Object.DestroyImmediate(existing);

        var panel = CreateUIObj("HintPanel", canvas, 1000, 72);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -44f);

        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.72f);

        var textGO = CreateUIObj("HintText", panel, 980, 66);
        var rt2 = textGO.GetComponent<RectTransform>();
        rt2.anchorMin = Vector2.zero; rt2.anchorMax = Vector2.one;
        rt2.offsetMin = rt2.offsetMax = Vector2.zero;
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 32;
        tmp.color = new Color(0.9f, 0.9f, 0.7f);
        tmp.alignment = TextAlignmentOptions.Center;

        var ui = managers.GetComponent<HintUI>() ?? managers.AddComponent<HintUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("panel").objectReferenceValue    = panel;
        so.FindProperty("hintText").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();
    }

    // ── ポップアップUI ──────────────────────────────────────────
    private static void SetupPopupUI(GameObject canvas, GameObject managers)
    {
        var existing = GameObject.Find("PopupPanel");
        if (existing != null) Object.DestroyImmediate(existing);

        var panel = CreateUIObj("PopupPanel", canvas, 640, 110);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, 100f);

        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

        var textGO = CreateUIObj("PopupText", panel, 620, 104);
        var rt2 = textGO.GetComponent<RectTransform>();
        rt2.anchorMin = Vector2.zero; rt2.anchorMax = Vector2.one;
        rt2.offsetMin = rt2.offsetMax = Vector2.zero;
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 26;
        tmp.color = new Color(1f, 0.95f, 0.7f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;

        var ui = managers.GetComponent<PopupUI>() ?? managers.AddComponent<PopupUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("panel").objectReferenceValue       = panel;
        so.FindProperty("messageText").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();
    }

    // ── メモUI ───────────────────────────────────────────────────
    private static void SetupNoteUI(GameObject canvas, GameObject managers)
    {
        var existing = GameObject.Find("NoteOverlay");
        if (existing != null) Object.DestroyImmediate(existing);

        // 全画面暗幕
        var overlay = CreateUIObj("NoteOverlay", canvas, 0, 0);
        var rt = overlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var ovBg = overlay.AddComponent<Image>();
        ovBg.color = new Color(0f, 0f, 0f, 0.75f);

        // メモパネル
        var notePanel = CreateUIObj("NotePanel", overlay, 700, 460);
        SetAnchorCenter(notePanel);
        var noteBg = notePanel.AddComponent<Image>();
        noteBg.color = new Color(0.88f, 0.82f, 0.65f, 1f);

        // タイトル
        var titleGO = CreateUIObj("NoteTitle", notePanel, 660, 60);
        var titleRt = titleGO.GetComponent<RectTransform>();
        titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0, -40f);
        var titleTmp = titleGO.AddComponent<TextMeshProUGUI>();
        titleTmp.fontSize = 32;
        titleTmp.color = new Color(0.2f, 0.1f, 0f);
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;

        // 本文
        var contentGO = CreateUIObj("NoteContent", notePanel, 660, 300);
        var contentRt = contentGO.GetComponent<RectTransform>();
        contentRt.anchorMin = contentRt.anchorMax = new Vector2(0.5f, 0.5f);
        contentRt.anchoredPosition = new Vector2(0, -30f);
        var contentTmp = contentGO.AddComponent<TextMeshProUGUI>();
        contentTmp.fontSize = 26;
        contentTmp.color = new Color(0.15f, 0.1f, 0f);
        contentTmp.textWrappingMode = TextWrappingModes.Normal;

        // 閉じるボタン
        var closeBtnGO = CreateUIObj("CloseButton", notePanel, 160, 50);
        var closeBtnRt = closeBtnGO.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = closeBtnRt.anchorMax = new Vector2(0.5f, 0f);
        closeBtnRt.anchoredPosition = new Vector2(0, 32f);
        closeBtnGO.AddComponent<Image>().color = new Color(0.3f, 0.2f, 0.1f);
        var closeBtn = closeBtnGO.AddComponent<Button>();
        var closeTxtGO = CreateUIObj("Text", closeBtnGO, 160, 50);
        var closeTxt = closeTxtGO.AddComponent<TextMeshProUGUI>();
        closeTxt.text = "閉じる";
        closeTxt.fontSize = 24;
        closeTxt.color = Color.white;
        closeTxt.alignment = TextAlignmentOptions.Center;
        var closeTxtRt = closeTxtGO.GetComponent<RectTransform>();
        closeTxtRt.anchorMin = Vector2.zero; closeTxtRt.anchorMax = Vector2.one;
        closeTxtRt.offsetMin = closeTxtRt.offsetMax = Vector2.zero;

        overlay.SetActive(false);

        var ui = managers.GetComponent<NoteUI>() ?? managers.AddComponent<NoteUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("overlay").objectReferenceValue      = overlay;
        so.FindProperty("titleText").objectReferenceValue    = titleTmp;
        so.FindProperty("contentText").objectReferenceValue  = contentTmp;
        so.FindProperty("closeButton").objectReferenceValue  = closeBtn;
        so.ApplyModifiedProperties();
    }

    // ── アイテム詳細UI ────────────────────────────────────────────
    private static void SetupItemDetailUI(GameObject canvas, GameObject managers)
    {
        var existing = GameObject.Find("ItemDetailPanel");
        if (existing != null) Object.DestroyImmediate(existing);

        var panel = CreateUIObj("ItemDetailPanel", canvas, 500, 150);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 115f);
        panel.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

        var nameGO = CreateUIObj("ItemName", panel, 480, 56);
        var nameRt = nameGO.GetComponent<RectTransform>();
        nameRt.anchorMin = nameRt.anchorMax = new Vector2(0.5f, 1f);
        nameRt.anchoredPosition = new Vector2(0, -32f);
        var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
        nameTmp.fontSize = 28;
        nameTmp.color = new Color(1f, 0.9f, 0.5f);
        nameTmp.fontStyle = FontStyles.Bold;
        nameTmp.alignment = TextAlignmentOptions.Center;

        var descGO = CreateUIObj("ItemDesc", panel, 480, 60);
        var descRt = descGO.GetComponent<RectTransform>();
        descRt.anchorMin = descRt.anchorMax = new Vector2(0.5f, 0f);
        descRt.anchoredPosition = new Vector2(0, 24f);
        var descTmp = descGO.AddComponent<TextMeshProUGUI>();
        descTmp.fontSize = 22;
        descTmp.color = new Color(0.8f, 0.8f, 0.8f);
        descTmp.alignment = TextAlignmentOptions.Center;
        descTmp.textWrappingMode = TextWrappingModes.Normal;

        panel.SetActive(false);

        var ui = managers.GetComponent<ItemDetailUI>() ?? managers.AddComponent<ItemDetailUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("panel").objectReferenceValue    = panel;
        so.FindProperty("nameText").objectReferenceValue = nameTmp;
        so.FindProperty("descText").objectReferenceValue = descTmp;
        so.ApplyModifiedProperties();
    }

    // ── NoteData アセット作成 ────────────────────────────────────
    private static void SetupNoteAssets()
    {
        CreateNoteAsset("NoteDesk",
            "note_desk",
            "古びたメモ",
            "館が建てられた年——1742年。\n先生はその数字を引き出しの鍵に\n使われていたとおっしゃっていた。");

        CreateNoteAsset("NoteBookshelf",
            "note_bookshelf",
            "ページの切れ端",
            "三冊の本の順番には意味がある。\n写真では左から 三・一・四 と\n並んでいた。");

        CreateNoteAsset("NoteFireplace",
            "note_fireplace",
            "走り書きのメモ",
            "電話機が壊れている。\n受話器のコードと内部の基板さえ\n揃えば修理できるはずだ。");
    }

    private static void CreateNoteAsset(string fileName, string noteId, string title, string content)
    {
        string path = $"Assets/_Project/ScriptableObjects/Notes/{fileName}.asset";
        System.IO.Directory.CreateDirectory("Assets/_Project/ScriptableObjects/Notes");
        var existing = AssetDatabase.LoadAssetAtPath<NoteData>(path);
        if (existing != null) return;

        var note = ScriptableObject.CreateInstance<NoteData>();
        var so = new SerializedObject(note);
        so.FindProperty("noteId").stringValue  = noteId;
        so.FindProperty("title").stringValue   = title;
        so.FindProperty("content").stringValue = content;
        so.ApplyModifiedProperties();
        AssetDatabase.CreateAsset(note, path);
        AssetDatabase.SaveAssets();
    }

    // ── メモオブジェクト配置 ─────────────────────────────────────
    private static void SetupNoteObjects()
    {
        PlaceNote("NoteOnDesk",      new Vector3(3.5f, 2.62f, 3.8f), RoomArea.Desk,      "note_desk");
        PlaceNote("NoteOnBookshelf", new Vector3(-3.8f, 2.0f, 5.0f), RoomArea.Bookshelf, "note_bookshelf");
        // 電話機(Telephone)から離れた暖炉左側マントル上に配置
        PlaceNote("NoteOnFireplace", new Vector3(2.0f, 3.2f, 5.5f), RoomArea.Fireplace, "note_fireplace");
    }

    private static void PlaceNote(string goName, Vector3 pos, RoomArea area, string noteId)
    {
        var existing = GameObject.Find(goName);
        if (existing != null) Object.DestroyImmediate(existing);

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = goName;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(0.4f, 0.04f, 0.3f);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(1.0f, 0.95f, 0.6f); // 明るいクリーム色で目立つ
        go.GetComponent<Renderer>().sharedMaterial = mat;

        var noteData = AssetDatabase.LoadAssetAtPath<NoteData>(
            $"Assets/_Project/ScriptableObjects/Notes/{NoteIdToFile(noteId)}.asset");

        var ni = go.AddComponent<NoteInteractable>();
        var so = new SerializedObject(ni);
        so.FindProperty("noteData").objectReferenceValue = noteData;
        so.FindProperty("requiredArea").enumValueIndex   = (int)area;
        so.ApplyModifiedProperties();

        var hover = go.AddComponent<HoverHighlight>();
        EditorUtility.SetDirty(go);
    }

    private static string NoteIdToFile(string noteId) => noteId switch
    {
        "note_desk"       => "NoteDesk",
        "note_bookshelf"  => "NoteBookshelf",
        "note_fireplace"  => "NoteFireplace",
        _ => noteId
    };

    // ── HoverHighlight を主要インタラクタブルに追加 ───────────────
    private static void AddHoverHighlights()
    {
        string[] targets = { "Book_01", "Book_02", "Book_03", "Painting", "FireplaceFrame", "Telephone" };
        foreach (var name in targets)
        {
            var go = GameObject.Find(name);
            if (go == null) continue;
            if (go.GetComponent<HoverHighlight>() == null)
                go.AddComponent<HoverHighlight>();
        }
    }

    // ── InventoryUI に ItemDetailUI を配線 ─────────────────────
    private static void WireInventoryUI(GameObject managers)
    {
        var invUI = Object.FindAnyObjectByType<InventoryUI>();
        var detailUI = Object.FindAnyObjectByType<ItemDetailUI>();
        if (invUI == null || detailUI == null) return;

        var so = new SerializedObject(invUI);
        so.FindProperty("itemDetailUI").objectReferenceValue = detailUI;
        so.ApplyModifiedProperties();
    }

    // ── GameClearUI に SubText を配線 ─────────────────────────
    private static void WireGameClearSubText(GameObject managers)
    {
        var clearUI = Object.FindAnyObjectByType<GameClearUI>();
        if (clearUI == null) return;
        var subText = GameObject.Find("SubText");
        if (subText == null) return;

        var tmp = subText.GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;
        var so = new SerializedObject(clearUI);
        so.FindProperty("subText").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();
    }

    // ── ヘルパー ──────────────────────────────────────────────────
    private static GameObject CreateUIObj(string name, GameObject parent, float w, float h)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(w, h);
        return go;
    }

    private static void SetAnchorCenter(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
    }
}
#endif
