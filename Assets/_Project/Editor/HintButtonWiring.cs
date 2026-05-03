#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using EscapeGame.Game;

public class HintButtonWiring
{
    [MenuItem("EscapeGame/Setup/Wire Buttons")]
    public static void Run()
    {
        WireHintButton();
        WireMenuButton();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[ButtonWiring] 配線完了");
    }

    static void WireHintButton()
    {
        var btn = GameObject.Find("HintButton")?.GetComponent<Button>();
        if (btn == null) { Debug.LogError("[ButtonWiring] HintButton が見つかりません"); return; }

        var hintUI = Object.FindAnyObjectByType<HintUI>();
        if (hintUI == null) { Debug.LogError("[ButtonWiring] HintUI が見つかりません"); return; }

        // 永続リスナーを全削除してから1つだけ追加（重複防止）
        var so = new SerializedObject(btn);
        so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls").ClearArray();
        so.ApplyModifiedProperties();
        btn.onClick.RemoveAllListeners();
        UnityEventTools.AddPersistentListener(btn.onClick, hintUI.Toggle);
        EditorUtility.SetDirty(btn);
        Debug.Log("[ButtonWiring] HintButton → HintUI.Toggle() 接続完了");
    }

    static void WireMenuButton()
    {
        var menuGO = GameObject.Find("MenuButton");
        if (menuGO == null) { Debug.LogError("[ButtonWiring] MenuButton が見つかりません"); return; }

        var btn = menuGO.GetComponent<Button>();
        if (btn == null) { Debug.LogError("[ButtonWiring] MenuButton に Button コンポーネントがありません"); return; }

        var rvc = Object.FindAnyObjectByType<RoomViewController>();
        if (rvc == null) { Debug.LogError("[ButtonWiring] RoomViewController が見つかりません"); return; }

        // 永続リスナーを全削除してから配線（重複防止）
        var so = new SerializedObject(btn);
        so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls").ClearArray();
        so.ApplyModifiedProperties();
        btn.onClick.RemoveAllListeners();
        UnityEventTools.AddPersistentListener(btn.onClick, rvc.MoveToCurrentOverview);
        EditorUtility.SetDirty(btn);
        Debug.Log("[ButtonWiring] MenuButton → RoomViewController.MoveToCurrentOverview() 接続完了");
    }
}
#endif
