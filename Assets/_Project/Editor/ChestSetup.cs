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

        [MenuItem("EscapeGame/Setup/Build Chest")]
        public static void Build()
        {
            EnsureDir("Assets/_Project/ScriptableObjects");
            EnsureDir(ITEM_DIR);
            EnsureDir("Assets/_Project/Sprites");
            EnsureDir(SPRITE_DIR);
            EnsureDir(MAT_DIR);

            var symbols    = GenerateSymbolSprites();
            var iconOrder  = GenerateMemoIcon("Icon_MemoOrder",  new Color(0.20f, 0.45f, 0.85f));
            var iconLegend = GenerateMemoIcon("Icon_MemoLegend", new Color(0.80f, 0.65f, 0.25f));

            var itemOrder = EnsureItemData(
                "ChestHintOrder", ItemIds.ChestHintOrder,
                "順序の手帳",
                "古びた手帳の切れ端。\n「シンボルを ①→④→②→③ の順に合わせよ」",
                iconOrder);
            var itemLegend = EnsureItemData(
                "ChestHintLegend", ItemIds.ChestHintLegend,
                "凡例の暗号文",
                "羊皮紙にシンボルと番号の対応。\n①  月\n②  星\n③  鍵\n④  花",
                iconLegend);

            // 暖炉群を非アクティブ化（破壊はせず復元可能に）
            foreach (var n in new[] { "Fireplace", "FireplaceFrame", "FireplaceOpening",
                                       "FireplacePhoto", "FireplacePointLight", "NoteOnFireplace" })
                DisableIfExists(n);

            // 既存 Chest を破棄して再構築
            var prev = GameObject.Find("Chest");
            if (prev != null) Object.DestroyImmediate(prev);

            var chestRoot = BuildChest(symbols);
            var chestPoint = BuildChestPoint();

            WireBookshelfHint(itemOrder);
            WireDeskHint(itemLegend);
            WireRoomViewController(chestPoint);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[ChestSetup] Build Chest 完了");
        }

        // ── 構築：チェスト本体 ─────────────────
        static GameObject BuildChest(Sprite[] symbols)
        {
            var root = new GameObject("Chest");
            // 元の暖炉の足元（右壁）に配置。Y=270 で正面を-X方向に向ける
            root.transform.SetPositionAndRotation(new Vector3(4.5f, 0f, 5.7f),
                                                  Quaternion.Euler(0f, 270f, 0f));

            var bodyMat   = LoadOrCreateMat("Mat_Chest_Body",   new Color(0.32f, 0.20f, 0.10f));
            var drawerMat = LoadOrCreateMat("Mat_Chest_Drawer", new Color(0.40f, 0.26f, 0.14f));
            var brassMat  = LoadOrCreateMat("Mat_Chest_Brass",  new Color(0.75f, 0.55f, 0.20f));
            var topMat    = LoadOrCreateMat("Mat_Chest_Top",    new Color(0.28f, 0.16f, 0.06f));

            // 本体
            MakeCube("ChestBody", root.transform,
                     new Vector3(0f, 0.425f, 0f),
                     new Vector3(1.4f, 0.85f, 0.6f), bodyMat);

            // 引き出し 3 段（装飾。クリック対象はダイヤルのみ）
            for (int i = 0; i < 3; i++)
            {
                var drawer = MakeCube($"Drawer_{i}", root.transform,
                    new Vector3(0f, 0.18f + i * 0.22f, -0.31f),
                    new Vector3(1.2f, 0.18f, 0.02f), drawerMat);

                // 把手
                MakeSphere($"Knob_{i}", drawer.transform,
                    new Vector3(0f, 0f, -0.5f),
                    new Vector3(0.05f, 0.4f, 2f), brassMat);
            }

            // 天板
            MakeCube("ChestTop", root.transform,
                     new Vector3(0f, 0.875f, 0f),
                     new Vector3(1.5f, 0.05f, 0.7f), topMat);

            // ChestPuzzle 本体
            var puzzle = root.AddComponent<ChestPuzzle>();

            // 4 個のシンボルダイヤルを天板に並べる
            var dials = new ChestSymbolDial[4];
            for (int i = 0; i < 4; i++)
                dials[i] = BuildDial(i, symbols, puzzle, root.transform, brassMat);

            // ChestPuzzle のフィールド代入
            var soPuzzle = new SerializedObject(puzzle);
            var dialsProp = soPuzzle.FindProperty("dials");
            dialsProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                dialsProp.GetArrayElementAtIndex(i).objectReferenceValue = dials[i];
            var keyItem = AssetDatabase.LoadAssetAtPath<ItemData>($"{ITEM_DIR}/RoomKey.asset");
            if (keyItem != null)
                soPuzzle.FindProperty("roomKeyItem").objectReferenceValue = keyItem;
            soPuzzle.ApplyModifiedPropertiesWithoutUndo();

            // AreaClickZone（Overview からチェストエリアへ移動）
            var clickBox = root.AddComponent<BoxCollider>();
            clickBox.center = new Vector3(0f, 0.45f, 0f);
            clickBox.size   = new Vector3(1.5f, 0.95f, 0.7f);
            var area = root.AddComponent<AreaClickZone>();
            var soArea = new SerializedObject(area);
            soArea.FindProperty("targetArea").enumValueIndex = (int)RoomArea.Chest;
            soArea.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        static ChestSymbolDial BuildDial(int idx, Sprite[] symbols, ChestPuzzle puzzle,
                                         Transform parent, Material brassMat)
        {
            // ダイヤル本体（薄い円柱）
            var dial = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dial.name = $"Dial_{idx}";
            Object.DestroyImmediate(dial.GetComponent<CapsuleCollider>());
            dial.transform.SetParent(parent, false);
            float xOffset = -0.45f + idx * 0.30f;
            dial.transform.localPosition = new Vector3(xOffset, 0.92f, 0f);
            dial.transform.localScale    = new Vector3(0.16f, 0.025f, 0.16f);
            dial.GetComponent<MeshRenderer>().sharedMaterial = brassMat;

            // クリック判定用 BoxCollider
            var bc = dial.AddComponent<BoxCollider>();
            bc.size = new Vector3(1.05f, 1.05f, 1.05f);

            // シンボル表示 SpriteRenderer
            var sprGo = new GameObject("Symbol");
            sprGo.transform.SetParent(dial.transform, false);
            sprGo.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            sprGo.transform.localScale    = new Vector3(0.85f, 5f, 0.85f); // 円柱scale.y=0.025 を打ち消す
            sprGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var sr = sprGo.AddComponent<SpriteRenderer>();
            sr.sprite       = symbols[0];
            sr.sortingOrder = 1;

            // ChestSymbolDial コンポーネント
            var comp = dial.AddComponent<ChestSymbolDial>();
            var so = new SerializedObject(comp);
            so.FindProperty("display").objectReferenceValue = sr;
            so.FindProperty("puzzle").objectReferenceValue  = puzzle;
            var symProp = so.FindProperty("symbols");
            symProp.arraySize = symbols.Length;
            for (int i = 0; i < symbols.Length; i++)
                symProp.GetArrayElementAtIndex(i).objectReferenceValue = symbols[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            return comp;
        }

        // ── カメラポイント ─────────────────
        static Transform BuildChestPoint()
        {
            const string PARENT = "CameraAnchors";
            var anchors = GameObject.Find(PARENT);
            if (anchors == null) anchors = new GameObject(PARENT);

            var existing = GameObject.Find("ChestPoint");
            if (existing != null) Object.DestroyImmediate(existing);

            var pt = new GameObject("ChestPoint");
            pt.transform.SetParent(anchors.transform, true);
            // チェスト正面（-X方向）から見る
            pt.transform.SetPositionAndRotation(new Vector3(2.5f, 1.6f, 5.7f),
                                                Quaternion.Euler(5f, 90f, 0f));
            return pt.transform;
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

        // ── スプライト生成 ─────────────────
        static Sprite[] GenerateSymbolSprites()
        {
            return new[]
            {
                GenSym("Sym_Moon",   DrawMoon),
                GenSym("Sym_Star",   DrawStar),
                GenSym("Sym_Key",    DrawKey),
                GenSym("Sym_Flower", DrawFlower),
                GenSym("Sym_Sword",  DrawSword),
            };
        }

        // シンボル描画関数群 (W,Hピクセル空間で白シルエットを描く)
        delegate bool ShapeMask(int x, int y, int W);

        static Sprite GenSym(string name, ShapeMask mask)
        {
            const int W = 96;
            var tex = new Texture2D(W, W, TextureFormat.ARGB32, false);
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color[W * W];
            var clear = new Color(0f, 0f, 0f, 0f);
            var fill  = new Color(0.96f, 0.92f, 0.85f);   // クリーム白
            var edge  = new Color(0.10f, 0.08f, 0.05f);   // 縁取り
            for (int y = 0; y < W; y++)
                for (int x = 0; x < W; x++)
                {
                    bool inside = mask(x, y, W);
                    if (!inside) { px[y * W + x] = clear; continue; }
                    // エッジ判定：1pxでも外側があるか
                    bool nearEdge = !mask(x - 1, y, W) || !mask(x + 1, y, W)
                                  || !mask(x, y - 1, W) || !mask(x, y + 1, W);
                    px[y * W + x] = nearEdge ? edge : fill;
                }
            tex.SetPixels(px);
            tex.Apply();
            return SaveSprite(tex, name);
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
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp != null)
            {
                imp.textureType         = TextureImporterType.Sprite;
                imp.alphaIsTransparency = true;
                imp.mipmapEnabled       = false;
                imp.filterMode          = FilterMode.Bilinear;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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

        static void DisableIfExists(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) go.SetActive(false);
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
