using UnityEngine;
using UnityEngine.InputSystem;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 個々の燭台をクリックして点灯/消灯を切り替える
    // OnMouseDownは新InputSystem環境で発火しないためRaycastで代替
    [RequireComponent(typeof(Collider))]
    public class CandleInteractable : MonoBehaviour
    {
        [SerializeField] private int candleIndex; // 0-indexed
        [SerializeField] private GameObject flameObject; // 炎の表示オブジェクト

        private CandelabraPuzzle puzzle;
        private Collider col;

        private void Awake()
        {
            puzzle = FindAnyObjectByType<CandelabraPuzzle>();
            col = GetComponent<Collider>();
        }

        private void Start()
        {
            RefreshFlame();
        }

        private void Update()
        {
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            if (RoomViewController.Instance.CurrentArea != RoomArea.Candelabra) return;
            if (puzzle == null || puzzle.IsSolved) return;

            var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out var hit) && hit.collider == col)
            {
                puzzle.ToggleCandle(candleIndex);
                RefreshFlame();
            }
        }

        private void RefreshFlame()
        {
            if (flameObject != null)
                flameObject.SetActive(puzzle != null && puzzle.IsLit(candleIndex));
        }
    }
}
