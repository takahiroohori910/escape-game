#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class BookshelfReskinSetup
{
    [MenuItem("EscapeGame/Setup/Reskin Bookshelf")]
    public static void Run()
    {
        if (Application.isPlaying) { Debug.LogError("[BookshelfReskin] Edit mode で実行してください"); return; }

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        // 本棚フレーム・背板（濃いオーク）
        Color darkOak  = new Color(0.22f, 0.12f, 0.05f);
        Color lightOak = new Color(0.35f, 0.20f, 0.09f);

        Apply("Bookshelf",      shader, darkOak,  0.10f);
        Apply("Bookshelf_Back", shader, new Color(0.18f, 0.10f, 0.04f), 0.05f);
        Apply("Shelf_Bottom",   shader, lightOak, 0.25f);
        Apply("Shelf_Mid",      shader, lightOak, 0.25f);
        Apply("Shelf_Top",      shader, lightOak, 0.25f);

        // パズル本（色は BookInteractable の MaterialPropertyBlock が上書きするので初期値は何でもよい）
        Apply("Book_01", shader, new Color(0.20f, 0.40f, 0.85f), 0.30f);
        Apply("Book_02", shader, new Color(0.80f, 0.20f, 0.20f), 0.30f);
        Apply("Book_03", shader, new Color(0.20f, 0.70f, 0.30f), 0.30f);

        // フィラー本の配色（書斎らしい落ち着いた色）
        Color[] palette =
        {
            new Color(0.45f, 0.14f, 0.08f), // 深赤
            new Color(0.10f, 0.20f, 0.42f), // ダークブルー
            new Color(0.08f, 0.28f, 0.14f), // ダークグリーン
            new Color(0.36f, 0.26f, 0.07f), // マスタード
            new Color(0.38f, 0.18f, 0.28f), // モーブ
            new Color(0.48f, 0.32f, 0.10f), // 焦げ茶
            new Color(0.13f, 0.30f, 0.36f), // ダークティール
            new Color(0.36f, 0.10f, 0.10f), // バーガンディ
        };

        for (int s = 0; s <= 2; s++)
        for (int b = 0; b <= 4; b++)
            Apply($"BookFiller_S{s}_B{b}", shader, palette[(b + s * 3) % palette.Length], 0.12f);

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[BookshelfReskin] 本棚の色替え完了");
    }

    static void Apply(string goName, Shader shader, Color color, float smoothness)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"[BookshelfReskin] {goName} not found"); return; }
        var r = go.GetComponent<MeshRenderer>() ?? go.GetComponentInChildren<MeshRenderer>();
        if (r == null) return;
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", smoothness);
        r.sharedMaterial = mat;
        EditorUtility.SetDirty(go);
    }
}
#endif
