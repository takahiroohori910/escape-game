#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class SafeSetup
{
    [MenuItem("EscapeGame/Setup/Add Desk Safe")]
    public static void Run()
    {
        if (Application.isPlaying) { Debug.LogError("[Safe] Edit mode で実行してください"); return; }

        var existing = GameObject.Find("DeskSafe");
        if (existing != null) Object.DestroyImmediate(existing);

        var deskTop = GameObject.Find("DeskTop");
        Vector3 dp = deskTop != null ? deskTop.transform.position : new Vector3(0f, 0.9f, 0.5f);

        // 金庫本体
        var safe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        safe.name = "DeskSafe";
        safe.transform.position   = new Vector3(dp.x + 0.35f, dp.y + 0.18f, dp.z - 0.05f);
        safe.transform.localScale = new Vector3(0.28f, 0.22f, 0.22f);
        safe.transform.rotation   = Quaternion.Euler(0f, -10f, 0f);

        // ダークグレーのマテリアル
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", new Color(0.20f, 0.20f, 0.20f));
        mat.SetFloat("_Smoothness", 0.6f);
        mat.SetFloat("_Metallic", 0.4f);
        const string matPath = "Assets/_Project/Materials/Generated/Mat_DeskSafe.mat";
        if (System.IO.File.Exists(matPath)) AssetDatabase.DeleteAsset(matPath);
        AssetDatabase.CreateAsset(mat, matPath);
        safe.GetComponent<MeshRenderer>().sharedMaterial = mat;

        // ダイヤル風の装飾（小さいシリンダー）
        var dial = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        dial.name = "SafeDial";
        dial.transform.SetParent(safe.transform);
        dial.transform.localPosition = new Vector3(-0.15f, 0f, -0.55f);
        dial.transform.localScale    = new Vector3(0.3f, 0.08f, 0.3f);
        dial.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Object.DestroyImmediate(dial.GetComponent<CapsuleCollider>());
        var dialMat = new Material(shader);
        dialMat.SetColor("_BaseColor", new Color(0.70f, 0.60f, 0.30f));
        dialMat.SetFloat("_Metallic", 0.8f);
        const string dialMatPath = "Assets/_Project/Materials/Generated/Mat_SafeDial.mat";
        if (System.IO.File.Exists(dialMatPath)) AssetDatabase.DeleteAsset(dialMatPath);
        AssetDatabase.CreateAsset(dialMat, dialMatPath);
        dial.GetComponent<MeshRenderer>().sharedMaterial = dialMat;

        // SafeInteractable と HoverHighlight を追加
        safe.AddComponent<EscapeGame.Game.SafeInteractable>();
        safe.AddComponent<EscapeGame.Game.HoverHighlight>();

        EditorUtility.SetDirty(safe);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Safe] 机の上に金庫を配置しました");
    }
}
#endif
