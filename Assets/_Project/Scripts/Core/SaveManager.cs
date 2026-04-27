using System.Collections.Generic;
using UnityEngine;

namespace EscapeGame.Core
{
    // PlayerPrefsを使ってフラグ・インベントリ・ベストタイムを保存/復元する
    public class SaveManager : SingletonMonoBehaviour<SaveManager>
    {
        private const string KeyFlags     = "EG_Flags";
        private const string KeyInventory = "EG_Inventory";
        private const string KeyBestTime  = "EG_BestTime";
        private const string KeyExists    = "EG_Exists";

        [SerializeField] private ItemData[] allItems;

        public static bool HasSave => PlayerPrefs.GetInt(KeyExists, 0) == 1;

        private void Start()
        {
            // プレイ中の変化を自動保存
            FlagManager.Instance.OnFlagChanged.AddListener((_, _) => Save());
            InventoryManager.Instance.OnInventoryChanged.AddListener(_ => Save());
        }

        public void Save()
        {
            PlayerPrefs.SetString(KeyFlags,     FlagManager.Instance.SerializeFlags());
            PlayerPrefs.SetString(KeyInventory, SerializeInventory());
            PlayerPrefs.SetInt   (KeyExists,    1);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            if (!HasSave) return;

            FlagManager.Instance.DeserializeFlags(PlayerPrefs.GetString(KeyFlags, ""));
            DeserializeInventory(PlayerPrefs.GetString(KeyInventory, ""));
        }

        public void ClearSave()
        {
            PlayerPrefs.DeleteKey(KeyFlags);
            PlayerPrefs.DeleteKey(KeyInventory);
            PlayerPrefs.DeleteKey(KeyExists);
            PlayerPrefs.Save();
        }

        public void SaveBestTime(float seconds)
        {
            float current = PlayerPrefs.GetFloat(KeyBestTime, float.MaxValue);
            if (seconds < current)
            {
                PlayerPrefs.SetFloat(KeyBestTime, seconds);
                PlayerPrefs.Save();
            }
        }

        public float GetBestTime() => PlayerPrefs.GetFloat(KeyBestTime, float.MaxValue);

        private string SerializeInventory()
        {
            var ids = new List<string>();
            foreach (var item in InventoryManager.Instance.GetItems())
                ids.Add(item.ItemId);
            return string.Join(",", ids);
        }

        private void DeserializeInventory(string data)
        {
            if (string.IsNullOrEmpty(data) || allItems == null) return;
            var dict = new Dictionary<string, ItemData>();
            foreach (var item in allItems)
                if (item != null) dict[item.ItemId] = item;

            foreach (var id in data.Split(','))
            {
                if (dict.TryGetValue(id, out var item))
                    InventoryManager.Instance.AddItem(item);
            }
        }
    }
}
