using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeGame.Core
{
    // アイテムの取得・選択・使用・合成を一括管理するマネージャー
    public class InventoryManager : SingletonMonoBehaviour<InventoryManager>
    {
        [SerializeField] private int maxSlots = 8; // 最大所持スロット数

        private readonly List<ItemData> items = new();
        private ItemData selectedItem;

        // アイテムリストが変化したときのイベント（読み取り専用で渡すことで外部改ざんを防ぐ）
        public UnityEvent<IReadOnlyList<ItemData>> OnInventoryChanged = new();
        public UnityEvent<ItemData> OnItemSelected = new();

        // アイテムを取得してインベントリに追加する
        public bool AddItem(ItemData item)
        {
            if (items.Count >= maxSlots)
            {
                Debug.LogWarning("[InventoryManager] インベントリが満杯です");
                return false;
            }
            items.Add(item);
            OnInventoryChanged.Invoke(items.AsReadOnly());
            Debug.Log($"[InventoryManager] アイテム取得: {item.ItemName}");
            return true;
        }

        // アイテムを使用してインベントリから除去する
        public bool UseItem(ItemData item)
        {
            if (!items.Remove(item)) return false;
            if (selectedItem == item) selectedItem = null;
            OnInventoryChanged.Invoke(items.AsReadOnly());
            return true;
        }

        // アイテムを選択状態にする（パズルへの適用などに使う）
        public void SelectItem(ItemData item)
        {
            selectedItem = item;
            OnItemSelected.Invoke(item);
            Debug.Log($"[InventoryManager] アイテム選択: {item?.ItemName ?? "なし"}");
        }

        // 選択中のアイテムを取得する
        public ItemData GetSelectedItem() => selectedItem;

        // 合成を試みる（素材が全て揃っていれば合成結果をインベントリに追加）
        public bool TryCraft(ItemData recipe)
        {
            if (!recipe.CanCraft) return false;

            foreach (var ingredient in recipe.CraftingIngredients)
            {
                if (!items.Contains(ingredient))
                {
                    Debug.Log($"[InventoryManager] 合成素材が不足: {ingredient.ItemName}");
                    return false;
                }
            }

            // 素材を消費して結果アイテムを追加
            foreach (var ingredient in recipe.CraftingIngredients)
            {
                items.Remove(ingredient);
            }
            AddItem(recipe.CraftingResult);
            Debug.Log($"[InventoryManager] 合成成功: {recipe.CraftingResult.ItemName}");
            return true;
        }

        // 現在の所持アイテムリストを読み取り専用で返す
        public IReadOnlyList<ItemData> GetItems() => items.AsReadOnly();
    }
}
