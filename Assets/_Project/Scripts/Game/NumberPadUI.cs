using UnityEngine;
using TMPro;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    public class NumberPadUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI codeDisplay;
        [SerializeField] private TextMeshProUGUI hintText;

        private DeskPuzzle deskPuzzle;
        private bool solved;

        private void Awake()
        {
            deskPuzzle = FindAnyObjectByType<DeskPuzzle>();
            deskPuzzle.OnSolved.AddListener(OnSolved);
            panel.SetActive(false);
        }

        private void Update()
        {
            if (solved) return;
            // Desk エリアを離れたら自動で閉じる
            if (panel.activeSelf && RoomViewController.Instance.CurrentArea != RoomArea.Desk)
                panel.SetActive(false);
            if (panel.activeSelf) RefreshDisplay();
        }

        public void Show()
        {
            if (solved) return;
            panel.SetActive(true);
            RefreshDisplay();
        }

        public void OnDigitPressed(string digit)
        {
            deskPuzzle.InputDigit(digit);
            RefreshDisplay();
        }

        public void OnClearPressed()
        {
            deskPuzzle.ClearInput();
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            string code = deskPuzzle.GetEnteredCode();
            string display = "";
            for (int i = 0; i < 4; i++)
                display += (i < code.Length ? code[i].ToString() : "—") + (i < 3 ? "  " : "");
            codeDisplay.text = display;
        }

        private void OnSolved()
        {
            solved = true;
            codeDisplay.text = "解錠！";
            Invoke(nameof(HidePanel), 1.2f);
        }

        private void HidePanel() => panel.SetActive(false);
    }
}
