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

        private void Update()
        {
            UpdateDisplay();

            // Pointer は Mouse / Touchscreen / Pen の共通親。iOS Touch でも反応する
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame) return;
            if (RoomViewController.Instance == null) return;
            if (RoomViewController.Instance.CurrentArea != RoomArea.DisplayCabinet) return;
            if (puzzle == null || puzzle.IsSolved) return;

            var cam = Camera.main;
            if (cam == null) return;
            var ray = cam.ScreenPointToRay(pointer.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit)) return;
            // 同じ GameObject に SphereCollider と BoxCollider が同居するため transform で比較
            if (hit.transform != transform) return;

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
