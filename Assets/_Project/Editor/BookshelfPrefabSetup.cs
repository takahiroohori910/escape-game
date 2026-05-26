#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using EscapeGame.Core;

namespace EscapeGame.EditorTools
{
    // QA_Books のプレハブを用いて本棚を構築する。
    // - パズル本3冊（Book_01/02/03）はメッシュだけ差し替え、機能を維持
    // - 充填本は QA_Books の Books_XX（複合本）と Book_XX（単体本）で配置
    // - 既存の Cube ベース充填本（S0F*等）を削除して置き換え
    // - 既存の Rebuild Bookshelf メニューは温存
    public static class BookshelfPrefabSetup
    {
        const string QA_PREFAB_DIR = "Assets/QA_Books/Prefabs";
        const string QA_TEX_DIR    = "Assets/QA_Books/Textures";

        // パズル本3冊に使うメッシュ Prefab（単体本から選定）
        const string PUZZLE_BOOK_PREFAB = "Assets/QA_Books/Prefabs/Book_01.prefab";

        [MenuItem("EscapeGame/Setup/Rebuild Bookshelf With Prefabs")]
        public static void Run()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("[BSPrefab] Edit mode で実行してください");
                return;
            }

            // 1. 古い Cube ベース充填本を削除
            DeleteOldFillers();

            // 2. パズル本3冊のメッシュを差し替え
            ReplacePuzzleBookMesh("Book_01");
            ReplacePuzzleBookMesh("Book_02");
            ReplacePuzzleBookMesh("Book_03");

