using UnityEngine;
using UnityEngine.InputSystem;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // チェスト天板の1個のシンボルダイヤル
    // クリックで次のシンボルへ循環。Chestエリアにいる間のみ反応。
    public class ChestSymbolDial : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer display;
        [SerializeField] private Sprite[] symbols;
        [SerializeField] private ChestPuzzle puzzle;

        private int current;
        public int CurrentSymbol => current;

        private void Start() => UpdateDisplay();

        private void Update()
        {
            if (RoomViewController.Instance == null) return;
            if (RoomViewController.Instance.CurrentArea != RoomArea.Chest) return;
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

            var cam = Camera.main;
            if (cam == null) return;

            var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit)) return;
            if (hit.transform != transform) return;

            Cycle();
        }

        public void Cycle()
        {
            if (puzzle != null && puzzle.IsSolved) return;
            if (symbols == null || symbols.Length == 0) return;
            current = (current + 1) % symbols.Length;
            UpdateDisplay();
            AudioManager.Instance?.PlaySE("SE_Click");
            puzzle?.OnDialChanged();
        }

        public void SetSymbol(int idx)
        {
            if (symbols == null || symbols.Length == 0) return;
            current = Mathf.Clamp(idx, 0, symbols.Length - 1);
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (display != null && symbols != null && current >= 0 && current < symbols.Length)
                display.sprite = symbols[current];
        }
    }
}
