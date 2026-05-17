#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace EscapeGame.EditorTools
{
    // 椅子の全パーツを一括で奥（+Z 方向）にずらす。
    // 各パーツ（Chair_Seat / Chair_Back / Chair_Leg_*）が World 直下で個別配置されているため、
    // ベース位置からの相対オフセットではなく、各パーツの z に固定シフトを加える。
    public static class ChairRepositionSetup
    {
        // 椅子を机の奥 (z=5.0 付近) に置きたい。Chair_Seat の現在 z=3.0 → 約 +2.0 のオフセット
        const float Z_OFFSET = 2.0f;

        static readonly string[] CHAIR_PARTS =
        {
            "Chair_Seat", "Chair_Back",
            "Chair_BackFrame_L", "Chair_BackFrame_R",
            "Chair_Leg_FL", "Chair_Leg_FR",
            "Chair_Leg_BL", "Chair_Leg_BR",
        };

        // Telephone は親 Fireplace が非アクティブで描画されていないが、
        // PhoneRepairPuzzle スクリプトを含む GameObject 自体は残るため完全削除する
        [MenuItem("EscapeGame/Setup/Remove Telephone")]
        public static void RemoveTelephone()
        {
            int removed = 0;
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                var tel = FindRecursiveByName(root.transform, "Telephone");
                if (tel != null) { Object.DestroyImmediate(tel); removed++; }
                var handset = FindRecursiveByName(root.transform, "TelephoneHandset");
                if (handset != null) { Object.DestroyImmediate(handset); removed++; }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[ChairReposition] Telephone 関連 {removed} 個を削除しました");
        }

        [MenuItem("EscapeGame/Setup/Move Chair To Desk Back")]
        public static void MoveChairBack()
        {
            int moved = 0;
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var name in CHAIR_PARTS)
                {
                    var go = FindRecursiveByName(root.transform, name);
                    if (go == null) continue;
                    var pos = go.transform.position;
                    go.transform.position = new Vector3(pos.x, pos.y, pos.z + Z_OFFSET);
                    moved++;
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[ChairReposition] 椅子の {moved} パーツを z +{Z_OFFSET} 移動しました");
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
    }
}
#endif
