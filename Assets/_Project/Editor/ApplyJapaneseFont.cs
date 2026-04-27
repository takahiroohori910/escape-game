#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;

public class ApplyJapaneseFont
{
    [MenuItem("EscapeGame/Setup/Apply JP Font")]
    public static void Apply()
    {
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/_Project/Fonts/NotoSansJP_Dynamic.asset");
        if (fontAsset == null) { Debug.LogError("[FontApply] Font Asset not found"); return; }

        int count = 0;
        foreach (var tmp in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include))
        {
            tmp.font = fontAsset;
            EditorUtility.SetDirty(tmp);
            count++;
        }
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[FontApply] Applied to {count} TMP objects");
    }
}
#endif
