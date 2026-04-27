#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class BookshelfRebuildSetup
{
    [MenuItem("EscapeGame/Setup/Rebuild Bookshelf")]
    public static void Run()
    {
        if (Application.isPlaying) { Debug.LogError("[BSRebuild] Edit mode で実行してください"); return; }

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        // ---- 既存ビジュアルを削除 ----
        string[] old = {
            "Bookshelf_Back","Shelf_Bottom","Shelf_Mid","Shelf_Top",
            "BS_Back","BS_Left","BS_Right","BS_Top","BS_Bottom",
            "BS_Shelf1","BS_Shelf2"
        };
        foreach (var n in old) Kill(n);
        for (int s = 0; s <= 2; s++)
            for (int b = 0; b <= 9; b++) { Kill($"BookFiller_S{s}_B{b}"); Kill($"S{s}_Filler_{b}"); }
        for (int i = 0; i < 60; i++) { Kill($"S0F{i}"); Kill($"S1LF{i}"); Kill($"S1RF{i}"); Kill($"S2F{i}"); }

        // Bookshelf 本体 MeshRenderer を非表示（コライダー・スクリプトは残す）
        var bsGO = GameObject.Find("Bookshelf");
        if (bsGO != null)
        {
            var mr = bsGO.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;
        }

        // ---- 色 ----
        Color darkWood = new Color(0.18f, 0.10f, 0.04f);   // ダークウォールナット
        Color shelfWood = new Color(0.26f, 0.15f, 0.06f);  // 棚板（やや明るめ）

        // ---- フレーム ----
        // 本棚内部: x -5.17 〜 -2.83, y 0 〜 3.0, 棚前面 z=5.50
        Box("BS_Back",   -4f,    1.5f,  5.90f, 2.60f, 3.06f, 0.04f, darkWood,  0.08f, shader);
        Box("BS_Left",  -5.27f,  1.5f,  5.68f, 0.10f, 3.06f, 0.44f, darkWood,  0.15f, shader);
        Box("BS_Right", -2.73f,  1.5f,  5.68f, 0.10f, 3.06f, 0.44f, darkWood,  0.15f, shader);
        Box("BS_Top",    -4f,    3.05f, 5.68f, 2.70f, 0.10f, 0.44f, darkWood,  0.15f, shader);
        Box("BS_Bottom", -4f,   -0.05f, 5.68f, 2.70f, 0.10f, 0.44f, darkWood,  0.15f, shader);
        Box("BS_Shelf1", -4f,    1.0f,  5.68f, 2.44f, 0.07f, 0.38f, shelfWood, 0.20f, shader);
        Box("BS_Shelf2", -4f,    2.0f,  5.68f, 2.44f, 0.07f, 0.38f, shelfWood, 0.20f, shader);

        // ---- セクション定義 ----
        // 内部 x: -5.17 〜 -2.83 (幅 2.34)
        // 本前面 z ≈ 5.50
        float z = 5.50f;
        float xL = -5.17f, xR = -2.83f;
        float s0Bot = 0.02f,  s0Top = 0.97f;   // 下段
        float s1Bot = 1.07f,  s1Top = 1.97f;   // 中段（パズル本）
        float s2Bot = 2.07f,  s2Top = 2.97f;   // 上段

        // ---- パズル本を中段に配置 ----
        float pW = 0.14f, pH = Mathf.Min(0.72f, s1Top - s1Bot - 0.04f), pD = 0.16f;
        float pY = s1Bot + pH * 0.5f;
        PlaceBook("Book_01", -4.60f, pY, z, pW, pH, pD);
        PlaceBook("Book_02", -4.00f, pY, z, pW, pH, pD);
        PlaceBook("Book_03", -3.40f, pY, z, pW, pH, pD);

        // ---- フィラー本 ----
        Color[] pal =
        {
            new Color(0.44f,0.13f,0.07f), // 深赤
            new Color(0.10f,0.20f,0.42f), // ダークブルー
            new Color(0.08f,0.27f,0.13f), // ダークグリーン
            new Color(0.36f,0.25f,0.07f), // マスタード
            new Color(0.38f,0.17f,0.27f), // モーブ
            new Color(0.46f,0.31f,0.10f), // 焦げ茶
            new Color(0.12f,0.29f,0.34f), // ティール
            new Color(0.34f,0.09f,0.09f), // バーガンディ
            new Color(0.52f,0.43f,0.18f), // タン
            new Color(0.27f,0.27f,0.34f), // スレート
            new Color(0.50f,0.27f,0.08f), // テラコッタ
            new Color(0.14f,0.14f,0.28f), // インディゴ
        };

        // 下段：全幅フィラー
        FillRow("S0F", xL, xR, s0Bot, s0Top, z, shader, pal, 0);
        // 中段：パズル本の左右にフィラー
        FillRow("S1LF", xL, -4.60f - pW * 0.5f - 0.01f, s1Bot, s1Top, z, shader, pal, 11);
        FillRow("S1RF", -3.40f + pW * 0.5f + 0.01f, xR, s1Bot, s1Top, z, shader, pal, 23);
        // 上段：全幅フィラー
        FillRow("S2F", xL, xR, s2Bot, s2Top, z, shader, pal, 35);

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[BSRebuild] 本棚を再構築しました");
    }

    // 本棚フレーム板を作る（コライダーなし）
    static void Box(string name, float x, float y, float z,
                    float sx, float sy, float sz,
                    Color col, float smooth, Shader shader)
    {
        Kill(name);
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        go.transform.position  = new Vector3(x, y, z);
        go.transform.localScale = new Vector3(sx, sy, sz);
        SetMat(go, col, smooth, shader);
    }

    // パズル本を再配置（コライダー・スクリプトは保持、マテリアルはURP Litに）
    static void PlaceBook(string name, float x, float y, float z,
                          float sx, float sy, float sz)
    {
        var go = GameObject.Find(name);
        if (go == null) return;
        go.transform.position   = new Vector3(x, y, z);
        go.transform.localScale = new Vector3(sx, sy, sz);
        // マテリアルは BookInteractable が MaterialPropertyBlock で上書きするので変更不要
        EditorUtility.SetDirty(go);
    }

    // 棚1段分に本を詰める（幅・高さを疑似ランダムに変化）
    static void FillRow(string prefix, float xStart, float xEnd,
                        float yBot, float yTop, float z,
                        Shader shader, Color[] pal, int seed)
    {
        float secH = yTop - yBot;
        float x = xStart;
        int idx = 0;
        while (x < xEnd - 0.04f && idx < 50)
        {
            int h = (seed + idx * 7) % 11;
            int w = (seed + idx * 13) % 9;
            float bH = secH * (0.68f + h * 0.03f);   // 68〜98%の高さ
            float bW = 0.055f + w * 0.010f;            // 0.055〜0.135 幅
            if (x + bW > xEnd) bW = xEnd - x;
            if (bW < 0.03f) break;

            Color c = pal[(seed + idx * 5) % pal.Length];
            Box($"{prefix}{idx}", x + bW * 0.5f, yBot + bH * 0.5f, z,
                bW, bH, 0.15f, c, 0.12f, shader);
            x += bW + 0.004f;
            idx++;
        }
    }

    static void SetMat(GameObject go, Color col, float smooth, Shader shader)
    {
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", col);
        mat.SetFloat("_Smoothness", smooth);
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    static void Kill(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) Object.DestroyImmediate(go);
    }
}
#endif
