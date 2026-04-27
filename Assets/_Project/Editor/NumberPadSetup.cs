#if UNITY_EDITOR
using EscapeGame.Game;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NumberPadSetup
{
    [MenuItem("EscapeGame/Setup/Create NumberPad UI")]
    public static void CreateNumberPadUI()
    {
        var canvas = GameObject.Find("Canvas_Main");
        if (canvas == null) { Debug.LogError("[NPSetup] Canvas_Main が見つかりません"); return; }

        // 既存パネル削除
        var existing = GameObject.Find("NumberPadPanel");
        if (existing != null) Object.DestroyImmediate(existing);

        // ── パネル ───────────────────────────────────────────────
        var panel = CreateUIObject("NumberPadPanel", canvas, 320, 420);
        SetAnchorCenter(panel);
        AddImage(panel, new Color(0.1f, 0.1f, 0.1f, 0.92f));

        // ── ヒントテキスト ────────────────────────────────────────
        var hintGO = CreateUIObject("HintText", panel, 280, 40);
        SetAnchorTop(hintGO, 200f);
        var hint = hintGO.AddComponent<TextMeshProUGUI>();
        hint.text = "";
        hint.fontSize = 14;
        hint.color = new Color(1f, 0.9f, 0.5f);
        hint.alignment = TextAlignmentOptions.Center;
        hint.enableWordWrapping = true;

        // ── コード表示 ────────────────────────────────────────────
        var displayGO = CreateUIObject("CodeDisplay", panel, 280, 60);
        SetAnchorTop(displayGO, 130f);
        var display = displayGO.AddComponent<TextMeshProUGUI>();
        display.text = "—  —  —";
        display.fontSize = 40;
        display.color = Color.white;
        display.alignment = TextAlignmentOptions.Center;

        // ── ボタングリッド ────────────────────────────────────────
        var grid = CreateUIObject("ButtonGrid", panel, 300, 240);
        SetAnchorBottom(grid, 20f);
        var layout = grid.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(88, 54);
        layout.spacing = new Vector2(8, 8);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;
        layout.childAlignment = TextAnchor.UpperCenter;

        string[] labels = { "7","8","9","4","5","6","1","2","3","CLR","0","" };
        foreach (var lbl in labels)
        {
            if (lbl == "") { CreateUIObject("Spacer", grid, 88, 54); continue; }
            CreateDigitButton(grid, lbl);
        }

        // ── NumberPadUI コンポーネント（Managers に追加）──────────
        var managers = GameObject.Find("Managers");
        if (managers == null) { Debug.LogError("[NPSetup] Managers が見つかりません"); return; }
        var ui = managers.GetComponent<NumberPadUI>() ?? managers.AddComponent<NumberPadUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("panel").objectReferenceValue       = panel;
        so.FindProperty("codeDisplay").objectReferenceValue = display;
        so.FindProperty("hintText").objectReferenceValue    = hint;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(managers);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[NPSetup] NumberPad UI 作成・シーン保存");
    }

    // ── ヘルパー ─────────────────────────────────────────────────

    private static GameObject CreateUIObject(string name, GameObject parent, float w, float h)
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

    private static void SetAnchorTop(GameObject go, float offsetY)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -offsetY);
    }

    private static void SetAnchorBottom(GameObject go, float offsetY)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, offsetY);
    }

    private static void AddImage(GameObject go, Color color)
    {
        var img = go.AddComponent<Image>();
        img.color = color;
    }

    private static void CreateDigitButton(GameObject parent, string label)
    {
        var go = CreateUIObject("Btn_" + label, parent, 88, 54);
        AddImage(go, new Color(0.25f, 0.25f, 0.3f, 1f));
        var btn = go.AddComponent<Button>();

        // ラベル
        var textGO = CreateUIObject("Text", go, 88, 54);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = label == "CLR" ? 18 : 26;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // DigitButton コンポーネントでランタイム配線（onClick.AddListener は保存されないため）
        var digitBtn = go.AddComponent<EscapeGame.Game.DigitButton>();
        var so = new SerializedObject(digitBtn);
        so.FindProperty("digit").stringValue  = label;
        so.FindProperty("isClear").boolValue  = (label == "CLR");
        so.ApplyModifiedProperties();
    }
}
#endif
