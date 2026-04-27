#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class GeometryDetailSetup
{
    private const string MAT_DIR = "Assets/_Project/Materials/Generated/";

    [MenuItem("EscapeGame/Setup/Geometry Detail")]
    public static void Run()
    {
        if (Application.isPlaying) { Debug.LogError("[Geo] Edit mode で実行してください"); return; }

        SetupDesk();
        SetupBookshelf();
        SetupChair();
        SetupFireplaceDetail();
        SetupMantleDecorations();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Geo] ジオメトリ強化完了");
    }

    // ── 机：脚・引き出し追加 ─────────────────────────────────────────────
    private static void SetupDesk()
    {
        var deskTop = GameObject.Find("DeskTop");
        if (deskTop == null) { Debug.LogWarning("[Geo] DeskTop が見つかりません"); return; }

        Vector3 topPos   = deskTop.transform.position;
        Vector3 topScale = deskTop.transform.localScale;
        Color legColor   = new Color(0.30f, 0.16f, 0.07f);  // ダークウォルナット

        float legH = topPos.y - 0.5f;
        float legY = topPos.y - topScale.y * 0.5f - legH * 0.5f;

        // 4本脚
        string[] legNames = { "DeskLeg_FL", "DeskLeg_FR", "DeskLeg_BL", "DeskLeg_BR" };
        Vector3[] legOffsets = {
            new Vector3(-topScale.x * 0.44f, 0f, -topScale.z * 0.40f),
            new Vector3( topScale.x * 0.44f, 0f, -topScale.z * 0.40f),
            new Vector3(-topScale.x * 0.44f, 0f,  topScale.z * 0.40f),
            new Vector3( topScale.x * 0.44f, 0f,  topScale.z * 0.40f)
        };

        for (int i = 0; i < 4; i++)
        {
            Remove(legNames[i]);
            var leg = Cube(legNames[i],
                new Vector3(topPos.x + legOffsets[i].x, legY, topPos.z + legOffsets[i].z),
                new Vector3(0.07f, legH, 0.07f), legColor);
            EditorUtility.SetDirty(leg);
        }

        // 引き出し（右側）
        Remove("DeskDrawer_R");
        Cube("DeskDrawer_R",
            new Vector3(topPos.x + topScale.x * 0.36f, topPos.y - topScale.y * 0.5f - 0.12f, topPos.z),
            new Vector3(0.18f, 0.20f, topScale.z * 0.85f),
            new Color(0.35f, 0.18f, 0.08f));

        // 引き出しノブ
        Remove("DeskKnob_R");
        Cube("DeskKnob_R",
            new Vector3(topPos.x + topScale.x * 0.46f, topPos.y - topScale.y * 0.5f - 0.12f, topPos.z),
            new Vector3(0.03f, 0.04f, 0.04f),
            new Color(0.65f, 0.50f, 0.20f));

        Debug.Log("[Geo] 机の詳細化完了");
    }

    // ── 書棚：背板・装飾 ─────────────────────────────────────────────────
    private static void SetupBookshelf()
    {
        var bs = GameObject.Find("Bookshelf");
        if (bs == null) { Debug.LogWarning("[Geo] Bookshelf が見つかりません"); return; }

        Vector3 bsPos   = bs.transform.position;
        Vector3 bsScale = bs.transform.localScale;
        Color darkWood  = new Color(0.22f, 0.12f, 0.05f);

        // 背板（後ろに暗い板）
        Remove("Bookshelf_Back");
        Cube("Bookshelf_Back",
            new Vector3(bsPos.x, bsPos.y, bsPos.z + bsScale.z * 0.45f),
            new Vector3(bsScale.x * 0.94f, bsScale.y * 0.96f, 0.04f),
            darkWood);

        // 追加の本（色）
        Color[] bookColors = {
            new Color(0.55f, 0.22f, 0.08f),  // 茶
            new Color(0.18f, 0.35f, 0.55f),  // 紺
            new Color(0.50f, 0.45f, 0.20f),  // 黄土
            new Color(0.25f, 0.45f, 0.22f),  // 濃緑
            new Color(0.50f, 0.20f, 0.20f),  // 深赤
            new Color(0.60f, 0.48f, 0.30f),  // ベージュ
        };

        float[] shelfYs    = { 0.05f, 1.0f, 2.0f };
        float[] bookWidths = { 0.08f, 0.06f, 0.07f, 0.09f, 0.06f, 0.08f };

        int idx = 0;
        for (int s = 0; s < shelfYs.Length; s++)
        {
            float startX = bsPos.x - bsScale.x * 0.4f;
            for (int b = 0; b < 5; b++)
            {
                string bName = $"BookFiller_S{s}_B{b}";
                Remove(bName);
                float bw  = bookWidths[(idx + b) % bookWidths.Length];
                float bx  = startX + b * 0.20f + 0.04f;
                float bh  = 0.22f + (b % 3) * 0.04f;
                var book  = Cube(bName,
                    new Vector3(bx, bsPos.y - bsScale.y * 0.5f + shelfYs[s] + bh * 0.5f + 0.06f, bsPos.z - 0.05f),
                    new Vector3(bw, bh, 0.18f),
                    bookColors[(idx + b) % bookColors.Length]);
                EditorUtility.SetDirty(book);
            }
            idx += 5;
        }

        Debug.Log("[Geo] 書棚詳細化完了");
    }

    // ── 椅子：机前に配置 ─────────────────────────────────────────────────
    private static void SetupChair()
    {
        string[] parts = { "Chair_Seat", "Chair_Back", "Chair_Leg_FL", "Chair_Leg_FR", "Chair_Leg_BL", "Chair_Leg_BR" };
        foreach (var p in parts) Remove(p);

        var deskTop = GameObject.Find("DeskTop");
        if (deskTop == null) return;

        Vector3 deskPos = deskTop.transform.position;
        Vector3 chairBase = new Vector3(deskPos.x, 0f, deskPos.z - 1.0f);
        Color seatColor  = new Color(0.25f, 0.12f, 0.06f);
        Color cushion    = new Color(0.35f, 0.18f, 0.10f);

        float seatH = 0.5f;

        // 座面
        Cube("Chair_Seat",
            new Vector3(chairBase.x, seatH + 0.04f, chairBase.z),
            new Vector3(0.5f, 0.08f, 0.45f), cushion);

        // 背もたれ
        Cube("Chair_Back",
            new Vector3(chairBase.x, seatH + 0.32f, chairBase.z + 0.2f),
            new Vector3(0.48f, 0.56f, 0.05f), cushion);

        // 背もたれ横枠
        Cube("Chair_BackFrame_L",
            new Vector3(chairBase.x - 0.22f, seatH + 0.30f, chairBase.z + 0.2f),
            new Vector3(0.04f, 0.62f, 0.05f), seatColor);
        Cube("Chair_BackFrame_R",
            new Vector3(chairBase.x + 0.22f, seatH + 0.30f, chairBase.z + 0.2f),
            new Vector3(0.04f, 0.62f, 0.05f), seatColor);

        // 4本脚
        float legY = seatH * 0.5f;
        Cube("Chair_Leg_FL", new Vector3(chairBase.x - 0.20f, legY, chairBase.z - 0.18f), new Vector3(0.05f, seatH, 0.05f), seatColor);
        Cube("Chair_Leg_FR", new Vector3(chairBase.x + 0.20f, legY, chairBase.z - 0.18f), new Vector3(0.05f, seatH, 0.05f), seatColor);
        Cube("Chair_Leg_BL", new Vector3(chairBase.x - 0.20f, legY, chairBase.z + 0.20f), new Vector3(0.05f, seatH, 0.05f), seatColor);
        Cube("Chair_Leg_BR", new Vector3(chairBase.x + 0.20f, legY, chairBase.z + 0.20f), new Vector3(0.05f, seatH, 0.05f), seatColor);

        Debug.Log("[Geo] 椅子追加完了");
    }

    // ── 暖炉の詳細化 ─────────────────────────────────────────────────────
    private static void SetupFireplaceDetail()
    {
        // 薪（丸太2本）
        Remove("Log_L");
        Remove("Log_R");

        var logL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        logL.name = "Log_L";
        logL.transform.position   = new Vector3(4.2f, 0.62f, 5.55f);
        logL.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
        logL.transform.rotation   = Quaternion.Euler(0f, 0f, 90f);
        Object.DestroyImmediate(logL.GetComponent<CapsuleCollider>());
        ApplyColorMat(logL, new Color(0.25f, 0.15f, 0.08f), "Mat_Log");
        EditorUtility.SetDirty(logL);

        var logR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        logR.name = "Log_R";
        logR.transform.position   = new Vector3(4.8f, 0.68f, 5.55f);
        logR.transform.localScale = new Vector3(0.08f, 0.32f, 0.08f);
        logR.transform.rotation   = Quaternion.Euler(10f, 20f, 90f);
        Object.DestroyImmediate(logR.GetComponent<CapsuleCollider>());
        ApplyColorMat(logR, new Color(0.22f, 0.13f, 0.07f), "Mat_Log");
        EditorUtility.SetDirty(logR);

        Debug.Log("[Geo] 暖炉詳細化完了");
    }

    // ── マントル（暖炉棚）の装飾 ─────────────────────────────────────────
    private static void SetupMantleDecorations()
    {
        // 時計（置き時計）
        Remove("MantleClock");
        var mc = Cube("MantleClock",
            new Vector3(4.5f, 2.55f, 5.75f),
            new Vector3(0.18f, 0.24f, 0.10f),
            new Color(0.30f, 0.22f, 0.12f));
        EditorUtility.SetDirty(mc);

        // 置き時計のフレーム
        Remove("MantleClock_Frame");
        var mcf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mcf.name = "MantleClock_Face";
        mcf.transform.position   = new Vector3(4.5f, 2.55f, 5.70f);
        mcf.transform.localScale = new Vector3(0.12f, 0.12f, 0.04f);
        Object.DestroyImmediate(mcf.GetComponent<SphereCollider>());
        ApplyColorMat(mcf, new Color(0.88f, 0.84f, 0.72f), "Mat_ClockFace2");
        EditorUtility.SetDirty(mcf);

        // 花瓶
        Remove("Vase");
        var vase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        vase.name = "Vase";
        vase.transform.position   = new Vector3(3.6f, 2.42f, 5.75f);
        vase.transform.localScale = new Vector3(0.10f, 0.18f, 0.10f);
        Object.DestroyImmediate(vase.GetComponent<CapsuleCollider>());
        ApplyColorMat(vase, new Color(0.25f, 0.40f, 0.50f), "Mat_Vase");
        EditorUtility.SetDirty(vase);

        Debug.Log("[Geo] マントル装飾完了");
    }

    // ── ユーティリティ ───────────────────────────────────────────────────
    private static GameObject Cube(string name, Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position   = pos;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        ApplyColorMat(go, color, "Mat_" + name);
        return go;
    }

    private static void ApplyColorMat(GameObject go, Color color, string matName)
    {
        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer == null) return;
        if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);
        string path = MAT_DIR + matName + ".mat";
        if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(mat, path);
        renderer.sharedMaterial = mat;
        EditorUtility.SetDirty(go);
    }

    private static void Remove(string name)
    {
        var e = GameObject.Find(name);
        if (e != null) Object.DestroyImmediate(e);
    }
}
#endif
