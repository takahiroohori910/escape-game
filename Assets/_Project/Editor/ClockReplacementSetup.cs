#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class ClockReplacementSetup
{
    [MenuItem("EscapeGame/Setup/Replace Clock With 3D")]
    public static void Run()
    {
        if (Application.isPlaying) { Debug.LogError("[Clock3D] Edit mode で実行してください"); return; }

        // 既存Clockを削除
        var old = GameObject.Find("Clock");
        if (old != null) Object.DestroyImmediate(old);

        // 文字盤テクスチャ生成・保存
        string texFullDir = Path.Combine(Application.dataPath, "_Project/Textures");
        Directory.CreateDirectory(texFullDir);
        string texFullPath = Path.Combine(texFullDir, "ClockFaceTex.png");
        string texAssetPath = "Assets/_Project/Textures/ClockFaceTex.png";
        File.WriteAllBytes(texFullPath, BuildClockFaceTexture().EncodeToPNG());
        AssetDatabase.ImportAsset(texAssetPath);
        var tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(texAssetPath);

        // 文字盤マテリアル
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var faceMat = new Material(shader);
        faceMat.SetTexture("_BaseMap", tex2d);
        faceMat.SetColor("_BaseColor", Color.white);
        const string faceMatPath = "Assets/_Project/Materials/Generated/Mat_ClockFace.mat";
        if (File.Exists(Path.Combine(Application.dataPath, "../" + faceMatPath)))
            AssetDatabase.DeleteAsset(faceMatPath);
        AssetDatabase.CreateAsset(faceMat, faceMatPath);

        // 親オブジェクト（コライダー・インタラクタブル）
        var root = new GameObject("Clock");
        root.transform.position = new Vector3(2.0f, 2.5f, 5.88f);
        var col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(1.0f, 1.0f, 0.15f);
        root.AddComponent<EscapeGame.Game.ClockInteractable>();
        root.AddComponent<EscapeGame.Game.HoverHighlight>();

        // 木製ボディ（壁面から飛び出た時計ケース）奥から前：ボディ→文字盤→針→ボルト
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "ClockBody";
        Object.DestroyImmediate(body.GetComponent<BoxCollider>());
        body.transform.SetParent(root.transform);
        body.transform.localPosition = new Vector3(0f, 0f, -0.04f);
        body.transform.localScale = new Vector3(1.08f, 1.08f, 0.07f);
        var bodyMat = new Material(shader);
        bodyMat.SetColor("_BaseColor", new Color(0.28f, 0.16f, 0.06f));
        bodyMat.SetFloat("_Smoothness", 0.2f);
        body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;

        // 文字盤 Quad（ボディ前面）
        var face = GameObject.CreatePrimitive(PrimitiveType.Quad);
        face.name = "ClockFace";
        Object.DestroyImmediate(face.GetComponent<MeshCollider>());
        face.transform.SetParent(root.transform);
        face.transform.localPosition = new Vector3(0f, 0f, -0.08f);  // 文字盤はボディの前
        face.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        face.transform.localScale = Vector3.one;
        face.GetComponent<MeshRenderer>().sharedMaterial = faceMat;

        // 分針：6時方向（下）= Euler Z:180  ※針は文字盤より前（さらに小さいz）
        CreateHand("MinuteHand", root.transform,
            Quaternion.Euler(0f, 0f, 180f),
            length: 0.42f, width: 0.028f, zOffset: -0.13f,
            new Color(0.04f, 0.04f, 0.04f));

        // 時針：11時30分方向 = Z軸15°
        CreateHand("HourHand", root.transform,
            Quaternion.Euler(0f, 0f, 15f),
            length: 0.26f, width: 0.052f, zOffset: -0.14f,
            new Color(0.48f, 0.08f, 0.08f));

        // 中心ボルト（最前面）
        var bolt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bolt.name = "ClockBolt";
        Object.DestroyImmediate(bolt.GetComponent<CapsuleCollider>());
        bolt.transform.SetParent(root.transform);
        bolt.transform.localPosition = new Vector3(0f, 0f, -0.16f);
        bolt.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        bolt.transform.localScale = new Vector3(0.06f, 0.02f, 0.06f);
        var boltMat = new Material(shader);
        boltMat.SetColor("_BaseColor", new Color(0.35f, 0.22f, 0.08f));
        boltMat.SetFloat("_Metallic", 0.6f);
        bolt.GetComponent<MeshRenderer>().sharedMaterial = boltMat;

        EditorUtility.SetDirty(root);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Clock3D] 3D針時計に置き換え完了");
    }

    static void CreateHand(string name, Transform parent, Quaternion rotation,
                            float length, float width, float zOffset, Color color)
    {
        var pivot = new GameObject(name + "Pivot");
        pivot.transform.SetParent(parent);
        pivot.transform.localPosition = new Vector3(0f, 0f, zOffset);
        pivot.transform.localRotation = rotation;

        var hand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hand.name = name;
        Object.DestroyImmediate(hand.GetComponent<BoxCollider>());
        hand.transform.SetParent(pivot.transform);
        hand.transform.localPosition = new Vector3(0f, length * 0.5f, 0f);
        hand.transform.localRotation = Quaternion.identity;
        hand.transform.localScale = new Vector3(width, length, 0.015f);

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", 0.6f);
        hand.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    static Texture2D BuildClockFaceTexture()
    {
        const int S = 512;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        var px = new Color32[S * S];
        int cx = S / 2, cy = S / 2, r = S / 2 - 8;

        for (int i = 0; i < px.Length; i++) px[i] = new Color32(0, 0, 0, 0);
        FillCircle(px, cx, cy, r, new Color32(238, 230, 208, 255));
        for (int t = 0; t < 8; t++)
            RingCircle(px, cx, cy, r - t, new Color32(62, 36, 12, 255));

        // 時間マーカー（UV X反転しても12個の点は対称なので問題なし）
        for (int h = 0; h < 12; h++)
        {
            float a = h * 30f * Mathf.Deg2Rad;
            int mr = r - 20;
            int mx = cx + Mathf.RoundToInt(Mathf.Sin(a) * mr);
            int my = cy + Mathf.RoundToInt(Mathf.Cos(a) * mr);
            FillCircle(px, mx, my, h % 3 == 0 ? 9 : 5, new Color32(42, 26, 6, 255));
        }

        tex.SetPixels32(px);
        tex.Apply(false);
        return tex;
    }

    static void FillCircle(Color32[] px, int cx, int cy, int radius, Color32 c)
    {
        const int S = 512;
        int r2 = radius * radius;
        for (int dy = -radius; dy <= radius; dy++)
        for (int dx = -radius; dx <= radius; dx++)
        {
            if (dx * dx + dy * dy > r2) continue;
            int x = cx + dx, y = cy + dy;
            if (x >= 0 && x < S && y >= 0 && y < S) px[y * S + x] = c;
        }
    }

    static void RingCircle(Color32[] px, int cx, int cy, int radius, Color32 c)
    {
        const int S = 512;
        for (int deg = 0; deg < 720; deg++)
        {
            float rad2 = deg * Mathf.Deg2Rad * 0.5f;
            int x = cx + Mathf.RoundToInt(Mathf.Sin(rad2) * radius);
            int y = cy + Mathf.RoundToInt(Mathf.Cos(rad2) * radius);
            if (x >= 0 && x < S && y >= 0 && y < S) px[y * S + x] = c;
        }
    }
}
#endif
