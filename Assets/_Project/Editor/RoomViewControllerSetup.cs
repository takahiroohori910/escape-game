#if UNITY_EDITOR
using EscapeGame.Game;
using UnityEditor;
using UnityEngine;

public class RoomViewControllerSetup
{
    [MenuItem("EscapeGame/Setup/Wire RoomViewController")]
    public static void WireRoomViewController()
    {
        var rvc = Object.FindAnyObjectByType<RoomViewController>();
        if (rvc == null) { Debug.LogError("[RVCSetup] RoomViewController が見つかりません"); return; }

        var mainCam = GameObject.FindWithTag("MainCamera");
        if (mainCam == null) { Debug.LogError("[RVCSetup] MainCamera が見つかりません"); return; }

        var so = new SerializedObject(rvc);
        so.FindProperty("cameraTransform").objectReferenceValue   = mainCam.transform;
        so.FindProperty("overviewPoint").objectReferenceValue     = FindChild("OverviewPoint");
        so.FindProperty("bookshelfPoint").objectReferenceValue    = FindChild("BookshelfPoint");
        so.FindProperty("deskPoint").objectReferenceValue         = FindChild("DeskPoint");
        so.FindProperty("fireplacePoint").objectReferenceValue    = FindChild("FireplacePoint");
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(rvc);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[RVCSetup] RoomViewController 配線完了・シーン保存");
    }

    private static Transform FindChild(string name)
    {
        var go = GameObject.Find(name);
        if (go == null) Debug.LogWarning($"[RVCSetup] {name} が見つかりません");
        return go?.transform;
    }
}
#endif
