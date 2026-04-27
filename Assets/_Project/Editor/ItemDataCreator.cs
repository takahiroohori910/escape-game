#if UNITY_EDITOR
using EscapeGame.Core;
using UnityEditor;
using UnityEngine;

public class ItemDataCreator
{
    [MenuItem("EscapeGame/Setup/Create Item Assets")]
    public static void CreateItemAssets()
    {
        const string dir = "Assets/_Project/ScriptableObjects/Items";
        if (!AssetDatabase.IsValidFolder("Assets/_Project/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets/_Project", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Items");

        CreateItem(dir, "PhoneCord",    "phone_cord",    "受話器コード",  "古い電話機のコード。修理に使えそうだ。");
        CreateItem(dir, "CircuitBoard", "circuit_board", "内部基板",      "電話機の内部基板。これがあれば修理できる。");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ItemDataCreator] PhoneCord・CircuitBoard を作成しました");
    }

    private static void CreateItem(string dir, string fileName, string id, string displayName, string desc)
    {
        string path = $"{dir}/{fileName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        if (existing != null)
        {
            Debug.Log($"[ItemDataCreator] {fileName} は既に存在します");
            return;
        }

        var item = ScriptableObject.CreateInstance<ItemData>();
        var so = new SerializedObject(item);
        so.FindProperty("itemId").stringValue      = id;
        so.FindProperty("itemName").stringValue    = displayName;
        so.FindProperty("description").stringValue = desc;
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(item, path);
        Debug.Log($"[ItemDataCreator] 作成: {path}");
    }
}
#endif
