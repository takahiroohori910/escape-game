#if UNITY_EDITOR
using EscapeGame.Game;
using UnityEditor;
using UnityEngine;

public class InteractableSetup
{
    [MenuItem("EscapeGame/Setup/Attach Interactables")]
    public static void AttachInteractables()
    {
        // ── エリア移動 ──────────────────────────────────────────────
        AttachAreaClickZone("Bookshelf",               RoomArea.Bookshelf);
        AttachAreaClickZone("DeskTop",                 RoomArea.Desk);
        AttachAreaClickZone("Fireplace/FireplaceFrame", RoomArea.Fireplace);

        // ── 本 ────────────────────────────────────────────────────
        AttachBookInteractable("Book_01", 0);
        AttachBookInteractable("Book_02", 1);
        AttachBookInteractable("Book_03", 2);

        // ── 絵画 ──────────────────────────────────────────────────
        var painting = GameObject.Find("Painting");
        if (painting != null)
        {
            EnsureCollider(painting);
            if (painting.GetComponent<PaintingInteractable>() == null)
                painting.AddComponent<PaintingInteractable>();
            Debug.Log("[InteractableSetup] Painting ✅");
        }

        // ── 電話 ──────────────────────────────────────────────────
        var telephone = GameObject.Find("Telephone");
        if (telephone != null)
        {
            EnsureCollider(telephone);
            if (telephone.GetComponent<TelephoneInteractable>() == null)
                telephone.AddComponent<TelephoneInteractable>();
            Debug.Log("[InteractableSetup] Telephone ✅");
        }

        // ── MENUボタン ────────────────────────────────────────────
        var menuButton = GameObject.Find("MenuButton");
        if (menuButton != null)
        {
            if (menuButton.GetComponent<BackButton>() == null)
                menuButton.AddComponent<BackButton>();
            Debug.Log("[InteractableSetup] MenuButton ✅");
        }

        EditorUtility.SetDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()[0]);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[InteractableSetup] 完了・シーン保存しました");
    }

    private static void AttachAreaClickZone(string path, RoomArea area)
    {
        var go = GameObject.Find(path.Contains("/") ? path.Split('/')[1] : path);
        if (go == null) { Debug.LogWarning($"[InteractableSetup] {path} が見つかりません"); return; }

        EnsureCollider(go);
        var zone = go.GetComponent<AreaClickZone>() ?? go.AddComponent<AreaClickZone>();
        var so = new SerializedObject(zone);
        so.FindProperty("targetArea").enumValueIndex = (int)area;
        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log($"[InteractableSetup] {go.name} → {area} ✅");
    }

    private static void AttachBookInteractable(string name, int index)
    {
        var go = GameObject.Find(name);
        if (go == null) { Debug.LogWarning($"[InteractableSetup] {name} が見つかりません"); return; }

        EnsureCollider(go);
        var book = go.GetComponent<BookInteractable>() ?? go.AddComponent<BookInteractable>();
        var so = new SerializedObject(book);
        so.FindProperty("bookIndex").intValue = index;
        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log($"[InteractableSetup] {name} bookIndex={index} ✅");
    }

    private static void EnsureCollider(GameObject go)
    {
        if (go.GetComponent<Collider>() == null)
            go.AddComponent<BoxCollider>();
    }
}
#endif
