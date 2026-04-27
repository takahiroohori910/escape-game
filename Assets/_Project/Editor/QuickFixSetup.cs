#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class QuickFixSetup
{
    private const string MAT_DIR = "Assets/_Project/Materials/Generated/";

    [MenuItem("EscapeGame/Setup/Quick Fix")]
    public static void Run()
    {
        if (Application.isPlaying) { Debug.LogError("[Fix] Edit mode で実行してください"); return; }

        FixBooks();
        FixAmbientLight();
        AddClockLight();
        FixParticleVelocity();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Fix] クイックフィックス完了");
    }

    // ── 本：自然な色味に＋ごく控えめな発光（識別できる程度）─────────────
    private static void FixBooks()
    {
        // 暗い部屋で識別できる程度の発光（ネオンにならない強さ）
        FixBook("Book_01", new Color(0.10f, 0.18f, 0.70f), new Color(0.08f, 0.12f, 0.40f), "Mat_Book_Blue");
        FixBook("Book_02", new Color(0.65f, 0.08f, 0.08f), new Color(0.35f, 0.04f, 0.04f), "Mat_Book_Red");
        FixBook("Book_03", new Color(0.08f, 0.55f, 0.12f), new Color(0.04f, 0.28f, 0.06f), "Mat_Book_Green");
        Debug.Log("[Fix] 本の色修正完了");
    }

    private static void FixBook(string goName, Color baseColor, Color emitColor, string matName)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"[Fix] {goName} が見つかりません"); return; }
        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer == null) return;

        if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);
        string path = MAT_DIR + matName + ".mat";
        if (File.Exists(path)) AssetDatabase.DeleteAsset(path);

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", baseColor);
        mat.SetColor("_EmissionColor", emitColor);
        mat.EnableKeyword("_EMISSION");
        mat.SetFloat("_Smoothness", 0.3f);
        AssetDatabase.CreateAsset(mat, path);
        renderer.sharedMaterial = mat;
        EditorUtility.SetDirty(go);
    }

    // ── アンビエントを少し上げて全体を見やすく ─────────────────────────
    private static void FixAmbientLight()
    {
        RenderSettings.ambientMode  = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.18f, 0.14f, 0.10f); // 以前の0.08から上げる
        Debug.Log("[Fix] アンビエントライト調整完了");
    }

    // ── 時計の近くにポイントライトを追加して見やすく ────────────────────
    private static void AddClockLight()
    {
        var existing = GameObject.Find("ClockLight");
        if (existing != null) Object.DestroyImmediate(existing);

        var go = new GameObject("ClockLight");
        var light = go.AddComponent<Light>();
        // 時計は (2.0, 2.5, 5.88) にある
        go.transform.position = new Vector3(2.0f, 2.5f, 5.3f);
        light.type      = LightType.Point;
        light.color     = new Color(0.95f, 0.90f, 0.80f);
        light.intensity = 1.8f;
        light.range     = 1.5f;
        light.shadows   = LightShadows.None;
        EditorUtility.SetDirty(go);
        Debug.Log("[Fix] 時計ライト追加完了");
    }

    // ── パーティクルの Velocity curve モード統一 ─────────────────────────
    private static void FixParticleVelocity()
    {
        FixPS("FireFlame");
        FixPS("FireEmbers");
        FixPS("DustMotes");
        FixPS("RainParticles");
        Debug.Log("[Fix] パーティクル Velocity 修正完了");
    }

    private static void FixPS(string name)
    {
        var go = GameObject.Find(name);
        if (go == null) return;
        var ps = go.GetComponent<ParticleSystem>();
        if (ps == null) return;

        var vel = ps.velocityOverLifetime;
        if (!vel.enabled) return;

        // x/y/z すべて TwoConstants モードに統一
        var x = vel.x;
        var y = vel.y;
        var z = vel.z;

        float xMin = x.constantMin, xMax = x.constantMax;
        float yMin = y.constantMin, yMax = y.constantMax;
        float zMin = z.constantMin, zMax = z.constantMax;

        vel.x = new ParticleSystem.MinMaxCurve(xMin == xMax ? xMin : xMin, xMin == xMax ? xMax : xMax);
        vel.y = new ParticleSystem.MinMaxCurve(yMin == yMax ? yMin : yMin, yMin == yMax ? yMax : yMax);
        vel.z = new ParticleSystem.MinMaxCurve(zMin == zMax ? zMin : zMin, zMin == zMax ? zMax : zMax);

        EditorUtility.SetDirty(go);
    }
}
#endif
