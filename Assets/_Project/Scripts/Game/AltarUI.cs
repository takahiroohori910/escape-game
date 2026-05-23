using UnityEngine;
using TMPro;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 祭壇の暗証番号入力UI
    public class AltarUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI codeDisplay;

        private AltarPuzzle puzzle;
        private bool solved;

        private void Awake()
        {
            puzzle = FindAnyObjectByType<AltarPuzzle>();
            if (puzzle != null) puzzle.OnSolved.AddListener(OnSolved);
            else Debug.LogError("[AltarUI] AltarPuzzleが見つかりません");
            if (panel != null) panel.SetActive(false);
        }

        private void Update()
        {
            if (solved || puzzle == null) return;
            if (panel.activeSelf && RoomViewController.Instance.CurrentArea != RoomArea.Altar)
                panel.SetActive(false);
        }

        public void Show()
        {
            if (solved || puzzle == null) return;
            panel.SetActive(true);
            RefreshDisplay();
        }

        public void OnDigitPressed(string digit)
        {
            puzzle?.InputDigit(digit);
            RefreshDisplay();
        }

        public void OnClearPressed()
        {
            puzzle?.ClearInput();
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (puzzle == null) return;
            string code = puzzle.GetEnteredCode();
            string display = "";
            for (int i = 0; i < 4; i++)
                display += (i < code.Length ? code[i].ToString() : "_") + (i < 3 ? "  " : "");
            codeDisplay.text = display;
        }

        private void OnSolved()
        {
            solved = true;
            codeDisplay.text = "解錠！";
            Invoke(nameof(HidePanel), 1.5f);
        }

        private void HidePanel() => panel.SetActive(false);
    }
}
