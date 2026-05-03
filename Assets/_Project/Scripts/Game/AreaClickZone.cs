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
        private Collider col;

        private void Awake() => col = GetComponent<Collider>();

        private void Update()
        {
            if (RoomViewController.Instance == null) return;
            col.enabled = RoomViewController.Instance.IsOverview;
        }

        private void OnMouseDown()
        {
            if (!RoomViewController.Instance.IsOverview) return;
            EscapeGame.Core.AudioManager.Instance?.PlaySE("SE_CameraMove");
            RoomViewController.Instance.MoveTo(targetArea);
        }
    }
}
