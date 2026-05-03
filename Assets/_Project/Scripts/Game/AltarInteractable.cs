using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    [RequireComponent(typeof(Collider))]
    public class AltarInteractable : MonoBehaviour
    {
        private void OnMouseDown()
        {
            if (RoomViewController.Instance.CurrentArea != RoomArea.Altar) return;
            AudioManager.Instance?.PlaySE("SE_Click");
            FindAnyObjectByType<AltarUI>()?.Show();
        }
    }
}
