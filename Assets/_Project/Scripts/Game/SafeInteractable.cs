using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    [RequireComponent(typeof(Collider))]
    public class SafeInteractable : MonoBehaviour
    {
        private void OnMouseDown()
        {
            if (RoomViewController.Instance.CurrentArea != RoomArea.Desk) return;
            AudioManager.Instance?.PlaySE("SE_Click");
            FindAnyObjectByType<NumberPadUI>()?.Show();
        }
    }
}
