#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using EscapeGame.Core;
using EscapeGame.Game;

namespace EscapeGame.EditorTools
{
    public static class ChestSetup
    {
        const string ITEM_DIR   = "Assets/_Project/ScriptableObjects/Items";
        const string SPRITE_DIR = "Assets/_Project/Sprites/Generated";
        const string MAT_DIR    = "Assets/_Project/Materials/Generated";

        // Furniture Mega Pack から複数候補を部屋外側に並べて表示する。
        // ユーザーが見比べて「N番がいい」と指示できるようにする。
        [MenuItem("EscapeGame/Setup/Show Furniture Candidates")]
        public static void ShowFurnitureCandidates()
        {
            var prev = GameObject.Find(SceneNames.FurnitureCandidates);
            if (prev != null) Object.DestroyImmediate(prev);
            var root = new GameObject("FurnitureCandidates");

            // 番号をまばらに選定して見た目バリエーションを確保
            int[] picks = { 1, 7, 13, 19, 25, 31, 37, 43, 49 };
            PlaceFurnitureRow(root.transform, "Closets", "Closet", picks, -2.5f);
            PlaceFurnitureRow(root.transform, "Drawers", "Drawer", picks,  6.0f);
            Debug.Log("[ChestSetup] 候補を部屋外側に並べました (Closets: 手前 z=-2.5 / Drawers: 奥 z=6.0)。"
                    + "Sceneビューで真上から眺めて『Closet33 がいい』のように指定してください");
        }

