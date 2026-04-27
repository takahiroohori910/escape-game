#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class VisualUpgradeSetup
{
    [MenuItem("EscapeGame/Setup/Visual Upgrade")]
    public static void Run()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("[Visual] Edit mode で実行してください");
            return;
        }

        UpgradeLighting();
        UpgradeRoom();
        UpgradeFurniture();
        UpgradeBooks();
        AddFireplaceOpening();
        AddBookshelfShelves();
        AddFireplaceLight();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Visual] ビジュアルアップグレード完了");
    }

    // ── ライティング ───────────────────────────────────────────────────────
    private static void UpgradeLighting()
    {
        var lightGO = GameObject.Find("Directional Light");
        if (lightGO == null) return;
        var light = lightGO.GetComponent<Light>();
        if (light == null) return;

        light.color     = new Color(1.0f, 0.88f, 0.72f); // 暖色アンバー
        light.intensity = 0.6f;
        lightGO.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

        // アンビエントライトも暖色に
        RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.12f, 0.10f, 0.08f);

        EditorUtility.SetDirty(lightGO);
        Debug.Log("[Visual] ディレクショナルライト調整完了");
    }

    // ── 暖炉そばのポイントライト ────────────────────────────────────────────
    private static void AddFireplaceLight()
    {
        var existing = GameObject.Find("FireplacePointLight");
        if (existing != null) Object.DestroyImmediate(existing);

        var go    = new GameObject("FireplacePointLight");
        var light = go.AddComponent<Light>();
        go.transform.position = new Vector3(4.0f, 1.5f, 5.0f);
        light.type      = LightType.Point;
        light.color     = new Color(1.0f, 0.55f, 0.15f); // オレンジ炎色
        light.intensity = 1.8f;
        light.range     = 5.0f;

        EditorUtility.SetDirty(go);
        Debug.Log("[Visual] 暖炉ポイントライト追加完了");
    }

    // ── 壁・床・天井 ────────────────────────────────────────────────────────
    private static void UpgradeRoom()
    {
        // 壁：深みのある緑がかったダークウッドパネリング
        Color wallColor    = new Color(0.22f, 0.18f, 0.14f);
        // 床：ダークオーク
        Color floorColor   = new Color(0.28f, 0.18f, 0.10f);
        // 天井：クリーム
        Color ceilingColor = new Color(0.85f, 0.82f, 0.76f);

        ApplyMat("BackWall",  wallColor,    "Mat_Wall");
        ApplyMat("LeftWall",  wallColor,    "Mat_Wall");
        ApplyMat("RightWall", wallColor,    "Mat_Wall");
        ApplyMat("Floor",     floorColor,   "Mat_Floor");
        ApplyMat("Ceiling",   ceilingColor, "Mat_Ceiling");
    }

    // ── 家具 ─────────────────────────────────────────────────────────────────
    private static void UpgradeFurniture()
    {
        Color walnut    = new Color(0.30f, 0.18f, 0.08f); // ウォルナット
        Color mahogany  = new Color(0.38f, 0.20f, 0.10f); // マホガニー
        Color stone     = new Color(0.50f, 0.48f, 0.45f); // 石
        Color darkGray  = new Color(0.15f, 0.15f, 0.15f); // 黒電話
        Color goldFrame = new Color(0.60f, 0.45f, 0.15f); // 絵画フレーム

        ApplyMat("Bookshelf",     walnut,    "Mat_Bookshelf");
        ApplyMat("DeskBody",      mahogany,  "Mat_Desk");
        ApplyMat("DeskTop",       mahogany,  "Mat_Desk");
        ApplyMat("Fireplace",     stone,     "Mat_Fireplace");
        ApplyMat("FireplaceFrame",stone,     "Mat_Fireplace");
        ApplyMat("Mantle",        mahogany,  "Mat_Mantle");
        ApplyMat("Telephone",     darkGray,  "Mat_Telephone");
        ApplyMat("Painting",      goldFrame, "Mat_Painting");
    }

    // ── 本3冊（パズルの正解色と一致） ────────────────────────────────────────
    private static void UpgradeBooks()
    {
        ApplyMat("Book_01", new Color(0.15f, 0.30f, 0.75f), "Mat_Book_Blue");  // 青
        ApplyMat("Book_02", new Color(0.72f, 0.15f, 0.15f), "Mat_Book_Red");   // 赤
        ApplyMat("Book_03", new Color(0.15f, 0.60f, 0.20f), "Mat_Book_Green"); // 緑
    }

    // ── 暖炉の炉内部（暗い開口部） ──────────────────────────────────────────
    private static void AddFireplaceOpening()
    {
        var existing = GameObject.Find("FireplaceOpening");
        if (existing != null) Object.DestroyImmediate(existing);

        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "FireplaceOpening";
        box.transform.position  = new Vector3(4.5f, 1.1f, 5.72f);
        box.transform.localScale= new Vector3(1.4f, 1.5f, 0.05f);
        Object.DestroyImmediate(box.GetComponent<BoxCollider>());
        ApplyMat(box, new Color(0.04f, 0.03f, 0.02f), "Mat_FireplaceOpening");

        EditorUtility.SetDirty(box);
        Debug.Log("[Visual] 暖炉開口部追加完了");
    }

    // ── 書棚の棚板（水平スラブ×3） ────────────────────────────────────────
    private static void AddBookshelfShelves()
    {
        // 既存の棚板削除
        foreach (var name in new[] { "Shelf_Bottom", "Shelf_Mid", "Shelf_Top" })
        {
            var e = GameObject.Find(name);
            if (e != null) Object.DestroyImmediate(e);
        }

        // Bookshelf: center (-4, 1.5, 5.7), size (2.5, 3, 0.5)
        // y range: 0 to 3 → shelves at y=0, 1.0, 2.0 relative to bottom
        float bsX = -4f, bsZ = 5.7f, bsBottom = 0f;
        float[] shelfYs = { bsBottom + 0.0f, bsBottom + 1.0f, bsBottom + 2.0f };
        string[] shelfNames = { "Shelf_Bottom", "Shelf_Mid", "Shelf_Top" };
        var shelfMat = new Color(0.25f, 0.14f, 0.06f);

        foreach (var (y, n) in new[] {
            (0.0f, "Shelf_Bottom"),
            (1.0f, "Shelf_Mid"),
            (2.0f, "Shelf_Top")
        })
        {
            var shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelf.name = n;
            shelf.transform.position   = new Vector3(bsX, y, bsZ);
            shelf.transform.localScale = new Vector3(2.5f, 0.06f, 0.5f);
            Object.DestroyImmediate(shelf.GetComponent<BoxCollider>());
            ApplyMat(shelf, shelfMat, "Mat_Shelf");
            EditorUtility.SetDirty(shelf);
        }

        Debug.Log("[Visual] 書棚棚板追加完了");
    }

    // ── ユーティリティ ───────────────────────────────────────────────────────
    private static void ApplyMat(GameObject go, Color color, string matName)
    {
        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer == null) return;

        const string dir = "Assets/_Project/Materials/Generated/";
        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

        string path = dir + matName + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            mat = new Material(shader ?? Shader.Find("Standard"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        EditorUtility.SetDirty(mat);
        renderer.sharedMaterial = mat;
        EditorUtility.SetDirty(go);
    }

    private static void ApplyMat(string goName, Color color, string matName)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"[Visual] {goName} が見つかりません"); return; }

        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer == null) return;

        const string dir = "Assets/_Project/Materials/Generated/";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string path = dir + matName + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            mat = new Material(shader ?? Shader.Find("Standard"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        EditorUtility.SetDirty(mat);
        renderer.sharedMaterial = mat;
        EditorUtility.SetDirty(go);
    }
}
#endif
