#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class FireplacePhotoSetup
{
    [MenuItem("EscapeGame/Setup/Rebuild Fireplace Photo")]
    public static void Run()
    {
        if (Application.isPlaying) { Debug.LogError("[PhotoSetup] Edit mode で実行してください"); return; }

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        // 既存ストリップを削除
        Kill("Strip_Blue"); Kill("Strip_Red"); Kill("Strip_Green");

        // 既存の子ビジュアルも削除（再実行時）
        string[] oldNames = { "PhotoBG","PhotoShelf","PhotoFiller0","PhotoFiller1","PhotoFiller2","PhotoFiller3",
                              "PhotoBook_Blue","PhotoBook_Red","PhotoBook_Green","PhotoHeader" };
        foreach (var n in oldNames) Kill(n);

        var photo = GameObject.Find("FireplacePhoto");
        if (photo == null) { Debug.LogError("[PhotoSetup] FireplacePhoto が見つかりません"); return; }

        // 額縁：ルートキューブをダークウッドに
        var frameMat = new Material(shader);
        frameMat.SetColor("_BaseColor", new Color(0.18f, 0.10f, 0.04f));
        frameMat.SetFloat("_Smoothness", 0.3f);
        photo.GetComponent<MeshRenderer>().sharedMaterial = frameMat;

        var t = photo.transform;

        // ---- 写真面（セピアクリーム背景） ----
        // local z = -0.6 で額縁前面より手前に出す
        AddQuad("PhotoBG", t,
            new Vector3(0f, 0f, -0.6f),
            new Vector3(0.84f, 0.84f, 1f),
            new Color(0.91f, 0.85f, 0.70f), shader);

        // ---- ヘッダー部分（暗いセピア帯：写真の上部を暗くして奥行き感） ----
        AddQuad("PhotoHeader", t,
            new Vector3(0f, 0.28f, -0.61f),
            new Vector3(0.82f, 0.30f, 1f),
            new Color(0.55f, 0.46f, 0.34f), shader);

        // ---- 棚板（写真内の本棚の棚） ----
        // local y=-0.16 あたり。幅は写真の 80%。
        AddQuad("PhotoShelf", t,
            new Vector3(0f, -0.16f, -0.62f),
            new Vector3(0.78f, 0.04f, 1f),
            new Color(0.28f, 0.16f, 0.06f), shader);

        // ---- 本（棚の上に立てる） ----
        // local y: 棚上面 = -0.16 + 0.02 = -0.14
        // 本の高さ (local) = 0.35 → world ≈ 0.35 * 0.44 = 0.154
        // 本中心 y = -0.14 + 0.35*0.5 = -0.14 + 0.175 = 0.035

        float shelfTopY = -0.14f;

        // グレーのフィラー本（左端）
        float[] grayWidths  = { 0.07f, 0.09f, 0.06f };
        float[] grayHeights = { 0.32f, 0.28f, 0.34f };
        Color[] grayColors  = {
            new Color(0.45f, 0.40f, 0.33f),
            new Color(0.38f, 0.34f, 0.28f),
            new Color(0.50f, 0.44f, 0.35f),
        };
        float gx = -0.38f;
        for (int i = 0; i < 3; i++)
        {
            float bH = grayHeights[i];
            AddQuad($"PhotoFiller{i}", t,
                new Vector3(gx + grayWidths[i] * 0.5f, shelfTopY + bH * 0.5f, -0.63f),
                new Vector3(grayWidths[i], bH, 1f),
                grayColors[i], shader);
            gx += grayWidths[i] + 0.01f;
        }

        // パズル本3冊（青・赤・緑 ← 正解順）
        float bookW = 0.09f, bookH = 0.36f;
        float bx = -0.14f;
        Color[] bookColors = {
            new Color(0.15f, 0.30f, 0.75f), // 青
            new Color(0.72f, 0.12f, 0.12f), // 赤
            new Color(0.12f, 0.55f, 0.20f), // 緑
        };
        string[] bookGoNames = { "PhotoBook_Blue","PhotoBook_Red","PhotoBook_Green" };
        for (int i = 0; i < 3; i++)
        {
            AddQuad(bookGoNames[i], t,
                new Vector3(bx + bookW * 0.5f, shelfTopY + bookH * 0.5f, -0.64f),
                new Vector3(bookW, bookH, 1f),
                bookColors[i], shader);
            bx += bookW + 0.015f;
        }

        // グレーフィラー（右端）
        float[] grayWidths2  = { 0.08f, 0.07f };
        float[] grayHeights2 = { 0.30f, 0.33f };
        float gx2 = bx;
        for (int i = 0; i < 2; i++)
        {
            float bH = grayHeights2[i];
            AddQuad($"PhotoFiller{3+i}", t,
                new Vector3(gx2 + grayWidths2[i] * 0.5f, shelfTopY + bH * 0.5f, -0.63f),
                new Vector3(grayWidths2[i], bH, 1f),
                grayColors[i], shader);
            gx2 += grayWidths2[i] + 0.01f;
        }

        EditorUtility.SetDirty(photo);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[PhotoSetup] FireplacePhoto を再構築しました");
    }

    static void AddQuad(string name, Transform parent, Vector3 localPos, Vector3 localScale, Color color, Shader shader)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = name;
        Object.DestroyImmediate(go.GetComponent<MeshCollider>());
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale    = localScale;
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", 0.05f);
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    static void Kill(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) Object.DestroyImmediate(go);
    }
}
#endif
