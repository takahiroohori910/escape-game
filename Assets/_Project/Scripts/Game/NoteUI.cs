using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // メモ・日記を全画面オーバーレイで表示する
    public class NoteUI : MonoBehaviour
    {
        public static NoteUI Instance { get; private set; }

        [SerializeField] private GameObject overlay;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button closeButton;

        private bool showedThisFrame;

        private void Awake()
        {
            Instance = this;
            overlay.SetActive(false);
            if (closeButton) closeButton.onClick.AddListener(Hide);
        }

        public void Show(NoteData note)
        {
            titleText.text   = note.Title;
            contentText.text = note.Content;
            overlay.SetActive(true);
            showedThisFrame = true;
        }

        public void Hide() => overlay.SetActive(false);

        private void Update()
        {
            if (!overlay.activeSelf) return;
            if (showedThisFrame) { showedThisFrame = false; return; }
            // Pointer は Mouse / Touchscreen / Pen の共通親。iOS Touch でも close できる
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) Hide();
        }
    }
}
