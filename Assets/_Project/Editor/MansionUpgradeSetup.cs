#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class MansionUpgradeSetup
{
    private const string TEX_DIR = "Assets/_Project/Textures/Imported/";
    private const string MAT_DIR = "Assets/_Project/Materials/Generated/";

    [MenuItem("EscapeGame/Setup/Mansion Full Upgrade")]
    public static void Run()
    {
        if (Application.isPlaying) { Debug.LogError("[Mansion] Edit mode で実行してください"); return; }

        AssetDatabase.Refresh();
        SetupNormalMaps();
        ApplyRoomMaterials();
        AddArchitecturalDetails();
        UpgradeLighting();
        AddDecorativeElements();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Mansion] 洋館ビジュアルアップグレード完了！");
    }

    // ── ノーマルマップのインポート設定 ─────────────────────────────────────
    private static void SetupNormalMaps()
    {
        string[] norMaps = {
            "herringbone_parquet_nor",
            "dark_planks_nor",
            "rock_face_nor",
            "beige_wall_001_nor",
            "worn_planks_nor"
        };
        foreach (var name in norMaps)
        {
            string path = TEX_DIR + name + ".jpg";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogWarning($"[Mansion] テクスチャ未検出: {path}"); continue; }
            if (importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
        }
        Debug.Log("[Mansion] ノーマルマップ設定完了");
    }

    // ── 部屋のマテリアル適用 ─────────────────────────────────────────────
    private static void ApplyRoomMaterials()
    {
        // 床：ヘリンボーンパーケット（4x4 タイリング）
        ApplyTex("Floor", "Mat_Floor", "herringbone_parquet",
            new Color(0.7f, 0.52f, 0.30f), 0.35f, new Vector2(4f, 4f));

        // 壁：ダーク木パネル（3x2 タイリング）
        foreach (var w in new[] { "BackWall", "LeftWall", "RightWall" })
            ApplyTex(w, "Mat_Wall", "dark_planks",
                new Color(0.52f, 0.36f, 0.20f), 0.12f, new Vector2(3f, 2f));

        // 天井：ベージュ漆喰（2x2 タイリング）
        ApplyTex("Ceiling", "Mat_Ceiling", "beige_wall_001",
            new Color(0.92f, 0.88f, 0.80f), 0.08f, new Vector2(2f, 2f));

        // 暖炉・フレーム：岩石
        foreach (var n in new[] { "Fireplace", "FireplaceFrame" })
            ApplyTex(n, "Mat_Stone", "rock_face",
                new Color(0.60f, 0.56f, 0.50f), 0.05f, new Vector2(2f, 2f));

        // 家具：暗い木材
        foreach (var n in new[] { "Bookshelf", "DeskBody", "DeskTop", "Mantle" })
            ApplyTex(n, "Mat_Wood_Dark", "worn_planks",
                new Color(0.38f, 0.20f, 0.09f), 0.20f, new Vector2(2f, 2f));

        Debug.Log("[Mansion] マテリアル適用完了");
    }

    // ── 建築ディテール：廻縁・巾木・腰壁ライン ───────────────────────────
    private static void AddArchitecturalDetails()
    {
        // クリーム色モールディング（天井際）
        Color molding  = new Color(0.88f, 0.84f, 0.76f);
        // ダークウッド巾木（床際）
        Color skirting = new Color(0.28f, 0.15f, 0.07f);
        // パネルライン
        Color panel    = new Color(0.42f, 0.25f, 0.12f);

        // 廻縁（天井際の水平ライン）
        CreateDetail("Molding_Back",  new Vector3(0f,     3.0f, 5.88f),  new Vector3(10f,  0.10f, 0.10f), molding);
        CreateDetail("Molding_Left",  new Vector3(-4.95f, 3.0f, 3.0f),   new Vector3(0.10f,0.10f, 6.2f),  molding);
        CreateDetail("Molding_Right", new Vector3( 4.95f, 3.0f, 3.0f),   new Vector3(0.10f,0.10f, 6.2f),  molding);

        // 巾木（床際）
        CreateDetail("Skirting_Back",  new Vector3(0f,     0.07f, 5.88f), new Vector3(10f,  0.14f, 0.05f), skirting);
        CreateDetail("Skirting_Left",  new Vector3(-4.97f, 0.07f, 3.0f),  new Vector3(0.05f,0.14f, 6.2f),  skirting);
        CreateDetail("Skirting_Right", new Vector3( 4.97f, 0.07f, 3.0f),  new Vector3(0.05f,0.14f, 6.2f),  skirting);

        // 腰壁ライン（壁面の高さ1.2mに横モール）
        CreateDetail("WainscotLine_Back",  new Vector3(0f,     1.22f, 5.89f), new Vector3(10f,  0.05f, 0.04f), panel);
        CreateDetail("WainscotLine_Left",  new Vector3(-4.97f, 1.22f, 3.0f),  new Vector3(0.04f,0.05f, 6.2f),  panel);
        CreateDetail("WainscotLine_Right", new Vector3( 4.97f, 1.22f, 3.0f),  new Vector3(0.04f,0.05f, 6.2f),  panel);

        // ドア枠風ライン（正面壁中央）
        CreateDetail("DoorFrame_Left",  new Vector3(-0.75f, 1.5f, 5.90f), new Vector3(0.08f, 3.0f, 0.04f), molding);
        CreateDetail("DoorFrame_Right", new Vector3( 0.75f, 1.5f, 5.90f), new Vector3(0.08f, 3.0f, 0.04f), molding);
        CreateDetail("DoorFrame_Top",   new Vector3( 0f,    3.05f, 5.90f), new Vector3(1.5f, 0.08f, 0.04f), molding);

        Debug.Log("[Mansion] 建築ディテール追加完了");
    }

    // ── 装飾オブジェクト ─────────────────────────────────────────────────
    private static void AddDecorativeElements()
    {
        // 燭台（暖炉マントル上）
        AddCandle("Candle_L", new Vector3(3.5f, 2.4f, 5.75f));
        AddCandle("Candle_R", new Vector3(5.5f, 2.4f, 5.75f));

        // 暖炉内の燠火（赤いキューブ）
        CreateFireEmber();

        Debug.Log("[Mansion] 装飾要素追加完了");
    }

    private static void AddCandle(string name, Vector3 pos)
    {
        var existing = GameObject.Find(name);
        if (existing != null) Object.DestroyImmediate(existing);

        // 本体
        var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = name;
        body.transform.position   = pos;
        body.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
        Object.DestroyImmediate(body.GetComponent<CapsuleCollider>());
        ApplyColorMat(body, new Color(0.95f, 0.92f, 0.82f), name + "_mat");

        // 炎（小さな黄色いスフィア）
        var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = name + "_Flame";
        flame.transform.position   = pos + new Vector3(0f, 0.18f, 0f);
        flame.transform.localScale = new Vector3(0.04f, 0.05f, 0.04f);
        Object.DestroyImmediate(flame.GetComponent<SphereCollider>());

        var flameRend = flame.GetComponent<MeshRenderer>();
        string matPath = MAT_DIR + name + "_FlameMat.mat";
        var flameMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        flameMat.SetColor("_BaseColor", new Color(1.0f, 0.8f, 0.1f));
        flameMat.SetColor("_EmissionColor", new Color(1.0f, 0.6f, 0.0f) * 2f);
        flameMat.EnableKeyword("_EMISSION");
        if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);
        AssetDatabase.CreateAsset(flameMat, matPath);
        flameRend.sharedMaterial = flameMat;
        EditorUtility.SetDirty(flame);
        EditorUtility.SetDirty(body);
    }

    private static void CreateFireEmber()
    {
        var existing = GameObject.Find("FireEmber");
        if (existing != null) Object.DestroyImmediate(existing);

        var ember = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ember.name = "FireEmber";
        ember.transform.position   = new Vector3(4.5f, 0.7f, 5.68f);
        ember.transform.localScale = new Vector3(0.8f, 0.12f, 0.3f);
        Object.DestroyImmediate(ember.GetComponent<BoxCollider>());

        var rend = ember.GetComponent<MeshRenderer>();
        string matPath = MAT_DIR + "Mat_FireEmber.mat";
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.SetColor("_BaseColor", new Color(0.9f, 0.3f, 0.05f));
        mat.SetColor("_EmissionColor", new Color(1.0f, 0.4f, 0.0f) * 3f);
        mat.EnableKeyword("_EMISSION");
        if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);
        AssetDatabase.CreateAsset(mat, matPath);
        rend.sharedMaterial = mat;
        EditorUtility.SetDirty(ember);
    }

    // ── ライティング ─────────────────────────────────────────────────────
    private static void UpgradeLighting()
    {
        // ディレクショナルライト：薄暗い夜嵐
        var dlGO = GameObject.Find("Directional Light");
        if (dlGO != null)
        {
            var dl = dlGO.GetComponent<Light>();
            dl.color     = new Color(0.6f, 0.7f, 0.9f); // 冷たい青みがかった月光
            dl.intensity = 0.15f;
            dlGO.transform.rotation = Quaternion.Euler(60f, -20f, 0f);
            dl.shadows   = LightShadows.Soft;
            EditorUtility.SetDirty(dlGO);
        }

        // アンビエント：暗い暖色
        RenderSettings.ambientMode  = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.08f, 0.06f, 0.05f);

        // フォグ：薄暗い密閉空間演出
        RenderSettings.fog        = true;
        RenderSettings.fogColor   = new Color(0.06f, 0.04f, 0.03f);
        RenderSettings.fogMode    = FogMode.Exponential;
        RenderSettings.fogDensity = 0.025f;

        // 暖炉ポイントライト
        SetupPointLight("FireplacePointLight", new Vector3(4.0f, 1.0f, 5.0f),
            new Color(1.0f, 0.45f, 0.08f), 3.0f, 6.5f);

        // デスクランプ（机の上）
        SetupPointLight("DeskLampLight", new Vector3(0f, 1.6f, 0.5f),
            new Color(0.9f, 0.78f, 0.5f), 1.5f, 4.0f);

        // 燭台左ライト
        SetupPointLight("CandleLight_L", new Vector3(3.5f, 2.5f, 5.5f),
            new Color(1.0f, 0.7f, 0.2f), 0.8f, 2.5f);

        // 燭台右ライト
        SetupPointLight("CandleLight_R", new Vector3(5.5f, 2.5f, 5.5f),
            new Color(1.0f, 0.7f, 0.2f), 0.8f, 2.5f);

        Debug.Log("[Mansion] ライティング改善完了（月光＋暖炉＋燭台）");
    }

    private static void SetupPointLight(string name, Vector3 pos, Color color, float intensity, float range)
    {
        var go = GameObject.Find(name);
        if (go == null) { go = new GameObject(name); go.AddComponent<Light>(); }
        var light = go.GetComponent<Light>();
        go.transform.position = pos;
        light.type      = LightType.Point;
        light.color     = color;
        light.intensity = intensity;
        light.range     = range;
        light.shadows   = LightShadows.Soft;
        EditorUtility.SetDirty(go);
    }

    // ── ユーティリティ ───────────────────────────────────────────────────
    private static void ApplyTex(string goName, string matName, string texBase, Color tint, float smoothness, Vector2 tiling)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"[Mansion] {goName} が見つかりません"); return; }

        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer == null) return;

        if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);
        string matPath = MAT_DIR + matName + ".mat";

        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, matPath);
        }

        var diff = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + texBase + "_diff.jpg");
        var nor  = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_DIR + texBase + "_nor.jpg");

        if (diff != null)
        {
            mat.SetTexture("_BaseMap", diff);
            mat.SetTextureScale("_BaseMap", tiling);
        }
        if (nor != null)
        {
            mat.SetTexture("_BumpMap", nor);
            mat.SetTextureScale("_BumpMap", tiling);
            mat.EnableKeyword("_NORMALMAP");
        }
        mat.SetFloat("_BumpScale", 1.2f);
        mat.SetColor("_BaseColor", tint);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", 0f);

        EditorUtility.SetDirty(mat);
        renderer.sharedMaterial = mat;
        EditorUtility.SetDirty(go);
    }

    private static void ApplyColorMat(GameObject go, Color color, string matName)
    {
        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer == null) return;
        if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);
        string matPath = MAT_DIR + matName + ".mat";
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(mat, matPath);
        renderer.sharedMaterial = mat;
        EditorUtility.SetDirty(go);
    }

    private static void CreateDetail(string name, Vector3 pos, Vector3 scale, Color color)
    {
        var existing = GameObject.Find(name);
        if (existing != null) Object.DestroyImmediate(existing);

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position   = pos;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        ApplyColorMat(go, color, "Mat_" + name);
        EditorUtility.SetDirty(go);
    }
}
#endif
