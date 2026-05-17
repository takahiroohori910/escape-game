#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace EscapeGame.EditorTools
{
    // Room1 の壁に風景画（本棚パズルのヒント）を配置する。
    // チェスト上の壁 (X=4.0, Y=2.5, Z=5.87) に額縁付きで吊るす。
    // 風景画の左から「赤・緑・青」が順に配置されているので、本棚の正解順 (赤→緑→青) を間接的に示す。
    public static class PaintingSetup
    {
        const string TEX_PATH   = "Assets/_Project/Textures/Painting_Landscape.png";
        const string MAT_DIR    = "Assets/_Project/Materials/Generated";
        const string ROOT_NAME  = "LandscapePainting";

        [MenuItem("EscapeGame/Setup/Build Landscape Painting")]
        public static void Build()
        {
            EnsureDir(MAT_DIR);

            // 既存があれば破棄
            var prev = GameObject.Find(ROOT_NAME);
            if (prev != null) Object.DestroyImmediate(prev);

            // テクスチャインポート設定を Default → 圧縮維持で読み込む
            var imp = AssetImporter.GetAtPath(TEX_PATH) as TextureImporter;
            if (imp != null)
            {
                imp.textureType   = TextureImporterType.Default;
                imp.mipmapEnabled = false;
                imp.filterMode    = FilterMode.Bilinear;
                imp.SaveAndReimport();
            }
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_PATH);
            if (tex == null)
            {
                Debug.LogError($"[PaintingSetup] テクスチャ未検出: {TEX_PATH}");
                return;
            }

            // 絵本体マテリアル（URP/Lit、両面描画＋自己発光で暗い部屋でも視認しやすく）
            var canvasMat = LoadOrCreateMat("Mat_Painting_Landscape", Color.white, 0.05f);
            canvasMat.SetTexture("_BaseMap", tex);
            canvasMat.SetFloat("_Cull", 0f); // Off = 両面描画
            canvasMat.SetTexture("_EmissionMap", tex);
            canvasMat.SetColor("_EmissionColor", new Color(1.4f, 1.4f, 1.4f, 1f));
            canvasMat.EnableKeyword("_EMISSION");
            canvasMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(canvasMat);

            // 額縁マテリアル（金色っぽい木枠）
            var frameMat = LoadOrCreateMat("Mat_Painting_Frame", new Color(0.55f, 0.40f, 0.18f), 0.25f);

            // ルート：絵の下端がチェスト天板 Y=2.36 にちょうど乗る高さ (0.7倍縮小後の高さ 0.665 の半分 = 0.33 上が中心)
            // 中心 Y=2.36+0.33=2.69 で絵の下端がチェスト天板に接地する
            var root = new GameObject(ROOT_NAME);
            root.transform.SetPositionAndRotation(new Vector3(4.0f, 2.69f, 5.0f),
                                                  Quaternion.Euler(10f, 0f, 0f));
            root.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

            // 額縁（外枠の厚みあるパネル）
            var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            Object.DestroyImmediate(frame.GetComponent<BoxCollider>());
            frame.transform.SetParent(root.transform, false);
            frame.transform.localPosition = new Vector3(0f, 0f, 0.02f);
            frame.transform.localScale    = new Vector3(1.40f, 0.95f, 0.04f);
            frame.GetComponent<MeshRenderer>().sharedMaterial = frameMat;

            // 絵本体（Quad、部屋手前向き）
            var canvas = GameObject.CreatePrimitive(PrimitiveType.Quad);
            canvas.name = "Canvas";
            Object.DestroyImmediate(canvas.GetComponent<MeshCollider>());
            canvas.transform.SetParent(root.transform, false);
            canvas.transform.localPosition = new Vector3(0f, 0f, -0.01f);
            canvas.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            canvas.transform.localScale    = new Vector3(1.30f, 0.85f, 1f);
            canvas.GetComponent<MeshRenderer>().sharedMaterial = canvasMat;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[PaintingSetup] 風景画を配置しました (4.0, 2.5, 5.87)");
        }

        static Material LoadOrCreateMat(string fileName, Color baseColor, float smoothness)
        {
            string path = $"{MAT_DIR}/{fileName}.mat";
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            var mat = existing != null ? existing : new Material(shader);
            mat.shader = shader;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic",   0f);
            if (existing == null) AssetDatabase.CreateAsset(mat, path);
            else EditorUtility.SetDirty(mat);
            return mat;
        }

        static void EnsureDir(string assetDir)
        {
            if (AssetDatabase.IsValidFolder(assetDir)) return;
            string parent = System.IO.Path.GetDirectoryName(assetDir);
            string leaf   = System.IO.Path.GetFileName(assetDir);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
