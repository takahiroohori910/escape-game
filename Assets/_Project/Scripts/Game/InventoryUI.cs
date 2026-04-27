using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 画面下部にインベントリスロットを表示する
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Transform slotContainer;

        [SerializeField] private ItemDetailUI itemDetailUI;

        private readonly List<(Image bg, TextMeshProUGUI label)> slots = new();
        private IReadOnlyList<ItemData> cachedItems = new List<ItemData>();

        private void Start()
        {
            InventoryManager.Instance.OnInventoryChanged.AddListener(Refresh);
            InitSlots(8);
        }

        public void InitSlots(int count)
        {
            foreach (Transform child in slotContainer) Destroy(child.gameObject);
            slots.Clear();
            for (int i = 0; i < count; i++)
            {
                var slot = Instantiate(slotPrefab, slotContainer);
                var bg = slot.GetComponent<Image>();
                var lbl = slot.GetComponentInChildren<TextMeshProUGUI>();
                slots.Add((bg, lbl));

                var btn = slot.GetComponent<UnityEngine.UI.Button>() ?? slot.AddComponent<UnityEngine.UI.Button>();
                int idx = i;
                btn.onClick.AddListener(() => OnSlotClicked(idx));
            }
        }

        private void OnSlotClicked(int idx)
        {
            if (idx < cachedItems.Count)
                itemDetailUI?.Show(cachedItems[idx]);
        }

        private void Refresh(IReadOnlyList<ItemData> items)
        {
            cachedItems = items;
            for (int i = 0; i < slots.Count; i++)
            {
                bool occupied = i < items.Count;
                slots[i].bg.color = occupied
                    ? new Color(0.3f, 0.55f, 0.3f, 0.9f)
                    : new Color(0.15f, 0.15f, 0.15f, 0.7f);
                slots[i].label.text = occupied ? items[i].ItemName : "";
            }
        }
    }
}
