using UnityEngine;
using UnityEngine.InputSystem;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 食器棚の解錠装置の確定/リセットレバー
    [RequireComponent(typeof(Collider))]
    public class CabinetLever : MonoBehaviour
    {
        public enum LeverMode { Submit, Reset }

        [SerializeField] private LeverMode mode;
        [SerializeField] private DisplayCabinetPuzzle puzzle;

        private void Update()
        {
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
            if (hit.transform != transform) return;

            if (mode == LeverMode.Submit) puzzle.Submit();
            else puzzle.ResetCounters();
        }
    }
}
