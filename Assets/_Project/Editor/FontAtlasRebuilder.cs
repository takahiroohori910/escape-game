#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using TMPro;

public static class FontAtlasRebuilder
{
    const string AssetPath = "Assets/_Project/Fonts/NotoSansJP_Dynamic.asset";

    // 完全再構築（Atlas Texture + Material を新規作成して割り当て）
    [MenuItem("EscapeGame/Font/Force Rebuild Atlas")]
    public static void ForceRebuild()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
        if (asset == null) { Debug.LogError("[FontRebuild] Asset 読込失敗: " + AssetPath); return; }

        // 既存の Atlas/Material サブアセットを削除
        var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetPath);
        foreach (var sub in subAssets)
        {
            if (sub == null || sub == asset) continue;
            if (sub is Texture2D || sub is Material)
            {
                Object.DestroyImmediate(sub, true);
                Debug.Log($"[FontRebuild] サブアセット削除: {sub.name} ({sub.GetType().Name})");
            }
        }

        // 新しい Atlas Texture を作成
        var atlasTexture = new Texture2D(asset.atlasWidth, asset.atlasHeight, TextureFormat.Alpha8, false);
        atlasTexture.name = "Font Atlas";
        // 透明で初期化
        var pixels = new Color32[asset.atlasWidth * asset.atlasHeight];
        atlasTexture.SetPixels32(pixels);
        atlasTexture.Apply();
        AssetDatabase.AddObjectToAsset(atlasTexture, asset);

        // Material 作成
        var shader = Shader.Find("TextMeshPro/Mobile/Distance Field");
        if (shader == null) shader = Shader.Find("TextMeshPro/Distance Field");
        if (shader == null) { Debug.LogError("[FontRebuild] TMP shader が見つかりません"); return; }
        var material = new Material(shader);
        material.name = asset.name + " Atlas Material";
        material.SetTexture("_MainTex", atlasTexture);
        AssetDatabase.AddObjectToAsset(material, asset);

        // asset に割り当て
        asset.atlasTextures = new[] { atlasTexture };
        asset.material = material;

        // データ再読み込み
        asset.ClearFontAssetData(true);
        asset.ReadFontAssetDefinition();

        EditorUtility.SetDirty(asset);
        EditorUtility.SetDirty(atlasTexture);
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);

        Debug.Log($"[FontRebuild] 完全再構築完了: AtlasTexture={atlasTexture.GetInstanceID()} Material={material.GetInstanceID()}");
    }

    // 使用されている文字を Atlas にベイク（シーン内 TMP + ScriptableObject(NoteData) を走査）
    [MenuItem("EscapeGame/Font/Bake Used Characters")]
    public static void BakeUsedCharacters()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
        if (asset == null) { Debug.LogError("[FontRebuild] Asset 読込失敗: " + AssetPath); return; }

        var chars = new HashSet<char>();
        // 1. シーン内の TMP_Text 全部
        var texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in texts) { if (!string.IsNullOrEmpty(t.text)) foreach (var c in t.text) chars.Add(c); }
        // 2. ScriptableObject の NoteData 等を Asset 検索（プロジェクト全体）
        foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (so == null) continue;
            var serObj = new SerializedObject(so);
            var prop = serObj.GetIterator();
            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(prop.stringValue))
                    foreach (var c in prop.stringValue) chars.Add(c);
            }
        }
        // 3. 基本ASCII を追加
        for (char c = ' '; c <= '~'; c++) chars.Add(c);

        var sb = new StringBuilder();
        foreach (var c in chars) sb.Append(c);
        var charString = sb.ToString();

        asset.TryAddCharacters(charString, out var missing);
        var missingCount = missing != null ? missing.Length : 0;
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
        Debug.Log($"[FontRebuild] {chars.Count} chars をベイク（追加失敗: {missingCount} 件）");
    }

    // 全 TMP_Text に新しい Material を再アサイン（Force Rebuild 後に実行）
    [MenuItem("EscapeGame/Font/Refresh All TMP Materials")]
    public static void RefreshAllTMPMaterials()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
        if (asset == null) { Debug.LogError("[FontRebuild] Asset 読込失敗"); return; }
        if (asset.material == null) { Debug.LogError("[FontRebuild] asset.material が null。先に Force Rebuild を実行"); return; }

        var texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int updated = 0;
        foreach (var t in texts)
        {
            if (t.font != asset) continue;
            t.fontSharedMaterial = asset.material;
            EditorUtility.SetDirty(t);
            updated++;
        }
        // Prefab/Asset 内の TMP も走査（GUI Object 含む）
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            var prefabTexts = prefab.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in prefabTexts)
            {
                if (t.font != asset) continue;
                t.fontSharedMaterial = asset.material;
                EditorUtility.SetDirty(t);
                updated++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[FontRebuild] {updated} TMP_Text の Material を更新");
    }

    // LiberationSans SDF Material をテンプレートとしてコピーし、Atlas を差し替える
    [MenuItem("EscapeGame/Font/Fix Material From Template")]
    public static void FixMaterialFromTemplate()
    {
        const string newAssetPath = "Assets/_Project/Fonts/NotoSansJP_Fresh.asset";
        var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(newAssetPath);
        if (asset == null) { Debug.LogError("[FontRebuild] Fresh asset not found"); return; }

        // テンプレート Material（LiberationSans SDF Material）取得
        Material templateMat = null;
        foreach (var guid in AssetDatabase.FindAssets("LiberationSans SDF Material t:Material"))
        {
            templateMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (templateMat != null) break;
        }
        if (templateMat == null && TMP_Settings.defaultFontAsset != null)
            templateMat = TMP_Settings.defaultFontAsset.material;
        if (templateMat == null) { Debug.LogError("[FontRebuild] LiberationSans Material が見つからない"); return; }

        // サブアセット取得
        var subs = AssetDatabase.LoadAllAssetsAtPath(newAssetPath);
        var atlasTex = subs.OfType<Texture2D>().FirstOrDefault();
        if (atlasTex == null) { Debug.LogError("[FontRebuild] Atlas Texture が見つからない"); return; }

        // 既存の Material サブアセット削除
        foreach (var sub in subs)
            if (sub is Material oldMat && oldMat != asset)
                Object.DestroyImmediate(oldMat, true);

        // テンプレートをコピーした Material 作成
        var newMat = new Material(templateMat);
        newMat.name = asset.name + " Material";
        newMat.SetTexture("_MainTex", atlasTex);
        AssetDatabase.AddObjectToAsset(newMat, asset);

        // SerializedObject で永続化
        var serObj = new SerializedObject(asset);
        serObj.FindProperty("m_Material").objectReferenceValue = newMat;
        serObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
        EditorUtility.SetDirty(newMat);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(newAssetPath, ImportAssetOptions.ForceUpdate);

        // 全 TMP に再アサイン
        var texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int updated = 0;
        foreach (var t in texts)
        {
            if (t.font == asset)
            {
                t.fontSharedMaterial = newMat;
                EditorUtility.SetDirty(t);
                updated++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[FontRebuild] テンプレート({templateMat.name}) からコピー → {updated} TMP に再アサイン");
    }

    // Fresh FontAsset のサブアセットから正規Material/Textureを取得して再アサイン
    [MenuItem("EscapeGame/Font/Fix Fresh Material")]
    public static void FixFreshMaterial()
    {
        const string newAssetPath = "Assets/_Project/Fonts/NotoSansJP_Fresh.asset";
        var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(newAssetPath);
        if (asset == null) { Debug.LogError("[FontRebuild] Fresh asset not found"); return; }

        var subs = AssetDatabase.LoadAllAssetsAtPath(newAssetPath);
        var matSub = subs.OfType<Material>().FirstOrDefault();
        var texSub = subs.OfType<Texture2D>().FirstOrDefault();
        Debug.Log($"[FontRebuild] サブアセット: mat={(matSub?.name ?? "なし")} tex={(texSub?.name ?? "なし")}");
        if (matSub == null) { Debug.LogError("[FontRebuild] Fresh の Material サブアセットがない"); return; }

        // SerializedObject で永続フィールドへ
        var serObj = new SerializedObject(asset);
        serObj.FindProperty("m_Material").objectReferenceValue = matSub;
        if (texSub != null)
        {
            var atlasArr = serObj.FindProperty("m_AtlasTextures");
            if (atlasArr.arraySize > 0) atlasArr.GetArrayElementAtIndex(0).objectReferenceValue = texSub;
            serObj.FindProperty("atlas").objectReferenceValue = texSub;
        }
        serObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();

        // 全 TMP に再アサイン
        var texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int updated = 0;
        foreach (var t in texts)
        {
            if (t.font == asset)
            {
                t.fontSharedMaterial = matSub;
                EditorUtility.SetDirty(t);
                updated++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[FontRebuild] {updated} TMP に正規Material({matSub.name}) を再アサイン");
    }

    // 全 TMP_Text の enabled を有効化 + sharedMaterials を更新
    [MenuItem("EscapeGame/Font/Enable All TMP")]
    public static void EnableAllTMP()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NotoSansJP_Fresh.asset");
        if (asset == null) { Debug.LogError("[FontRebuild] Fresh asset not found"); return; }
        var subs = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Fonts/NotoSansJP_Fresh.asset");
        var matSub = subs.OfType<Material>().FirstOrDefault();
        if (matSub == null) { Debug.LogError("[FontRebuild] Material サブアセットなし"); return; }

        var texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int enabledCount = 0, matFixed = 0;
        foreach (var t in texts)
        {
            if (!t.enabled) { t.enabled = true; enabledCount++; }
            if (t.font != asset) { t.font = asset; matFixed++; }
            t.fontSharedMaterial = matSub;
            // m_fontSharedMaterials の古い参照をクリア
            var so = new SerializedObject(t);
            var arr = so.FindProperty("m_fontSharedMaterials");
            if (arr != null) arr.arraySize = 0;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(t);
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[FontRebuild] enabled復元: {enabledCount}, font再設定: {matFixed} (全{texts.Length}件)");
    }

    // 正規API + サブアセット永続化版
    [MenuItem("EscapeGame/Font/Create Fresh FontAsset v2")]
    public static void CreateFreshFontAssetV2()
    {
        const string ttfPath = "Assets/_Project/Fonts/JapaneseFont.ttf";
        const string newAssetPath = "Assets/_Project/Fonts/NotoSansJP_Fresh.asset";
        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
        if (sourceFont == null) { Debug.LogError($"[FontRebuild] TTF読込失敗: {ttfPath}"); return; }

        // 既存削除
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(newAssetPath) != null)
            AssetDatabase.DeleteAsset(newAssetPath);

        // CreateFontAsset
        var newAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont, 90, 9,
            UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
            4096, 4096, AtlasPopulationMode.Dynamic, true);
        if (newAsset == null) { Debug.LogError("[FontRebuild] CreateFontAsset 失敗"); return; }
        AssetDatabase.CreateAsset(newAsset, newAssetPath);

        // Atlas Texture をサブアセット化
        Texture2D firstTex = null;
        if (newAsset.atlasTextures != null)
        {
            foreach (var tex in newAsset.atlasTextures)
            {
                if (tex == null) continue;
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(tex)))
                {
                    tex.name = newAsset.name + " Atlas";
                    AssetDatabase.AddObjectToAsset(tex, newAsset);
                }
                if (firstTex == null) firstTex = tex;
            }
        }

        // Material をサブアセット化（既に Asset 所属でなければ）
        Material mat = newAsset.material;
        if (mat == null || mat.name.Contains("Arial"))
        {
            // Material が間違っているか null → 手動で TMP shader から作成
            var shader = Shader.Find("TextMeshPro/Distance Field");
            if (shader == null) shader = Shader.Find("TextMeshPro/Mobile/Distance Field");
            mat = new Material(shader);
            mat.name = newAsset.name + " Material";
            if (firstTex != null) mat.SetTexture("_MainTex", firstTex);
        }
        else if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(mat)))
        {
            mat.name = newAsset.name + " Material";
        }
        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(mat)))
            AssetDatabase.AddObjectToAsset(mat, newAsset);

        // SerializedObject で永続フィールド更新
        var serObj = new SerializedObject(newAsset);
        serObj.FindProperty("m_Material").objectReferenceValue = mat;
        if (firstTex != null)
        {
            var atlasArr = serObj.FindProperty("m_AtlasTextures");
            if (atlasArr.arraySize == 0) atlasArr.InsertArrayElementAtIndex(0);
            atlasArr.GetArrayElementAtIndex(0).objectReferenceValue = firstTex;
            serObj.FindProperty("atlas").objectReferenceValue = firstTex;
        }
        serObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(newAsset);
        EditorUtility.SetDirty(mat);
        if (firstTex != null) EditorUtility.SetDirty(firstTex);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(newAssetPath, ImportAssetOptions.ForceUpdate);

        // 確認
        var subs = AssetDatabase.LoadAllAssetsAtPath(newAssetPath);
        var subNames = string.Join(", ", subs.Where(s => s != null).Select(s => $"{s.name}({s.GetType().Name})"));
        Debug.Log($"[FontRebuild] Fresh v2 サブアセット: {subNames}");

        // 全 TMP に再アサイン
        var loaded = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(newAssetPath);
        var texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int updated = 0;
        foreach (var t in texts) { t.font = loaded; t.fontSharedMaterial = mat; EditorUtility.SetDirty(t); updated++; }
        AssetDatabase.SaveAssets();
        Debug.Log($"[FontRebuild] {updated} TMP を Fresh v2 に切替（mat={mat.name}）");

        // 文字ベイク
        var chars = new HashSet<char>();
        foreach (var t in texts) { if (!string.IsNullOrEmpty(t.text)) foreach (var c in t.text) chars.Add(c); }
        foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (so == null) continue;
            var so2 = new SerializedObject(so);
            var prop = so2.GetIterator();
            while (prop.NextVisible(true))
                if (prop.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(prop.stringValue))
                    foreach (var c in prop.stringValue) chars.Add(c);
        }
        for (char c = ' '; c <= '~'; c++) chars.Add(c);
        var sb = new StringBuilder();
        foreach (var c in chars) sb.Append(c);
        loaded.TryAddCharacters(sb.ToString(), out var missing);
        EditorUtility.SetDirty(loaded);
        AssetDatabase.SaveAssets();
        Debug.Log($"[FontRebuild] {chars.Count} chars ベイク（失敗: {(missing != null ? missing.Length : 0)} 件）");
    }

    // 全TMPをNotoSansJP_Dynamicに戻す（日本語表示用、Liberation切替の逆操作）
    [MenuItem("EscapeGame/Font/Restore NotoSansJP")]
    public static void RestoreNotoSansJP()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
        if (asset == null) { Debug.LogError("[FontRebuild] NotoSansJP_Dynamic 読込失敗"); return; }
        if (asset.material == null) { Debug.LogError("[FontRebuild] asset.material が null。先に Force Rebuild Atlas を実行"); return; }

        var texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int updated = 0;
        foreach (var t in texts)
        {
            t.font = asset;
            t.fontSharedMaterial = asset.material;
            EditorUtility.SetDirty(t);
            updated++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[FontRebuild] {updated} TMP を NotoSansJP_Dynamic に復元");
    }

    // 軽量再構築（既存 Atlas を維持しつつ再読み込み）
    [MenuItem("EscapeGame/Font/Soft Rebuild")]
    public static void SoftRebuild()
    {
        AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
        var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
        if (asset == null) { Debug.LogError("[FontRebuild] Asset 読込失敗: " + AssetPath); return; }
        asset.ClearFontAssetData(true);
        asset.ReadFontAssetDefinition();
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssetIfDirty(asset);
        Debug.Log("[FontRebuild] Soft Rebuild 完了");
    }
}
#endif
