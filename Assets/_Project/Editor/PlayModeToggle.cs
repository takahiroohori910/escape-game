#if UNITY_EDITOR
using UnityEditor;

public static class PlayModeToggle
{
    [MenuItem("EscapeGame/Play Mode/Enter Play Mode")]
    public static void EnterPlayMode() => EditorApplication.isPlaying = true;

    [MenuItem("EscapeGame/Play Mode/Exit Play Mode")]
    public static void ExitPlayMode() => EditorApplication.isPlaying = false;
}
#endif
