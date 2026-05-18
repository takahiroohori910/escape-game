#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace EscapeGame.EditorTools
{
    // BackWall を扉エリアで分割して「扉の中・奥」に壁メッシュがない構造にする
    // 元 BackWall: x[-5..5] y[0..4] z[5.9..6.1]
    // 扉エリア: x[-0.71..0.71] y[0..3]
    // 分割後: Left (x[-5..-0.71]) / Right (x[0.71..5]) / AboveDoor (y[3..4] x[-0.71..0.71])
    public static class BackWallSplitSetup
    {
        [MenuItem("EscapeGame/Setup/Split BackWall For Door")]
        public static void Run()
        {
            var bw = GameObject.Find("BackWall");
            if (bw == null) { Debug.LogError("[BackWallSplit] BackWall が見つかりません"); return; }

            // 元 BackWall のマテリアル取得（同じ質感で分割壁を作る）
            var mr = bw.GetComponent<MeshRenderer>();
            var mat = mr != null ? mr.sharedMaterial : null;

            // 既存の分割を削除（冪等性）
            foreach (var n in new[] { "BackWall_Left", "BackWall_Right", "BackWall_AboveDoor" })
            {
                var existing = GameObject.Find(n);
                if (existing != null) Object.DestroyImmediate(existing);
            }

            // 元 BackWall は非表示（メッシュも当たり判定も消す）
            bw.SetActive(false);

            // 扉開口部のパラメータ（Room1Door の中心 x=0, 幅 1.42）
            const float doorHalfWidth = 0.71f;
            const float doorTop       = 3.0f;
            const float wallZ         = 6.0f;
            const float wallThickness = 0.2f;
            const float wallTop       = 4.0f;
            const float wallLeftX     = -5.0f;
            const float wallRightX    = 5.0f;

            // 左：x[-5..-0.71]
            float leftW = -doorHalfWidth - wallLeftX;       // 4.29
            CreateWallSection("BackWall_Left",
                new Vector3((wallLeftX + (-doorHalfWidth)) * 0.5f, wallTop * 0.5f, wallZ),
                new Vector3(leftW, wallTop, wallThickness), mat);

            // 右：x[0.71..5]
            float rightW = wallRightX - doorHalfWidth;      // 4.29
            CreateWallSection("BackWall_Right",
                new Vector3((doorHalfWidth + wallRightX) * 0.5f, wallTop * 0.5f, wallZ),
                new Vector3(rightW, wallTop, wallThickness), mat);

            // 扉の上：x[-0.71..0.71] y[3..4]
            float aboveH = wallTop - doorTop;               // 1.0
            CreateWallSection("BackWall_AboveDoor",
                new Vector3(0f, (doorTop + wallTop) * 0.5f, wallZ),
                new Vector3(doorHalfWidth * 2f, aboveH, wallThickness), mat);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[BackWallSplit] BackWall を扉エリアで分割しました（Left/Right/AboveDoor）");
        }

        static void CreateWallSection(string name, Vector3 pos, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position  = pos;
            go.transform.localScale = size;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }
    }
}
#endif
