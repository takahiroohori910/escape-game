#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 電話・メモ・写真の位置と外観を自然に整える
/// </summary>
public class RepositionSetup
{
    private const string MAT_DIR = "Assets/_Project/Materials/Generated/";

    [MenuItem("EscapeGame/Setup/Reposition Props")]
    public static void Run()
    {
        if (Application.isPlaying) { Debug.LogError("[Repo] Edit mode で実行してください"); return; }

        RepositionTelephone();
        RepositionNotes();
        RepositionFireplacePhoto();

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Repo] プロップ再配置完了");
    }

    // ── 電話を机の上に移動 ────────────────────────────────────────────────
    private static void RepositionTelephone()
    {
        var tel = GameObject.Find("Telephone");
        if (tel == null) { Debug.LogWarning("[Repo] Telephone が見つかりません"); return; }

        // 机（DeskTop）の位置を基準に
        var deskTop = GameObject.Find("DeskTop");
        Vector3 deskPos = deskTop != null ? deskTop.transform.position : new Vector3(0f, 0.9f, 0.5f);

        // 机の左端・上面に配置
        tel.transform.position   = new Vector3(deskPos.x - 0.55f, deskPos.y + 0.12f, deskPos.z + 0.1f);
        tel.transform.localScale = new Vector3(0.22f, 0.14f, 0.28f);
        tel.transform.rotation   = Quaternion.Euler(0f, -15f, 0f);

        // 黒い電話マテリアル
        ApplyColorMat(tel, new Color(0.10f, 0.10f, 0.10f), "Mat_Telephone");

        // 受話器パーツ（見た目の強化）
        var existing = GameObject.Find("TelephoneHandset");
        if (existing != null) Object.DestroyImmediate(existing);

        var handset = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handset.name = "TelephoneHandset";
        handset.transform.position   = new Vector3(deskPos.x - 0.55f, deskPos.y + 0.22f, deskPos.z + 0.05f);
        handset.transform.localScale = new Vector3(0.06f, 0.05f, 0.24f);
        handset.transform.rotation   = Quaternion.Euler(0f, -15f, 10f);
        Object.DestroyImmediate(handset.GetComponent<BoxCollider>());
        ApplyColorMat(handset, new Color(0.08f, 0.08f, 0.08f), "Mat_TelHandset");

        EditorUtility.SetDirty(tel);
        Debug.Log("[Repo] 電話を机の上に移動完了");
    }

    // ── メモを自然な位置・外観に ──────────────────────────────────────────
    private static void RepositionNotes()
    {
        // 机のメモ：机の中央に広げた紙として
        var noteDesk = GameObject.Find("NoteOnDesk");
        if (noteDesk != null)
        {
            var deskTop = GameObject.Find("DeskTop");
            Vector3 dp = deskTop != null ? deskTop.transform.position : new Vector3(0f, 0.9f, 0.5f);
            noteDesk.transform.position   = new Vector3(dp.x + 0.2f, dp.y + 0.05f, dp.z);
            noteDesk.transform.localScale = new Vector3(0.30f, 0.005f, 0.22f);
            noteDesk.transform.rotation   = Quaternion.Euler(0f, 12f, 0f);
            ApplyColorMat(noteDesk, new Color(0.92f, 0.88f, 0.78f), "Mat_NoteDesk");
            EditorUtility.SetDirty(noteDesk);
        }

        // 書棚のメモ：棚の隙間に差し込んだ紙として
        var noteShelf = GameObject.Find("NoteOnBookshelf");
        if (noteShelf != null)
        {
            noteShelf.transform.position   = new Vector3(-4.0f, 2.28f, 5.55f);
            noteShelf.transform.localScale = new Vector3(0.18f, 0.24f, 0.02f);
            noteShelf.transform.rotation   = Quaternion.Euler(0f, 0f, 8f); // 少し傾いた手紙
            ApplyColorMat(noteShelf, new Color(0.88f, 0.84f, 0.72f), "Mat_NoteShelf");
            EditorUtility.SetDirty(noteShelf);
        }

        // 暖炉のメモ：マントルに置かれた封筒/手紙として
        var noteFireplace = GameObject.Find("NoteOnFireplace");
        if (noteFireplace != null)
        {
            noteFireplace.transform.position   = new Vector3(5.2f, 2.38f, 5.72f); // マントル上
            noteFireplace.transform.localScale = new Vector3(0.20f, 0.005f, 0.14f);
            noteFireplace.transform.rotation   = Quaternion.Euler(0f, -20f, 0f);
            ApplyColorMat(noteFireplace, new Color(0.86f, 0.80f, 0.65f), "Mat_NoteFireplace");
            EditorUtility.SetDirty(noteFireplace);
        }

        Debug.Log("[Repo] メモ再配置完了");
    }

    // ── 暖炉の写真（本の並び順ヒント）をマントルに自然に配置 ─────────────
    private static void RepositionFireplacePhoto()
    {
        var photo = GameObject.Find("FireplacePhoto");
        if (photo == null) { Debug.LogWarning("[Repo] FireplacePhoto が見つかりません"); return; }

        // マントルに立てかけた額縁として（壁から少し離して視認性確保）
        photo.transform.position   = new Vector3(4.5f, 2.72f, 5.48f);
        photo.transform.localScale = new Vector3(0.55f, 0.44f, 0.04f);
        photo.transform.rotation   = Quaternion.Euler(3f, 5f, 0f);  // 自然に立てかけた傾き

        // 羊皮紙色で目立つように
        ApplyColorMat(photo, new Color(0.88f, 0.78f, 0.55f), "Mat_PhotoFrame");
        EditorUtility.SetDirty(photo);
        Debug.Log("[Repo] 暖炉写真（額縁）再配置完了");
    }

    // ── ユーティリティ ───────────────────────────────────────────────────
    private static void ApplyColorMat(GameObject go, Color color, string matName)
    {
        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer == null) return;
        if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);
        string path = MAT_DIR + matName + ".mat";
        if (System.IO.File.Exists(path)) AssetDatabase.DeleteAsset(path);
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", 0.1f);
        AssetDatabase.CreateAsset(mat, path);
        renderer.sharedMaterial = mat;
        EditorUtility.SetDirty(go);
    }
}
#endif
