#if UNITY_EDITOR
using EscapeGame.Core;
using EscapeGame.Game;
using UnityEditor;
using UnityEngine;

public class WirePuzzleRewards
{
    [MenuItem("EscapeGame/Setup/Wire Puzzle Rewards")]
    public static void Wire()
    {
        var wirer = Object.FindAnyObjectByType<PuzzleWirer>();
        if (wirer == null) { Debug.LogError("[WireRewards] PuzzleWirer が見つかりません"); return; }

        var so = new SerializedObject(wirer);
        so.FindProperty("phoneCordItem").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<ItemData>("Assets/_Project/ScriptableObjects/Items/PhoneCord.asset");
        so.FindProperty("circuitBoardItem").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<ItemData>("Assets/_Project/ScriptableObjects/Items/CircuitBoard.asset");
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(wirer);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[WireRewards] PuzzleWirer アイテム配線・シーン保存完了");
    }
}
#endif
