#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class GitGuard
{
    public static bool RequireCleanGit(string actionName = "破壊的操作")
    {
        string repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string status;
        try
        {
            var psi = new ProcessStartInfo("git", "status --porcelain")
            {
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = Process.Start(psi);
            status = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogWarning($"[GitGuard] git実行に失敗: {e.Message}");
            return EditorUtility.DisplayDialog(
                "Git未検出",
                "git コマンドが見つかりません。コミット保護なしで続行しますか？",
                "続行", "キャンセル");
        }

        if (string.IsNullOrEmpty(status)) return true;

        return EditorUtility.DisplayDialog(
            "未コミットの変更があります",
            $"{actionName} は既存オブジェクトを破壊して再生成します。\n" +
            "先にターミナルでコミットすることを強く推奨します。\n\n" +
            "未コミット変更:\n" + status,
            "それでも続行", "キャンセル");
    }
}
#endif
