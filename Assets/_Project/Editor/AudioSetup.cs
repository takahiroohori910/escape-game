#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using EscapeGame.Core;

public class AudioSetup
{
    [MenuItem("EscapeGame/Setup/Audio Setup")]
    public static void Run()
    {
        var am = Object.FindAnyObjectByType<AudioManager>();
        if (am == null) { Debug.LogError("[Audio] AudioManager が見つかりません"); return; }

        var so = new SerializedObject(am);

        string seDir = "Assets/_Project/Audio/SE/";
        string bgmDir = "Assets/_Project/Audio/BGM/";

        Assign(so, "bgmMain",       bgmDir + "BGM_Main");
        Assign(so, "bgmClear",      bgmDir + "BGM_Clear");
        Assign(so, "seClick",       seDir + "SE_Click");
        Assign(so, "seHover",       seDir + "SE_Hover");
        Assign(so, "seBookMove",    seDir + "SE_BookMove");
        Assign(so, "sePuzzleSolve", seDir + "SE_PuzzleSolve");
        Assign(so, "sePuzzleFail",  seDir + "SE_PuzzleFail");
        Assign(so, "seItemPickup",  seDir + "SE_ItemPickup");
        Assign(so, "seCameraMove",  seDir + "SE_CameraMove");
        Assign(so, "seNoteOpen",    seDir + "SE_NoteOpen");
        Assign(so, "sePhoneRepair", seDir + "SE_PhoneRepair");
        Assign(so, "sePhoneCall",   seDir + "SE_PhoneCall");

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(am);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Audio] オーディオクリップ配線完了");
    }

    private static void Assign(SerializedObject so, string field, string pathBase)
    {
        string[] exts = { ".mp3", ".wav", ".ogg" };
        foreach (var ext in exts)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(pathBase + ext);
            if (clip != null)
            {
                so.FindProperty(field).objectReferenceValue = clip;
                Debug.Log($"[Audio] {field} = {clip.name}");
                return;
            }
        }
        Debug.LogWarning($"[Audio] {field}: クリップ未検出 ({pathBase})");
    }
}
#endif
