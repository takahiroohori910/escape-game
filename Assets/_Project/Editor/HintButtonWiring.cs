#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using EscapeGame.Game;

public class HintButtonWiring
{
    [MenuItem("EscapeGame/Setup/Wire Hint Button")]
    public static void Run()
    {
        var btn = GameObject.Find("HintButton")?.GetComponent<Button>();
        if (btn == null) { Debug.LogError("[HintWiring] HintButton が見つかりません"); return; }

        var hintUI = Object.FindAnyObjectByType<HintUI>();
        if (hintUI == null) { Debug.LogError("[HintWiring] HintUI が見つかりません"); return; }

        // 既存リスナーをクリアして再接続
        btn.onClick.RemoveAllListeners();
        UnityEventTools.AddPersistentListener(btn.onClick, hintUI.Toggle);

        EditorUtility.SetDirty(btn);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[HintWiring] HintButton → HintUI.Toggle() 接続完了");
    }
}
#endif
