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
            AssetDatabase.LoadAssetAtPath<ItemData>(AssetPaths.Item_PhoneCord);
        so.FindProperty("circuitBoardItem").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<ItemData>(AssetPaths.Item_CircuitBoard);
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(wirer);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[WireRewards] PuzzleWirer アイテム配線・シーン保存完了");
    }
}
#endif
