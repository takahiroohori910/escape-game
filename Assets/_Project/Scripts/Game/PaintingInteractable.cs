using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // デスクエリアの絵画：装飾。クリックすると短いポップアップを表示する
    [RequireComponent(typeof(Collider))]
    public class PaintingInteractable : MonoBehaviour
    {
        private void OnMouseDown()
        {
            if (RoomViewController.Instance.CurrentArea != RoomArea.Desk) return;
            PopupUI.Instance?.Show("古い肖像画だ。特に手がかりはなさそうだ。", 3f);
        }
    }
}