        static void PlaceFurnitureRow(Transform parent, string category, string prefix,
                                       int[] indices, float zPos)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                string name = $"{prefix}{indices[i]:D2}";
                string path = $"Assets/Furniture Mega Pack/Prefabs/{category}/{name}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) { Debug.LogWarning($"[ChestSetup] Not found: {path}"); continue; }
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = name;
                go.transform.SetParent(parent);
                go.transform.position = new Vector3(-6f + i * 1.5f, 0f, zPos);
            }
        }

        // 候補を撤去（選定が終わったらクリーンアップ）
        [MenuItem("EscapeGame/Setup/Clear Furniture Candidates")]
        public static void ClearFurnitureCandidates()
        {
            var prev = GameObject.Find(SceneNames.FurnitureCandidates);
            if (prev != null) Object.DestroyImmediate(prev);
            Debug.Log("[ChestSetup] 候補をクリアしました");
        }

        [MenuItem("EscapeGame/Setup/Build Chest")]
        public static void Build()
        {
            EnsureDir("Assets/_Project/ScriptableObjects");
            EnsureDir(ITEM_DIR);
            EnsureDir("Assets/_Project/Sprites");
            EnsureDir(SPRITE_DIR);
            EnsureDir(MAT_DIR);

            var (symbolTexs, symbolNames) = GenerateSymbolTextures();
            var iconOrder  = GenerateMemoIcon("Icon_MemoOrder",  new Color(0.20f, 0.45f, 0.85f));
            var iconLegend = GenerateMemoIcon("Icon_MemoLegend", new Color(0.80f, 0.65f, 0.25f));

            var itemOrder = EnsureItemData(
                "ChestHintOrder", ItemIds.ChestHintOrder,
                "古びた手帳の切れ端",
                "「絵柄を 月 → 花 → 雲 → 月 → 花 の順に呼べ」",
                iconOrder);
            var itemLegend = EnsureItemData(
                "ChestHintLegend", ItemIds.ChestHintLegend,
                "謎のメモ",
                "月  =  上段\n雲  =  中段\n花  =  下段",
                iconLegend);

            // 暖炉群を非アクティブ化（破壊はせず復元可能に）
            // 子から順に Disable して、親が非アクティブになっても子の検索が成立するようにする
            foreach (var n in new[] { "FireFlame", "FireplaceFrame", "FireplaceOpening",
                                       "FireplacePhoto", "FireplacePointLight", "NoteOnFireplace",
                                       "FireplaceFire", "FireplaceFlame", "FireplaceLogs",
                                       "FireplaceEmbers", "FireplaceArea", "Fireplace",
                                       // 暖炉エリアの装飾品（家具に被るため撤去）
                                       "Candle_L", "Candle_L_Flame", "CandleLight_L",
                                       "Candle_R", "Candle_R_Flame", "CandleLight_R",
                                       "Vase",
                                       // 暖炉の薪・燃え種（家具の下に位置するため撤去）
                                       "Log_L", "Log_R", "FireEmber", "FireEmbers",
                                       // マントル時計（装飾。DeskPuzzle ヒント時計は別オブジェクト Clock）
                                       "MantleClock", "MantleClock_Face", "Mantle" })
                DisableIfExists(n);

            // 既存 Chest を破棄して再構築
            var prev = GameObject.Find(SceneNames.Chest);
            if (prev != null) Object.DestroyImmediate(prev);

            var chestRoot  = BuildChest(symbolTexs, symbolNames);
            var chestPoint = BuildChestPoint();
            BuildChestLight();

            WireBookshelfHint(itemOrder);
            WireDeskHint(itemLegend);
            WireRoomViewController(chestPoint);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[ChestSetup] Build Chest 完了");
        }

        // ── 構築：Furniture Mega Pack の Drawer37 を採用 ─────────────────
        // 自作メッシュ組み立てでは家具感が出なかったため、ユーザー選定の実プレハブを採用。
        const string CHEST_PREFAB_PATH = "Assets/Furniture Mega Pack/Prefabs/Drawers/Drawer37.prefab";

        static GameObject BuildChest(Texture2D[] symbolTexs, string[] symbolNames)
        {
            // 候補表示が残っていたら自動撤去（選定後の清掃）
            var candidates = GameObject.Find(SceneNames.FurnitureCandidates);
            if (candidates != null) Object.DestroyImmediate(candidates);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CHEST_PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"[ChestSetup] プレハブ未検出: {CHEST_PREFAB_PATH}");
                return null;
            }

            var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            root.name = "Chest";

            // 一旦原点に戻して bounds を測ってからクリックボックスを設定する
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var localBounds = ComputeLocalRendererBounds(root);

            // 暖炉跡（右壁）に配置。Y=0、Z=5.2（BackWall Z=5.9 への貫通を避けるため -Z 方向に 0.5m 出す）
            root.transform.SetPositionAndRotation(new Vector3(4.0f, 0f, 5.2f),
                                                  Quaternion.identity);

            // ChestPuzzle コンポーネント（鍵アイテム不要：解錠で Flag のみ立てて扉が直接開く）
            var puzzle = root.AddComponent<ChestPuzzle>();

            // AreaClickZone（Overview からチェストエリアへ）
            var clickBox = root.AddComponent<BoxCollider>();
            clickBox.center = localBounds.center;
            clickBox.size   = localBounds.size + new Vector3(0.05f, 0.05f, 0.05f);
            var area = root.AddComponent<AreaClickZone>();
            var soArea = new SerializedObject(area);
            soArea.FindProperty("targetArea").enumValueIndex = (int)RoomArea.Chest;
            soArea.ApplyModifiedPropertiesWithoutUndo();

            // 3段引き出しに Interactable を仕込む（0=上 / 1=中 / 2=下）
            WireDrawerInteractables(root, puzzle);

            _ = symbolTexs; _ = symbolNames;
            return root;
        }

        // Drawer37_1/_2/_3 に BoxCollider + ChestDrawerInteractable を仕込む
        static void WireDrawerInteractables(GameObject chestRoot, ChestPuzzle puzzle)
        {
            string[] names = { "Drawer37_1", "Drawer37_2", "Drawer37_3" };
            for (int i = 0; i < names.Length; i++)
            {
                var drawer = FindRecursiveByName(chestRoot.transform, names[i]);
                if (drawer == null)
                {
                    Debug.LogWarning($"[ChestSetup] {names[i]} が見つかりません");
                    continue;
                }

                // クリック判定用 BoxCollider（メッシュの local bounds に合わせる）
                if (drawer.GetComponent<Collider>() == null)
                {
                    var bc = drawer.AddComponent<BoxCollider>();
                    var mf = drawer.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null)
                    {
                        bc.center = mf.sharedMesh.bounds.center;
                        bc.size   = mf.sharedMesh.bounds.size;
                    }
                }

                var inter = drawer.GetComponent<ChestDrawerInteractable>();
                if (inter == null) inter = drawer.AddComponent<ChestDrawerInteractable>();
                var so = new SerializedObject(inter);
                so.FindProperty("drawerIndex").intValue = i;
                so.FindProperty("puzzle").objectReferenceValue = puzzle;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // 子レンダラーすべての bounds を root のローカル空間で結合して返す
        static Bounds ComputeLocalRendererBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<MeshRenderer>();
            if (renderers.Length == 0)
                return new Bounds(new Vector3(0f, 0.5f, 0f), Vector3.one);

            var rootInv = root.transform.worldToLocalMatrix;
            bool initialized = false;
            Bounds b = default;
            foreach (var r in renderers)
            {
                var wb = r.bounds; // world-space AABB
                // 8 頂点を root ローカルに戻して再 AABB（簡易）
                var corners = new Vector3[8];
                var min = wb.min; var max = wb.max;
                corners[0] = rootInv.MultiplyPoint3x4(new Vector3(min.x, min.y, min.z));
                corners[1] = rootInv.MultiplyPoint3x4(new Vector3(max.x, min.y, min.z));
                corners[2] = rootInv.MultiplyPoint3x4(new Vector3(min.x, max.y, min.z));
                corners[3] = rootInv.MultiplyPoint3x4(new Vector3(max.x, max.y, min.z));
                corners[4] = rootInv.MultiplyPoint3x4(new Vector3(min.x, min.y, max.z));
                corners[5] = rootInv.MultiplyPoint3x4(new Vector3(max.x, min.y, max.z));
                corners[6] = rootInv.MultiplyPoint3x4(new Vector3(min.x, max.y, max.z));
                corners[7] = rootInv.MultiplyPoint3x4(new Vector3(max.x, max.y, max.z));
                foreach (var c in corners)
                {
                    if (!initialized) { b = new Bounds(c, Vector3.zero); initialized = true; }
                    else b.Encapsulate(c);
                }
            }
            return b;
        }

        // 旧 BuildDial / CreateSymbolMaterials は方式変更で削除済み（引き出し順序方式）

        // ── カメラポイント ─────────────────
        static Transform BuildChestPoint()
        {
            const string PARENT = "CameraAnchors";
            var anchors = GameObject.Find(PARENT);
            if (anchors == null) anchors = new GameObject(PARENT);

            var existing = GameObject.Find(SceneNames.ChestPoint);
            if (existing != null) Object.DestroyImmediate(existing);

            var pt = new GameObject("ChestPoint");
            pt.transform.SetParent(anchors.transform, true);
            // ユーザーが Align With View で確定した視点
            pt.transform.SetPositionAndRotation(new Vector3(3.539f, 2.116f, 1.231f),
                                                Quaternion.Euler(9.282f, 5.844f, 0f));
            return pt.transform;
        }

        // 暖炉ライトの代替光源（チェストを照らす暖色ポイントライト）
        static void BuildChestLight()
        {
            const string NAME = "ChestPointLight";
            var existing = GameObject.Find(NAME);
            if (existing != null) Object.DestroyImmediate(existing);

            var go = new GameObject(NAME);
            go.transform.position = new Vector3(3.6f, 1.8f, 5.7f);
            var l = go.AddComponent<Light>();
            l.type      = LightType.Point;
            l.color     = new Color(1.0f, 0.88f, 0.65f);
            l.intensity = 3.2f;
            l.range     = 6.0f;
            l.shadows   = LightShadows.Soft;
        }

        // ── 既存スクリプトへのアサイン ─────────────────
        static void WireBookshelfHint(ItemData hint)
        {
            var puzzle = Object.FindAnyObjectByType<BookshelfPuzzle>();
            if (puzzle == null || hint == null) return;
            var so = new SerializedObject(puzzle);
            var prop = so.FindProperty("hintItem");
            if (prop != null) prop.objectReferenceValue = hint;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireDeskHint(ItemData hint)
        {
            var puzzle = Object.FindAnyObjectByType<DeskPuzzle>();
            if (puzzle == null || hint == null) return;
            var so = new SerializedObject(puzzle);
            var prop = so.FindProperty("hintItem");
            if (prop != null) prop.objectReferenceValue = hint;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireRoomViewController(Transform chestPoint)
        {
            var rvc = Object.FindAnyObjectByType<RoomViewController>();
            if (rvc == null || chestPoint == null) return;
            var so = new SerializedObject(rvc);
            var prop = so.FindProperty("chestPoint");
            if (prop != null) prop.objectReferenceValue = chestPoint;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── ItemData 生成 ─────────────────
        static ItemData EnsureItemData(string fileName, string id, string displayName,
                                        string desc, Sprite icon)
        {
            string path = $"{ITEM_DIR}/{fileName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            var item = existing != null ? existing : ScriptableObject.CreateInstance<ItemData>();
            var so = new SerializedObject(item);
            so.FindProperty("itemId").stringValue      = id;
            so.FindProperty("itemName").stringValue    = displayName;
            so.FindProperty("description").stringValue = desc;
            if (icon != null) so.FindProperty("icon").objectReferenceValue = icon;
            so.ApplyModifiedPropertiesWithoutUndo();
            if (existing == null) AssetDatabase.CreateAsset(item, path);
            else EditorUtility.SetDirty(item);
            return item;
        }

        // ── テクスチャ生成（Sprite アセット化せず Texture2D を直接マテリアルに渡す）─────
        delegate bool ShapeMask(int x, int y, int W);

        static (Texture2D[], string[]) GenerateSymbolTextures()
        {
            var shapes = new (string name, ShapeMask mask)[]
            {
                ("Sym_Moon",   DrawMoon),
                ("Sym_Star",   DrawStar),
                ("Sym_Key",    DrawKey),
                ("Sym_Flower", DrawFlower),
                ("Sym_Sword",  DrawSword),
            };
            var texs  = new Texture2D[shapes.Length];
            var names = new string[shapes.Length];

            // 全 PNG を先に書き出し、最後に Refresh + Import で一括反映
            for (int i = 0; i < shapes.Length; i++)
            {
                names[i] = shapes[i].name;
                var tmp = RenderSymbolTexture(shapes[i].mask);
                File.WriteAllBytes($"{SPRITE_DIR}/{shapes[i].name}.png", tmp.EncodeToPNG());
                Object.DestroyImmediate(tmp);
            }
            AssetDatabase.Refresh();
            for (int i = 0; i < shapes.Length; i++)
            {
                string path = $"{SPRITE_DIR}/{shapes[i].name}.png";
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp != null)
                {
                    imp.textureType         = TextureImporterType.Default;
                    imp.alphaIsTransparency = true;
                    imp.mipmapEnabled       = false;
                    imp.filterMode          = FilterMode.Bilinear;
                    imp.SaveAndReimport();
                }
                texs[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return (texs, names);
        }

        static Texture2D RenderSymbolTexture(ShapeMask mask)
        {
            const int W = 96;
            var tex = new Texture2D(W, W, TextureFormat.ARGB32, false);
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color[W * W];
            var clear = new Color(0f, 0f, 0f, 0f);
            var fill  = new Color(0.96f, 0.92f, 0.85f);
            var edge  = new Color(0.10f, 0.08f, 0.05f);
            for (int y = 0; y < W; y++)
                for (int x = 0; x < W; x++)
                {
                    bool inside = mask(x, y, W);
                    if (!inside) { px[y * W + x] = clear; continue; }
                    bool nearEdge = !mask(x - 1, y, W) || !mask(x + 1, y, W)
                                  || !mask(x, y - 1, W) || !mask(x, y + 1, W);
                    px[y * W + x] = nearEdge ? edge : fill;
                }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        static bool DrawMoon(int x, int y, int W)
        {
            int cx = W / 2, cy = W / 2, r = W * 3 / 10;
            float d1 = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
            float d2 = Mathf.Sqrt((x - (cx + W / 8)) * (x - (cx + W / 8))
                                + (y - cy) * (y - cy));
            return d1 < r && d2 > r;
        }

        static bool DrawStar(int x, int y, int W)
        {
            int cx = W / 2, cy = W / 2;
            int t = W / 12; int len = W * 5 / 16;
            if (Mathf.Abs(x - cx) <= t && Mathf.Abs(y - cy) <= len) return true; // 縦
            if (Mathf.Abs(y - cy) <= t && Mathf.Abs(x - cx) <= len) return true; // 横
            int dx = x - cx, dy = y - cy;
            if (Mathf.Abs(dx - dy) <= t && Mathf.Abs(dx) <= len * 3 / 4) return true; // 斜め1
            if (Mathf.Abs(dx + dy) <= t && Mathf.Abs(dx) <= len * 3 / 4) return true; // 斜め2
            return false;
        }

        static bool DrawKey(int x, int y, int W)
        {
            int cx = W / 2;
            int headCy = W * 3 / 4;
            int headR  = W / 8;
            // 円環（ヘッド）
            float dh = Mathf.Sqrt((x - cx) * (x - cx) + (y - headCy) * (y - headCy));
            if (dh < headR + 1 && dh > headR - 4) return true;
            // 軸（縦棒）
            if (Mathf.Abs(x - cx) < 3 && y < headCy - headR + 2 && y > W / 8) return true;
            // 歯（横棒）
            if (Mathf.Abs(y - W / 5) < 2 && x >= cx && x < cx + W / 6) return true;
            if (Mathf.Abs(y - W / 5 + 5) < 2 && x >= cx && x < cx + W / 9) return true;
            return false;
        }

        static bool DrawFlower(int x, int y, int W)
        {
            int cx = W / 2, cy = W / 2;
            int r = W / 6;
            // 4方向の花びら（円）
            if (InCircle(x, y, cx, cy - r * 3 / 2, r)) return true;
            if (InCircle(x, y, cx, cy + r * 3 / 2, r)) return true;
            if (InCircle(x, y, cx - r * 3 / 2, cy, r)) return true;
            if (InCircle(x, y, cx + r * 3 / 2, cy, r)) return true;
            // 中央
            if (InCircle(x, y, cx, cy, r * 2 / 3)) return true;
            return false;
        }

        static bool DrawSword(int x, int y, int W)
        {
            int cx = W / 2;
            // 剣身（縦長）
            if (Mathf.Abs(x - cx) <= 3 && y > W / 6 && y < W * 5 / 6) return true;
            // 鍔（横棒）
            if (Mathf.Abs(y - W / 4) <= 2 && Mathf.Abs(x - cx) <= W / 5) return true;
            // 柄頭
            if (Mathf.Abs(y - W / 8) <= 4 && Mathf.Abs(x - cx) <= 5) return true;
            return false;
        }

        static bool InCircle(int x, int y, int cx, int cy, int r)
            => (x - cx) * (x - cx) + (y - cy) * (y - cy) < r * r;

        static Sprite GenerateMemoIcon(string name, Color stripe)
        {
            const int W = 96, H = 96;
            var tex = new Texture2D(W, H, TextureFormat.ARGB32, false);
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color[W * H];
            var clear = new Color(0f, 0f, 0f, 0f);
            var paper = new Color(0.95f, 0.92f, 0.82f);
            var ink   = new Color(0.20f, 0.15f, 0.08f);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    bool inPaper = x >= 10 && x < W - 10 && y >= 8 && y < H - 16;
                    if (!inPaper) { px[y * W + x] = clear; continue; }
                    // 紙の縁
                    bool isEdge = x == 10 || x == W - 11 || y == 8 || y == H - 17;
                    if (isEdge) { px[y * W + x] = ink; continue; }
                    // 上端帯（色分け）
                    if (y >= H - 22 && y < H - 16) { px[y * W + x] = stripe; continue; }
                    px[y * W + x] = paper;
                }
            // 横の罫線3本
            int[] lines = { H * 1 / 3, H * 1 / 2, H * 5 / 8 };
            foreach (var ly in lines)
                for (int x = 16; x < W - 16; x++)
                    px[ly * W + x] = ink;
            tex.SetPixels(px);
            tex.Apply();
            return SaveSprite(tex, name);
        }

        static Sprite SaveSprite(Texture2D tex, string name)
        {
            string path = $"{SPRITE_DIR}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.Refresh();
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp != null)
            {
                imp.textureType         = TextureImporterType.Sprite;
                imp.alphaIsTransparency = true;
                imp.mipmapEnabled       = false;
                imp.filterMode          = FilterMode.Bilinear;
                imp.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                // フォールバック：1回 Reimport を強制
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            return sprite;
        }

        // ── ユーティリティ ─────────────────
        static GameObject MakeCube(string name, Transform parent,
                                    Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale    = localScale;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        static GameObject MakeSphere(string name, Transform parent,
                                      Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<SphereCollider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale    = localScale;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        static Material LoadOrCreateMat(string fileName, Color baseColor)
        {
            string path = $"{MAT_DIR}/{fileName}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", 0.2f);
            mat.SetFloat("_Metallic",   0.0f);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        // 色や設定値を必ず上書きしたい時のヘルパー
        static Material ResetOrCreateMat(string fileName, Color baseColor)
        {
            string path = $"{MAT_DIR}/{fileName}.mat";
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            var mat = existing != null ? existing : new Material(shader);
            mat.shader = shader;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", 0.25f);
            mat.SetFloat("_Metallic",   0.0f);
            if (existing == null) AssetDatabase.CreateAsset(mat, path);
            else EditorUtility.SetDirty(mat);
            return mat;
        }

        // 非アクティブの親配下も含めて検索して SetActive(false) する。
        // GameObject.Find は非アクティブの親配下を見つけられないため、
        // 全 root を再帰探索する独自実装を用いる。
        static void DisableIfExists(string name)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                var found = FindRecursiveByName(root.transform, name);
                if (found != null) found.SetActive(false);
            }
        }

        static GameObject FindRecursiveByName(Transform t, string name)
        {
            if (t.name == name) return t.gameObject;
            foreach (Transform child in t)
            {
                var f = FindRecursiveByName(child, name);
                if (f != null) return f;
            }
            return null;
        }

        static void EnsureDir(string assetDir)
        {
            if (AssetDatabase.IsValidFolder(assetDir)) return;
            string parent = Path.GetDirectoryName(assetDir);
            string leaf   = Path.GetFileName(assetDir);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
