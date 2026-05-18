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
        [SerializeField] private string requiredFlag = "";
        [SerializeField] private string blockedMessage = "";
        private Collider col;

        private void Awake() => col = GetComponent<Collider>();

        private void Update()
        {
            if (RoomViewController.Instance == null) return;
            // Title overlay 表示中は Unity の OnMouseDown が UI Raycast を貫通して
            // 誤発火するため、Collider 自体を無効化して防ぐ
            bool titleShown = TitleUI.Instance != null && TitleUI.Instance.IsShowing;
            col.enabled = RoomViewController.Instance.IsOverview && !titleShown;
        }

        private void OnMouseDown()
        {
            if (!RoomViewController.Instance.IsOverview) return;
            if (TitleUI.Instance != null && TitleUI.Instance.IsShowing) return;

            if (!string.IsNullOrEmpty(requiredFlag) && !FlagManager.Instance.HasFlag(requiredFlag))
            {
                if (!string.IsNullOrEmpty(blockedMessage))
                    PopupUI.Instance?.Show(blockedMessage);
                return;
            }

            EscapeGame.Core.AudioManager.Instance?.PlaySE("SE_CameraMove");
            RoomViewController.Instance.MoveTo(targetArea);
        }
    }
}
