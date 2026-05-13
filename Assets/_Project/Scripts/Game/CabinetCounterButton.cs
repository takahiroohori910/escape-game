using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 食器棚の解錠装置の1ボタン。クリックで対応カウンタを+1、隣の数字表示を更新
    [RequireComponent(typeof(Collider))]
    public class CabinetCounterButton : MonoBehaviour
    {
        public enum CounterTarget { Top, Mid, Bot }

        [SerializeField] private CounterTarget target;
        [SerializeField] private DisplayCabinetPuzzle puzzle;
        [SerializeField] private TextMeshPro displayText;

        private Collider col;

        private void Awake() => col = GetComponent<Collider>();

        private void Update()
        {
            UpdateDisplay();

            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            if (RoomViewController.Instance == null) return;
            if (RoomViewController.Instance.CurrentArea != RoomArea.DisplayCabinet) return;
            if (puzzle == null || puzzle.IsSolved) return;

            var cam = Camera.main;
            if (cam == null) return;
            var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit)) return;
            if (hit.collider != col) return;

            switch (target)
            {
                case CounterTarget.Top: puzzle.IncrementTop(); break;
                case CounterTarget.Mid: puzzle.IncrementMid(); break;
                case CounterTarget.Bot: puzzle.IncrementBot(); break;
            }
        }

        private void UpdateDisplay()
        {
            if (displayText == null || puzzle == null) return;
            int v = target switch
            {
                CounterTarget.Top => puzzle.GetTop(),
                CounterTarget.Mid => puzzle.GetMid(),
                _ => puzzle.GetBot(),
            };
            string s = v.ToString();
            if (displayText.text != s) displayText.text = s;
        }
    }
}
