using UnityEngine;
using UnityEngine.InputSystem;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 肖像画の紋章をクリックする
    // OnMouseDownは新InputSystem環境で発火しないためRaycastで代替
    [RequireComponent(typeof(Collider))]
    public class PortraitSymbolInteractable : MonoBehaviour
    {
        [SerializeField] private int symbolIndex; // 0=指輪, 1=剣, 2=王冠, 3=書物

        private PortraitPuzzle puzzle;
        private Collider col;

        private void Awake()
        {
            puzzle = FindAnyObjectByType<PortraitPuzzle>();
            col = GetComponent<Collider>();
        }

        private void Update()
        {
            // Pointer は Mouse / Touchscreen / Pen の共通親。iOS Touch でも反応する
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame) return;
            if (RoomViewController.Instance.CurrentArea != RoomArea.Portrait) return;
            if (puzzle == null || puzzle.IsSolved) return;

            var ray = Camera.main.ScreenPointToRay(pointer.position.ReadValue());
            if (Physics.Raycast(ray, out var hit) && hit.collider == col)
            {
                AudioManager.Instance?.PlaySE("SE_Click");
                puzzle.OnSymbolClicked(symbolIndex);
            }
        }
    }
}
