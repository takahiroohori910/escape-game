using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 3Dオブジェクトをクリック/タップしてエリア移動するコンポーネント
    // Overview時のみ移動、既にそのエリアにいる場合は何もしない
    [RequireComponent(typeof(Collider))]
    public class AreaClickZone : MonoBehaviour
    {
        [SerializeField] private RoomArea targetArea;

        private void OnMouseDown()
        {
            if (RoomViewController.Instance.CurrentArea != RoomArea.Overview) return;
            EscapeGame.Core.AudioManager.Instance?.PlaySE("SE_CameraMove");
            RoomViewController.Instance.MoveTo(targetArea);
        }
    }
}
