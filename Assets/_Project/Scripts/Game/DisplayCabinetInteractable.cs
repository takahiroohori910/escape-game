using UnityEngine;
using UnityEngine.InputSystem;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    [RequireComponent(typeof(Collider))]
    public class DisplayCabinetInteractable : MonoBehaviour
    {
        private Collider col;

        private void Awake() => col = GetComponent<Collider>();

        private void Update()
        {
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            if (RoomViewController.Instance.CurrentArea != RoomArea.DisplayCabinet) return;

            var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out var hit) && hit.collider == col)
            {
                AudioManager.Instance?.PlaySE("SE_Click");
                FindAnyObjectByType<DisplayCabinetUI>()?.Show();
            }
        }
    }
}
