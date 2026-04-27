using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 壁掛け時計：クリックすると時刻をポップアップ表示する
    [RequireComponent(typeof(Collider))]
    public class ClockInteractable : MonoBehaviour
    {
        [SerializeField] private RoomArea requiredArea = RoomArea.Overview;

        private void OnMouseDown()
        {
            if (RoomViewController.Instance.CurrentArea != requiredArea) return;
            FlagManager.Instance.SetFlag(Flags.ClockInspected);
            AudioManager.Instance?.PlaySE("SE_Click");
        }
    }
}
