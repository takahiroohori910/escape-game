using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // インベントリのアイテムをクリックしたとき詳細を表示する
    public class ItemDetailUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descText;

        private bool showedThisFrame;

        private void Awake() => panel.SetActive(false);

        public void Show(ItemData item)
        {
            if (item == null) { Hide(); return; }
            nameText.text = item.ItemName;
            descText.text = item.Description;
            panel.SetActive(true);
            showedThisFrame = true;
        }

        public void Hide() => panel.SetActive(false);

        private void Update()
        {
            if (showedThisFrame) { showedThisFrame = false; return; }
            if (panel.activeSelf && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) Hide();
        }
    }
}
