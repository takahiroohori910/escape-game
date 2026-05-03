#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneSnapshotGenerator
{
    [MenuItem("EscapeGame/Dump Scene Snapshot")]
    public static void DumpSnapshot()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Scene Snapshot");
        sb.AppendLine($"Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine();

        AppendCameraPoints(sb);
        AppendInteractables(sb);
        AppendPuzzles(sb);
        AppendCanvases(sb);

        string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../.claude/scene-snapshot.md"));
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"[SceneSnapshot] 出力: {path}");
        AssetDatabase.Refresh();
    }

    static void AppendCameraPoints(StringBuilder sb)
    {
        sb.AppendLine("## カメラポイント");
        sb.AppendLine("| 名前 | Position | Rotation |");
        sb.AppendLine("|------|----------|----------|");

        foreach (var go in FindAll())
        {
            if (!go.name.Contains("Point") && !go.name.Contains("Camera")) continue;
            var t = go.transform;
            sb.AppendLine($"| {go.name} | {Vec3(t.position)} | {Vec3(t.eulerAngles)} |");
        }
        sb.AppendLine();
    }

    static void AppendInteractables(StringBuilder sb)
    {
        sb.AppendLine("## インタラクタブル");
        sb.AppendLine("| 名前 | Position | Scripts | Collider |");
        sb.AppendLine("|------|----------|---------|---------|");

        var types = new[] {
            "CandleInteractable", "PortraitSymbolInteractable",
            "DisplayCabinetInteractable", "RoomClickZone", "AreaClickZone"
        };

        foreach (var go in FindAll())
        {
            var scripts = new List<string>();
            foreach (var comp in go.GetComponents<MonoBehaviour>())
                if (comp != null && System.Array.IndexOf(types, comp.GetType().Name) >= 0)
                    scripts.Add(comp.GetType().Name);

            if (scripts.Count == 0) continue;

            var col = go.GetComponent<Collider>();
            string colInfo = col != null ? $"{col.GetType().Name} enabled={col.enabled}" : "なし";
            sb.AppendLine($"| {go.name} | {Vec3(go.transform.position)} | {string.Join(", ", scripts)} | {colInfo} |");
        }
        sb.AppendLine();
    }

    static void AppendPuzzles(StringBuilder sb)
    {
        sb.AppendLine("## パズルコンポーネント");
        sb.AppendLine("| 名前 | Position | Script |");
        sb.AppendLine("|------|----------|--------|");

        foreach (var go in FindAll())
        {
            foreach (var comp in go.GetComponents<MonoBehaviour>())
            {
                if (comp == null) continue;
                var name = comp.GetType().Name;
                if (!name.Contains("Puzzle") && !name.Contains("UI")) continue;
                sb.AppendLine($"| {go.name} | {Vec3(go.transform.position)} | {name} |");
                break;
            }
        }
        sb.AppendLine();
    }

    static void AppendCanvases(StringBuilder sb)
    {
        sb.AppendLine("## Canvas 階層");
        sb.AppendLine("| Canvas名 | 子オブジェクト数 | Active |");
        sb.AppendLine("|----------|----------------|--------|");

        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            sb.AppendLine($"| {canvas.gameObject.name} | {canvas.transform.childCount} | {canvas.gameObject.activeSelf} |");
        }
        sb.AppendLine();
    }

    static GameObject[] FindAll() =>
        Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

    static string Vec3(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";
}
#endif
