using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 暖炉の上の写真：本の正しい並び順のヒント（青→赤→緑）
    [RequireComponent(typeof(Collider))]
    public class FireplacePhotoInteractable : MonoBehaviour
    {
        private void OnMouseDown()
        {
            if (RoomViewController.Instance.CurrentArea != RoomArea.Fireplace) return;
            AudioManager.Instance?.PlaySE("SE_Click");
        }
    }
}