            // 3. 充填本を Prefab で配置
            PlaceFillerBooks();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[BSPrefab] 本棚をプレハブで再構築しました");
        }

        // QA_Books の Standard シェーダーマテリアルを URP/Lit に変換する。
        // 対象は QA_Books/Models/Materials/ 配下の 2 個のみ。
        // 他の Standard マテリアル（Furniture Mega Pack 等）は触らない。
        [MenuItem("EscapeGame/Setup/Convert QA_Books Materials to URP")]
        public static void ConvertQABooksMaterials()
        {
            string[] paths =
            {
                "Assets/QA_Books/Models/Materials/Books_mtl.mat",
                "Assets/QA_Books/Models/Materials/BooksA_mtl.mat",
            };
            int converted = 0;
            foreach (var path in paths)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) { Debug.LogWarning($"[BSPrefab] {path} 未検出"); continue; }
                ConvertStandardToURPLit(mat);
                EditorUtility.SetDirty(mat);
                converted++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[BSPrefab] QA_Books マテリアル {converted} 個を URP/Lit に変換");
        }

        static void ConvertStandardToURPLit(Material mat)
        {
            // Standard プロパティを退避してから URP/Lit シェーダーに切り替え
            var mainTex   = mat.HasProperty("_MainTex")        ? mat.GetTexture("_MainTex")        : null;
            var color     = mat.HasProperty("_Color")          ? mat.GetColor("_Color")            : Color.white;
            var bumpMap   = mat.HasProperty("_BumpMap")        ? mat.GetTexture("_BumpMap")        : null;
            var metallic  = mat.HasProperty("_Metallic")       ? mat.GetFloat("_Metallic")         : 0f;
            var smooth    = mat.HasProperty("_Glossiness")     ? mat.GetFloat("_Glossiness")       : 0.2f;
            var occlMap   = mat.HasProperty("_OcclusionMap")   ? mat.GetTexture("_OcclusionMap")   : null;
            var emitMap   = mat.HasProperty("_EmissionMap")    ? mat.GetTexture("_EmissionMap")    : null;
            var emitColor = mat.HasProperty("_EmissionColor")  ? mat.GetColor("_EmissionColor")    : Color.black;

            mat.shader = Shader.Find("Universal Render Pipeline/Lit");

            if (mainTex != null) mat.SetTexture("_BaseMap", mainTex);
            mat.SetColor("_BaseColor", color);
            if (bumpMap != null) mat.SetTexture("_BumpMap", bumpMap);
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smooth);
            if (occlMap != null) mat.SetTexture("_OcclusionMap", occlMap);
            if (emitMap != null)
            {
                mat.SetTexture("_EmissionMap", emitMap);
                mat.SetColor("_EmissionColor", emitColor);
                mat.EnableKeyword("_EMISSION");
            }
        }

        // S2_01 だけ残して他の新規追加本（S0_/S1L_/S1R_/S2_）を全削除し、
        // S2_01 を Y軸 180度回転（表裏逆だったため）
        [MenuItem("EscapeGame/Setup/Cleanup Books (Keep S2_01 Only)")]
        public static void CleanupKeepS201()
        {
            int removed = 0;
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var toDelete = new List<GameObject>();
            foreach (var root in scene.GetRootGameObjects())
                CollectByPrefix(root.transform, toDelete,
                    "S0_", "S1L_", "S1R_", "S2_", "S0Stack", "S2Stack");
            // S2_01 は保護
            toDelete.RemoveAll(go => go.name == "S2_01");
            foreach (var go in toDelete) { Object.DestroyImmediate(go); removed++; }

            var keep = GameObject.Find(SceneNames.S2_01);
            if (keep != null)
                keep.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[BSPrefab] {removed} 個の本を削除、S2_01 を Y=180° 回転");
        }

        // 同名 S2_01 が複数存在する場合、Y=180° 回転済みでないものを削除
        [MenuItem("EscapeGame/Setup/Delete Extra S2_01")]
        public static void DeleteExtraS201()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var found = new List<GameObject>();
            foreach (var root in scene.GetRootGameObjects())
                FindAllByName(root.transform, "S2_01", found);

            Debug.Log($"[BSPrefab] S2_01 が {found.Count} 個 見つかりました");
            foreach (var go in found)
                Debug.Log($"  - pos={go.transform.position} rot={go.transform.eulerAngles} scale={go.transform.localScale}");

            // Y rotation が 180° 近傍のものは保護、それ以外を削除
            int deleted = 0;
            foreach (var go in found)
            {
                float yDelta = Mathf.Abs(Mathf.DeltaAngle(go.transform.eulerAngles.y, 180f));
                if (yDelta > 1f) { Object.DestroyImmediate(go); deleted++; }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[BSPrefab] {deleted} 個の S2_01 を削除");
        }

        static void FindAllByName(Transform t, string name, List<GameObject> result)
        {
            if (t.name == name) result.Add(t.gameObject);
            foreach (Transform c in t) FindAllByName(c, name, result);
        }

        // 本棚関連オブジェクト全部を X 方向に一括シフトする（壁めり込み解消用）
        [MenuItem("EscapeGame/Setup/Move Bookshelf Right (+0.5m)")]
        public static void MoveBookshelfRight()
        {
            string[] names =
            {
                "Bookshelf",
                "BS_Back", "BS_Left", "BS_Right", "BS_Top", "BS_Bottom",
                "BS_Shelf1", "BS_Shelf2",
                "Book_01", "Book_02", "Book_03",
                "S2_01",
            };
            const float DX = 0.5f;
            int moved = 0;
            foreach (var n in names)
            {
                var go = GameObject.Find(n);
                if (go == null) continue;
                go.transform.position += new Vector3(DX, 0f, 0f);
                moved++;
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[BSPrefab] 本棚関連 {moved} 個を X+{DX} 移動");
        }

        // S2_01 を雛形に各段（下・中・上）へ横方向に複製して棚を埋める
        // 中段はパズル本3冊を避けて配置
        [MenuItem("EscapeGame/Setup/Fill Shelves With S2_01 Copies")]
        public static void FillShelvesWithS201()
        {
            var src = GameObject.Find(SceneNames.S2_01);
            if (src == null) { Debug.LogError("[BSPrefab] S2_01 が見つかりません"); return; }
            var mr = src.GetComponent<MeshRenderer>();
            if (mr == null) return;

            float bookW = mr.bounds.size.x;
            float spacing = bookW + 0.03f;

            // 棚内寸（BS_Left/BS_Right から動的に取得して、Move Bookshelf Right 実行後でも動く）
            var bsL = GameObject.Find(SceneNames.BS_Left);
            var bsR = GameObject.Find(SceneNames.BS_Right);
            if (bsL == null || bsR == null) { Debug.LogError("[BSPrefab] BS_Left/BS_Right 未検出"); return; }
            float xMin = bsL.transform.position.x + 0.10f;
            float xMax = bsR.transform.position.x - 0.10f;

            // 段ごとの Y 位置（棚板の上面）。BS_Shelf1/2 と BS_Bottom から動的に取得
            var bsBottom = GameObject.Find(SceneNames.BS_Bottom);
            var bsShelf1 = GameObject.Find(SceneNames.BS_Shelf1);
            var bsShelf2 = GameObject.Find(SceneNames.BS_Shelf2);
            float yLower = bsBottom != null ? bsBottom.transform.position.y + 0.05f : 0f;
            float yMiddle = bsShelf1 != null ? bsShelf1.transform.position.y + 0.04f : 1.035f;
            float yUpper  = bsShelf2 != null ? bsShelf2.transform.position.y + 0.04f : 2.035f;

            float z = src.transform.position.z;
            int created = 0;
            int deleted = 0;

            // 既存の複製本を消してから再生成（冪等性）
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var toDelete = new List<GameObject>();
            foreach (var root in scene.GetRootGameObjects())
                CollectByPrefix(root.transform, toDelete, "S2_01_");
            foreach (var go in toDelete) { Object.DestroyImmediate(go); deleted++; }

            // 強引に各段 2冊配置（左端・右端）。中段はパズル本3冊を保護するため複製本配置しない
            float xLeft  = xMin + bookW * 0.5f;
            float xRight = xMax - bookW * 0.5f;
            float[] xPositions = { xLeft, xRight };

            foreach (var y in new[] { yLower, yUpper })
            {
                int idx = 0;
                foreach (var x in xPositions)
                {
                    // 上段は既存 S2_01 と同位置スキップ
                    if (y > 1.5f &&
                        Mathf.Abs(x - src.transform.position.x) < bookW * 0.4f)
                    { idx++; continue; }

                    var dup = Object.Instantiate(src);
                    dup.name = $"S2_01_y{(y * 10):F0}_{idx:D2}";
                    dup.transform.position = new Vector3(x, y, z);
                    dup.transform.rotation = src.transform.rotation;
                    dup.transform.localScale = src.transform.localScale;
                    created++;
                    idx++;
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[BSPrefab] 既存複製 {deleted} 個削除、{created} 個の S2_01 を複製配置");
        }

        static bool IsNearPuzzleBook(float x, float bookW)
        {
            // パズル本3冊の x 位置（Book_01: -4.6, Book_02: -4.0, Book_03: -3.4、Move 実行後は +0.5）
            var positions = new[] { "Book_01", "Book_02", "Book_03" };
            foreach (var n in positions)
            {
                var go = GameObject.Find(n);
                if (go == null) continue;
                if (Mathf.Abs(x - go.transform.position.x) < bookW * 0.55f) return true;
            }
            return false;
        }

        // パズル本3冊を元の細長い Cube メッシュに戻す（並べ替えパズル機能維持）
        // メッシュ差し替え（QA Books の Book_01）から、元の単純な Cube + 色マテリアルに戻す
        [MenuItem("EscapeGame/Setup/Reset Puzzle Books to Cube")]
        public static void ResetPuzzleBooksToCube()
        {
            // Unity 内蔵 Cube メッシュを取得
            var tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var cubeMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(tempCube);

            // Move Bookshelf Right (+0.5) を考慮した位置（元 -4.6, -4.0, -3.4 → +0.5 で -4.1, -3.5, -2.9）
            var books = new (string name, string mat, float x)[]
            {
                ("Book_01", "Mat_Book_Red",   -4.10f),
                ("Book_02", "Mat_Book_Blue",  -3.50f),
                ("Book_03", "Mat_Book_Green", -2.90f),
            };

            foreach (var (name, matName, x) in books)
            {
                var go = GameObject.Find(name);
                if (go == null) continue;

                // メッシュを Cube に戻す
                var mf = go.GetComponent<MeshFilter>();
                if (mf != null) mf.sharedMesh = cubeMesh;

                // 位置・回転・scale を元の細長い Cube サイズに戻す
                go.transform.position    = new Vector3(x, 1.43f, 5.5f);
                go.transform.rotation    = Quaternion.identity;
                go.transform.localScale  = new Vector3(0.14f, 0.72f, 0.16f);

                // マテリアルを色付き Cube 用に再設定
                var mr = go.GetComponent<MeshRenderer>();
                var mat = AssetDatabase.LoadAssetAtPath<Material>(
                    $"Assets/_Project/Materials/Generated/{matName}.mat");
                if (mat != null && mr != null) mr.sharedMaterial = mat;

                // BoxCollider を Cube 標準に
                var bc = go.GetComponent<BoxCollider>();
                if (bc != null) { bc.center = Vector3.zero; bc.size = Vector3.one; }

                EditorUtility.SetDirty(go);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[BSPrefab] パズル本3冊を Cube メッシュに戻しました");
        }

        [MenuItem("EscapeGame/Setup/Optimize QA_Books Textures to 2K")]
        public static void OptimizeTextures()
        {
            if (!AssetDatabase.IsValidFolder(QA_TEX_DIR))
            {
                Debug.LogWarning($"[BSPrefab] {QA_TEX_DIR} が見つかりません");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { QA_TEX_DIR });
            int changed = 0;
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null) continue;
                if (imp.maxTextureSize > 2048)
                {
                    imp.maxTextureSize = 2048;
                    imp.SaveAndReimport();
                    changed++;
                }
            }
            Debug.Log($"[BSPrefab] {changed} 個のテクスチャを 2K に最適化しました");
        }

        // 既存 Cube ベース充填本を削除（S0F* / S1LF* / S1RF* / S2F* / *Stack* など）
        static void DeleteOldFillers()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var toDelete = new List<GameObject>();
            foreach (var root in scene.GetRootGameObjects())
                CollectByPrefix(root.transform, toDelete,
                    "S0F", "S1LF", "S1RF", "S2F", "S0Stack", "S2Stack", "PrefabBook_");
            foreach (var go in toDelete)
                Object.DestroyImmediate(go);
            Debug.Log($"[BSPrefab] 古い充填本 {toDelete.Count} 個を削除");
        }

        static void CollectByPrefix(Transform t, List<GameObject> result, params string[] prefixes)
        {
            foreach (var p in prefixes)
            {
                if (t.name.StartsWith(p)) { result.Add(t.gameObject); return; }
            }
            // 再帰：子も検索（ただし、親が削除対象なら子は skip）
            foreach (Transform c in t)
                CollectByPrefix(c, result, prefixes);
        }

        // パズル本のメッシュだけを Prefab から差し替え（BookInteractable・色マテリアル維持）
        static void ReplacePuzzleBookMesh(string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PUZZLE_BOOK_PREFAB);
            if (prefab == null)
            {
                Debug.LogError($"[BSPrefab] Prefab 未検出: {PUZZLE_BOOK_PREFAB}");
                return;
            }
            var prefabMf = prefab.GetComponent<MeshFilter>();
            if (prefabMf == null || prefabMf.sharedMesh == null)
            {
                Debug.LogError($"[BSPrefab] Prefab に MeshFilter/Mesh がありません");
                return;
            }

            var sceneGo = GameObject.Find(name);
            if (sceneGo == null) return;

            var mf = sceneGo.GetComponent<MeshFilter>();
            if (mf == null) mf = sceneGo.AddComponent<MeshFilter>();
            mf.sharedMesh = prefabMf.sharedMesh;

            // 棚一段に合うように scale を計算（充填本と同じ目標高さ）
            float s = ComputeUniformScale(prefab, TARGET_BOOK_HEIGHT);
            sceneGo.transform.localScale = new Vector3(s, s, s);

            // BoxCollider を Mesh bounds に合わせる
            var bc = sceneGo.GetComponent<BoxCollider>();
            if (bc != null)
            {
                bc.center = prefabMf.sharedMesh.bounds.center;
                bc.size   = prefabMf.sharedMesh.bounds.size;
            }
            EditorUtility.SetDirty(sceneGo);
        }

        // 充填本を棚の3段に配置する。複合本（Books_XX）で棚を一気に埋める方針
        static void PlaceFillerBooks()
        {
            // 棚の内寸（既存 BookshelfRebuildSetup と合わせる）
            const float xL = -5.17f, xR = -2.83f, z = 5.50f;
            const float s0Y = 0.05f;  // 下段ベース
            const float s1Y = 1.10f;  // 中段ベース（パズル本配置レベル）
            const float s2Y = 2.10f;  // 上段ベース

            // 複合本（Books_XX）の Prefab パス候補。実際に存在するもののみ使用
            var comboPrefabs = LoadPrefabsByPrefix("Books_", 33);
            var singlePrefabs = LoadPrefabsByPrefix("Book_", 67);
            if (comboPrefabs.Count == 0 && singlePrefabs.Count == 0)
            {
                Debug.LogError("[BSPrefab] QA_Books Prefab が読み込めません");
                return;
            }

            // 上段：複合本でリッチ装飾
            FillShelfWithCombo("S2_", xL, xR, s2Y, z, comboPrefabs, 17);
            // 下段：複合本ベース
            FillShelfWithCombo("S0_", xL, xR, s0Y, z, comboPrefabs, 7);
            // 中段：パズル本の左右を単体本で埋める（識別性確保のため抑え気味）
            FillShelfWithSingles("S1L_", xL, -4.60f - 0.10f, s1Y, z, singlePrefabs, 11);
            FillShelfWithSingles("S1R_", -3.40f + 0.10f, xR, s1Y, z, singlePrefabs, 23);
        }

        static List<GameObject> LoadPrefabsByPrefix(string prefix, int maxNumber)
        {
            var result = new List<GameObject>();
            for (int i = 1; i <= maxNumber; i++)
            {
                string path = $"{QA_PREFAB_DIR}/{prefix}{i:D2}.prefab";
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (p != null) result.Add(p);
            }
            return result;
        }

        // 目標：棚一段の本の高さ。Prefab メッシュサイズから scale を計算する基準
        const float TARGET_BOOK_HEIGHT = 0.65f;

        // Prefab を「目標高さ」になるように scale を計算
        static float ComputeUniformScale(GameObject prefab, float targetHeight)
        {
            var mf = prefab.GetComponentInChildren<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) return 0.3f;
            float meshH = mf.sharedMesh.bounds.size.y;
            if (meshH < 0.001f) return 0.3f;
            return targetHeight / meshH;
        }

        // 複合本（Books_XX）を1段に並べる。メッシュサイズから scale を計算
        static void FillShelfWithCombo(string prefix, float xL, float xR, float yBase, float z,
                                        List<GameObject> prefabs, int seed)
        {
            if (prefabs.Count == 0) return;
            float x = xL;
            int idx = 0;
            while (x < xR - 0.05f && idx < 20)
            {
                var p = prefabs[(seed + idx * 5) % prefabs.Count];
                float s = ComputeUniformScale(p, TARGET_BOOK_HEIGHT);
                var go = (GameObject)PrefabUtility.InstantiatePrefab(p);
                go.name = $"{prefix}{idx:D2}";
                go.transform.localScale = new Vector3(s, s, s);
                go.transform.position = new Vector3(x, yBase, z);
                go.transform.rotation = Quaternion.identity;

                // 配置後の実幅で次の x を計算
                var mr = go.GetComponentInChildren<MeshRenderer>();
                float w = (mr != null) ? mr.bounds.size.x : 0.20f;
                // 位置 x を「本の左端」基準に揃える
                float minX = (mr != null) ? mr.bounds.min.x : x;
                float shift = x - minX;
                go.transform.position += new Vector3(shift, 0, 0);
                x += w + 0.005f;
                idx++;
            }
        }

        // 単体本を1段に並べる
        static void FillShelfWithSingles(string prefix, float xL, float xR, float yBase, float z,
                                         List<GameObject> prefabs, int seed)
        {
            if (prefabs.Count == 0) return;
            float x = xL;
            int idx = 0;
            while (x < xR - 0.03f && idx < 30)
            {
                var p = prefabs[(seed + idx * 7) % prefabs.Count];
                float s = ComputeUniformScale(p, TARGET_BOOK_HEIGHT);
                var go = (GameObject)PrefabUtility.InstantiatePrefab(p);
                go.name = $"{prefix}{idx:D2}";
                go.transform.localScale = new Vector3(s, s, s);
                go.transform.position = new Vector3(x, yBase, z);
                go.transform.rotation = Quaternion.identity;

                var mr = go.GetComponentInChildren<MeshRenderer>();
                float w = (mr != null) ? mr.bounds.size.x : 0.06f;
                float minX = (mr != null) ? mr.bounds.min.x : x;
                float shift = x - minX;
                go.transform.position += new Vector3(shift, 0, 0);
                x += w + 0.003f;
                idx++;
            }
        }
    }
}
#endif
