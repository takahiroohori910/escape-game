#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ParticleEffectsSetup
{
    [MenuItem("EscapeGame/Setup/Particle Effects")]
    public static void Run()
    {
        if (Application.isPlaying) { Debug.LogError("[FX] Edit mode で実行してください"); return; }

        SetupFireplaceFlame();
        SetupFireEmbers();
        SetupDustMotes();
        SetupWindow();
        SetupRain();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FX] パーティクルエフェクト設定完了");
    }

    // ── 暖炉の炎 ────────────────────────────────────────────────────────
    private static void SetupFireplaceFlame()
    {
        RemoveExisting("FireFlame");

        var go = new GameObject("FireFlame");
        go.transform.position = new Vector3(4.5f, 0.72f, 5.55f);
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop           = true;
        main.startLifetime  = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize      = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
        main.startColor     = new ParticleSystem.MinMaxGradient(
                                  new Color(1.0f, 0.30f, 0.0f, 1f),
                                  new Color(1.0f, 0.72f, 0.1f, 1f));
        main.maxParticles   = 120;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.05f;

        var emission = ps.emission;
        emission.rateOverTime = 40f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale     = new Vector3(0.75f, 0.01f, 0.25f);
        shape.rotation  = new Vector3(0f, 0f, 0f);

        // 色：オレンジ→黄色→白→煙（透明）
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(new Color(1f, 0.2f, 0f),   0.0f),
                new GradientColorKey(new Color(1f, 0.7f, 0.1f), 0.3f),
                new GradientColorKey(new Color(1f, 0.95f, 0.6f),0.6f),
                new GradientColorKey(new Color(0.5f, 0.5f, 0.5f),0.85f),
                new GradientColorKey(new Color(0.2f, 0.2f, 0.2f),1.0f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0.5f, 0.8f),
                new GradientAlphaKey(0f, 1f)
            });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        // サイズ：成長してから消える
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        var sizeCurve = AnimationCurve.Linear(0f, 0.4f, 1f, 0f);
        sizeCurve.AddKey(new Keyframe(0.3f, 1.0f));
        size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // 速度：上昇＋微小横揺れ
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.y = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        vel.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
        vel.z = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);

        // ノイズで炎を揺らす
        var noise = ps.noise;
        noise.enabled   = true;
        noise.strength  = 0.08f;
        noise.frequency = 0.5f;
        noise.scrollSpeed= 0.3f;

        SetParticleMat(ps, "FireFlameMat", new Color(1f, 0.5f, 0.1f), true);
        EditorUtility.SetDirty(go);
        Debug.Log("[FX] 暖炉炎パーティクル追加完了");
    }

    // ── 火の粉（スパーク） ───────────────────────────────────────────────
    private static void SetupFireEmbers()
    {
        RemoveExisting("FireEmbers");

        var go = new GameObject("FireEmbers");
        go.transform.position = new Vector3(4.5f, 0.8f, 5.55f);
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop          = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3.0f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(0.5f, 2.0f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.01f, 0.04f);
        main.startColor    = new ParticleSystem.MinMaxGradient(
                                 new Color(1f, 0.5f, 0f),
                                 new Color(1f, 0.9f, 0.3f));
        main.maxParticles  = 50;
        main.gravityModifier = -0.1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 8f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle     = 35f;
        shape.radius    = 0.3f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(new Color(1f, 0.8f, 0.1f), 0f),
                new GradientColorKey(new Color(1f, 0.3f, 0f),   0.5f),
                new GradientColorKey(new Color(0.3f, 0.3f, 0.3f),1f)
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        SetParticleMat(ps, "EmberMat", new Color(1f, 0.6f, 0f), true);
        EditorUtility.SetDirty(go);
        Debug.Log("[FX] 火の粉パーティクル追加完了");
    }

    // ── 浮遊埃（雰囲気演出） ─────────────────────────────────────────────
    private static void SetupDustMotes()
    {
        RemoveExisting("DustMotes");

        var go = new GameObject("DustMotes");
        go.transform.position = new Vector3(0f, 1.5f, 3f);
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop          = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 15f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(0f, 0.05f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.005f, 0.02f);
        main.startColor    = new ParticleSystem.MinMaxGradient(
                                 new Color(0.9f, 0.85f, 0.7f, 0.3f),
                                 new Color(0.8f, 0.75f, 0.6f, 0.1f));
        main.maxParticles  = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 5f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale     = new Vector3(8f, 2.5f, 5f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.y = new ParticleSystem.MinMaxCurve(-0.005f, 0.005f);
        vel.x = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);

        var noise = ps.noise;
        noise.enabled    = true;
        noise.strength   = 0.02f;
        noise.frequency  = 0.1f;
        noise.scrollSpeed= 0.05f;

        SetParticleMat(ps, "DustMat", new Color(0.9f, 0.85f, 0.7f), false);
        EditorUtility.SetDirty(go);
        Debug.Log("[FX] 埃パーティクル追加完了");
    }

    // ── 窓の作成 ─────────────────────────────────────────────────────────
    private static void SetupWindow()
    {
        // 左壁に窓を配置（x=-4.9, y=1.8, z=2.0）
        RemoveExisting("Window");
        RemoveExisting("WindowGlass");
        RemoveExisting("WindowSill");

        // 窓枠（外枠）
        Color frameColor = new Color(0.22f, 0.14f, 0.08f);
        const string matDir = "Assets/_Project/Materials/Generated/";

        // 上下左右の枠
        CreateWindowBar("Window_FrameTop",    new Vector3(-4.93f, 2.28f, 2.0f), new Vector3(0.04f, 0.08f, 1.42f), frameColor);
        CreateWindowBar("Window_FrameBottom", new Vector3(-4.93f, 1.12f, 2.0f), new Vector3(0.04f, 0.08f, 1.42f), frameColor);
        CreateWindowBar("Window_FrameLeft",   new Vector3(-4.93f, 1.70f, 1.32f), new Vector3(0.04f, 1.24f, 0.08f), frameColor);
        CreateWindowBar("Window_FrameRight",  new Vector3(-4.93f, 1.70f, 2.68f), new Vector3(0.04f, 1.24f, 0.08f), frameColor);
        CreateWindowBar("Window_FrameHMid",   new Vector3(-4.93f, 1.70f, 2.0f), new Vector3(0.04f, 1.24f, 0.04f), frameColor);
        CreateWindowBar("Window_FrameVMid",   new Vector3(-4.93f, 1.70f, 2.0f), new Vector3(0.04f, 0.04f, 1.42f), frameColor);

        // ガラス（半透明）
        var glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glass.name = "WindowGlass";
        glass.transform.position   = new Vector3(-4.92f, 1.70f, 2.0f);
        glass.transform.localScale = new Vector3(0.02f, 1.12f, 1.30f);
        Object.DestroyImmediate(glass.GetComponent<BoxCollider>());

        var glassRend = glass.GetComponent<MeshRenderer>();
        var glassMat  = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        glassMat.SetColor("_BaseColor", new Color(0.3f, 0.4f, 0.55f, 0.15f));
        glassMat.SetFloat("_Smoothness", 0.9f);
        glassMat.SetFloat("_Surface", 1f);   // Transparent
        glassMat.SetFloat("_Blend", 0f);     // Alpha blend
        glassMat.renderQueue = 3000;
        if (!System.IO.Directory.Exists(matDir)) System.IO.Directory.CreateDirectory(matDir);
        AssetDatabase.CreateAsset(glassMat, matDir + "Mat_WindowGlass.mat");
        glassRend.sharedMaterial = glassMat;
        EditorUtility.SetDirty(glass);

        // 窓台（ウィンドウシル）
        var sill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sill.name = "WindowSill";
        sill.transform.position   = new Vector3(-4.88f, 1.08f, 2.0f);
        sill.transform.localScale = new Vector3(0.06f, 0.06f, 1.5f);
        Object.DestroyImmediate(sill.GetComponent<BoxCollider>());
        ApplyColorMat(sill, new Color(0.55f, 0.52f, 0.48f), "Mat_WindowSill");
        EditorUtility.SetDirty(sill);

        // 嵐の外景（暗い壁）
        var outside = GameObject.CreatePrimitive(PrimitiveType.Cube);
        outside.name = "OutsideStorm";
        outside.transform.position   = new Vector3(-5.3f, 1.70f, 2.0f);
        outside.transform.localScale = new Vector3(0.1f, 1.5f, 1.5f);
        Object.DestroyImmediate(outside.GetComponent<BoxCollider>());
        ApplyColorMat(outside, new Color(0.04f, 0.06f, 0.10f), "Mat_StormNight");
        EditorUtility.SetDirty(outside);

        Debug.Log("[FX] 窓追加完了");
    }

    // ── 雨粒パーティクル（窓の外） ──────────────────────────────────────
    private static void SetupRain()
    {
        RemoveExisting("RainParticles");

        var go = new GameObject("RainParticles");
        go.transform.position = new Vector3(-5.1f, 2.5f, 2.0f);
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop          = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(3.0f, 5.0f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.005f, 0.012f);
        main.startRotation = new ParticleSystem.MinMaxCurve(Mathf.PI * 0.5f);  // 縦線
        main.startColor    = new ParticleSystem.MinMaxGradient(
                                 new Color(0.5f, 0.6f, 0.75f, 0.6f),
                                 new Color(0.6f, 0.7f, 0.85f, 0.4f));
        main.maxParticles  = 300;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 150f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale     = new Vector3(0.1f, 0.1f, 1.3f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.y = new ParticleSystem.MinMaxCurve(-5f, -3.5f);
        vel.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);

        SetParticleMat(ps, "RainMat", new Color(0.6f, 0.7f, 0.85f), false);
        EditorUtility.SetDirty(go);
        Debug.Log("[FX] 雨粒パーティクル追加完了");
    }

    // ── ユーティリティ ───────────────────────────────────────────────────
    private static void SetParticleMat(ParticleSystem ps, string matName, Color color, bool additive)
    {
        const string dir = "Assets/_Project/Materials/Generated/";
        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        var shader   = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                    ?? Shader.Find("Particles/Standard Unlit")
                    ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        if (additive)
        {
            mat.SetFloat("_Blend", 4f);         // Additive
            mat.SetFloat("_SrcBlend", 1f);
            mat.SetFloat("_DstBlend", 1f);
            mat.SetFloat("_ZWrite", 0f);
            mat.renderQueue = 3500;
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        }
        else
        {
            mat.SetFloat("_Surface", 1f);
            mat.renderQueue = 3000;
        }
        string path = dir + matName + ".mat";
        AssetDatabase.CreateAsset(mat, path);
        renderer.material = mat;
    }

    private static void CreateWindowBar(string name, Vector3 pos, Vector3 scale, Color color)
    {
        RemoveExisting(name);
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position   = pos;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        ApplyColorMat(go, color, "Mat_" + name);
        EditorUtility.SetDirty(go);
    }

    private static void ApplyColorMat(GameObject go, Color color, string matName)
    {
        const string dir = "Assets/_Project/Materials/Generated/";
        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer == null) return;
        string path = dir + matName + ".mat";
        if (System.IO.File.Exists(path)) AssetDatabase.DeleteAsset(path);
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(mat, path);
        renderer.sharedMaterial = mat;
    }

    private static void RemoveExisting(string name)
    {
        var e = GameObject.Find(name);
        if (e != null) Object.DestroyImmediate(e);
    }
}
#endif
