using UnityEngine;

namespace EscapeGame.Core
{
    // アイテム1種のデータを定義するScriptableObject
    // Project窓で右クリック → Create → EscapeGame → ItemData で作成可能
    [CreateAssetMenu(fileName = "NewItemData", menuName = "EscapeGame/ItemData")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string itemId;           // アイテムを一意に識別するID（例: "key_01"）
        [SerializeField] private string itemName;         // 表示名（例: "古びた鍵"）
        [SerializeField] private string description;      // アイテム説明文
        [SerializeField] private Sprite icon;             // インベントリに表示するアイコン画像
        [SerializeField] private ItemData[] craftingIngredients; // 合成に必要な素材リスト（空なら合成不可）
        [SerializeField] private ItemData craftingResult;        // 合成結果のアイテム（nullなら合成不可）

        public string ItemId => itemId;
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public ItemData[] CraftingIngredients => craftingIngredients;
        public ItemData CraftingResult => craftingResult;

        // 合成可能かどうかを返す
        public bool CanCraft => craftingIngredients != null && craftingIngredients.Length > 0 && craftingResult != null;
    }
}
