#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class FireplaceSetup
{
    [MenuItem("EscapeGame/Setup/Fix Fireplace Meshes")]
    public static void FixFireplaceMeshes()
    {
        var cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        if (cubeMesh == null) { Debug.LogError("[FireplaceSetup] Cube.fbx が見つかりません"); return; }

        // 既存マテリアル取得（部屋と同じものを流用）
        var wallMat  = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/WallMaterial.mat");
        var deskMat  = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/DeskMaterial.mat");
        var floorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/FloorMaterial.mat");

        SetMesh("FireplaceFrame", cubeMesh, wallMat  ?? CreateGray(new Color(0.35f, 0.22f, 0.12f)));
        SetMesh("Mantle",         cubeMesh, deskMat  ?? CreateGray(new Color(0.45f, 0.30f, 0.18f)));
        SetMesh("Telephone",      cubeMesh, floorMat ?? CreateGray(new Color(0.15f, 0.15f, 0.18f)));

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FireplaceSetup] 暖炉メッシュ設定・シーン保存完了");
    }

    private static void SetMesh(string goName, Mesh mesh, Material mat)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"[FireplaceSetup] {goName} が見つかりません"); return; }

        var mf = go.GetComponent<UnityEngine.MeshFilter>();
        var mr = go.GetComponent<UnityEngine.MeshRenderer>();
        if (mf == null || mr == null) { Debug.LogWarning($"[FireplaceSetup] {goName} にMeshFilter/MeshRendererがありません"); return; }

        var soMF = new SerializedObject(mf);
        soMF.FindProperty("m_Mesh").objectReferenceValue = mesh;
        soMF.ApplyModifiedProperties();

        if (mat != null)
        {
            var soMR = new SerializedObject(mr);
            var matsProp = soMR.FindProperty("m_Materials");
            if (matsProp.arraySize == 0) matsProp.arraySize = 1;
            matsProp.GetArrayElementAtIndex(0).objectReferenceValue = mat;
            soMR.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(go);
        Debug.Log($"[FireplaceSetup] {goName} メッシュ設定完了");
    }

    private static Material CreateGray(Color col)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = col;
        return mat;
    }
}
#endif
