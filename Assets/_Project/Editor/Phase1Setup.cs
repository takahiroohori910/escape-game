#if UNITY_EDITOR
using EscapeGame.Core;
using EscapeGame.Game;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

public class Phase1Setup
{
    [MenuItem("EscapeGame/Setup/Phase1 Complete Setup")]
    public static void Run()
    {
        SetupRewards();
        SetupBookshelfUI();
        SetupInventoryUI();
        SetupGameClearUI();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Phase1Setup] 全セットアップ完了・シーン保存");
    }

    // ── パズル報酬 ─────────────────────────────────────────────────
    static void SetupRewards()
    {
        var phoneCord     = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/_Project/ScriptableObjects/Items/PhoneCord.asset");
        var circuitBoard  = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/_Project/ScriptableObjects/Items/CircuitBoard.asset");

        WireReward(GameObject.Find("Bookshelf"),  phoneCord,    Object.FindAnyObjectByType<BookshelfPuzzle>()?.OnSolved);
        WireRewardDesk(GameObject.Find("DeskTop"), circuitBoard);
    }

    static void WireReward(GameObject go, ItemData item, UnityEngine.Events.UnityEvent evt)
    {
        if (go == null || item == null || evt == null) { Debug.LogWarning("[Phase1Setup] WireReward: null 参照"); return; }
        var giver = go.GetComponent<PuzzleRewardGiver>() ?? go.AddComponent<PuzzleRewardGiver>();
        var so = new SerializedObject(giver);
        so.FindProperty("rewardItem").objectReferenceValue = item;
        so.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(evt, giver.GiveReward);
        EditorUtility.SetDirty(go);
        Debug.Log($"[Phase1Setup] {go.name} → {item.ItemName} 報酬配線完了");
    }

    static void WireRewardDesk(GameObject go, ItemData item)
    {
        if (go == null || item == null) return;
        var puzzle = go.GetComponent<DeskPuzzle>();
        if (puzzle == null) return;
        WireReward(go, item, puzzle.OnSolved);
    }

    // ── 本棚ステータスUI ──────────────────────────────────────────
    static void SetupBookshelfUI()
    {
        var canvas = GameObject.Find("Canvas_Main");
        if (canvas == null) return;

        var existing = GameObject.Find("BookshelfStatusPanel");
        if (existing != null) Object.DestroyImmediate(existing);

        var panel = CreateUIObject("BookshelfStatusPanel", canvas, 400, 100);
        SetAnchorTop(panel, 60f);
        AddImage(panel, new Color(0.05f, 0.05f, 0.05f, 0.8f));

        var instrGO = CreateUIObject("Instruction", panel, 380, 30);
        SetAnchorTop(instrGO, 15f);
        var instr = instrGO.AddComponent<TextMeshProUGUI>();
        instr.text = "本をクリックして順番を変えよう";
        instr.fontSize = 14; instr.color = new Color(0.8f, 0.8f, 0.8f);
        instr.alignment = TextAlignmentOptions.Center;

        var statusGO = CreateUIObject("StatusText", panel, 380, 50);
        SetAnchorBottom(statusGO, 10f);
        var status = statusGO.AddComponent<TextMeshProUGUI>();
        status.text = "[ 1 ]   [ 2 ]   [ 3 ]";
        status.fontSize = 32; status.color = Color.white;
        status.alignment = TextAlignmentOptions.Center;

        var managers = GameObject.Find("Managers");
        var ui = managers.GetComponent<BookshelfStatusUI>() ?? managers.AddComponent<BookshelfStatusUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("panel").objectReferenceValue          = panel;
        so.FindProperty("statusText").objectReferenceValue     = status;
        so.FindProperty("instructionText").objectReferenceValue = instr;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(managers);
        Debug.Log("[Phase1Setup] BookshelfStatusUI 作成完了");
    }

    // ── インベントリUI ────────────────────────────────────────────
    static void SetupInventoryUI()
    {
        var canvas = GameObject.Find("Canvas_Main");
        if (canvas == null) return;

        var existing = GameObject.Find("InventoryBar");
        if (existing != null) Object.DestroyImmediate(existing);

        // スロットプレハブ（非アクティブで保持）
        var prefabParent = GameObject.Find("_Prefabs") ?? new GameObject("_Prefabs");
        prefabParent.SetActive(false);
        var slotPrefab = CreateUIObject("SlotPrefab", prefabParent, 70, 70);
        AddImage(slotPrefab, new Color(0.15f, 0.15f, 0.15f, 0.7f));
        var slotLabel = CreateUIObject("Label", slotPrefab, 70, 30);
        SetAnchorBottom(slotLabel, 4f);
        var lbl = slotLabel.AddComponent<TextMeshProUGUI>();
        lbl.fontSize = 11; lbl.color = Color.white;
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.enableWordWrapping = true;

        // バー本体
        var bar = CreateUIObject("InventoryBar", canvas, 640, 80);
        SetAnchorBottomFull(bar, 10f);
        AddImage(bar, new Color(0f, 0f, 0f, 0.5f));

        var container = CreateUIObject("SlotContainer", bar, 640, 80);
        SetAnchorStretch(container);
        var layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 6; layout.padding = new RectOffset(10, 10, 5, 5);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false; layout.childControlHeight = false;

        var managers = GameObject.Find("Managers");
        var ui = managers.GetComponent<InventoryUI>() ?? managers.AddComponent<InventoryUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("slotPrefab").objectReferenceValue    = slotPrefab;
        so.FindProperty("slotContainer").objectReferenceValue = container.transform;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(managers);
        Debug.Log("[Phase1Setup] InventoryUI 作成完了");
    }

    // ── ゲームクリアUI ────────────────────────────────────────────
    static void SetupGameClearUI()
    {
        var canvas = GameObject.Find("Canvas_Main");
        if (canvas == null) return;

        var existing = GameObject.Find("ClearOverlay");
        if (existing != null) Object.DestroyImmediate(existing);

        var overlay = CreateUIObject("ClearOverlay", canvas, 0, 0);
        SetAnchorStretch(overlay);
        AddImage(overlay, new Color(0f, 0f, 0f, 0.85f));
        overlay.SetActive(false);

        var textGO = CreateUIObject("ClearText", overlay, 600, 120);
        SetAnchorCenter(textGO);
        var txt = textGO.AddComponent<TextMeshProUGUI>();
        txt.text = "脱出成功！";
        txt.fontSize = 64; txt.color = new Color(1f, 0.9f, 0.3f);
        txt.alignment = TextAlignmentOptions.Center;

        var subGO = CreateUIObject("SubText", overlay, 500, 50);
        SetAnchorCenter(subGO);
        subGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -80);
        var sub = subGO.AddComponent<TextMeshProUGUI>();
        sub.text = "あなたは嵐の洋館から脱出した！";
        sub.fontSize = 22; sub.color = Color.white;
        sub.alignment = TextAlignmentOptions.Center;

        var managers = GameObject.Find("Managers");
        var ui = managers.GetComponent<GameClearUI>() ?? managers.AddComponent<GameClearUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("overlay").objectReferenceValue   = overlay;
        so.FindProperty("clearText").objectReferenceValue = txt;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(managers);
        Debug.Log("[Phase1Setup] GameClearUI 作成完了");
    }

    // ── UIヘルパー ────────────────────────────────────────────────
    static GameObject CreateUIObject(string name, GameObject parent, float w, float h)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(w, h);
        return go;
    }

    static void SetAnchorCenter(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
    }

    static void SetAnchorTop(GameObject go, float offsetY)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -offsetY);
    }

    static void SetAnchorBottom(GameObject go, float offsetY)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, offsetY);
    }

    static void SetAnchorBottomFull(GameObject go, float offsetY)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, offsetY + rt.sizeDelta.y / 2f);
    }

    static void SetAnchorStretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void AddImage(GameObject go, Color color)
    {
        var img = go.AddComponent<Image>();
        img.color = color;
    }
}
#endif
