using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // Overview視点でクリックするとRoom2に移動する扉
    // インベントリに部屋の鍵がある場合のみ通過できる
    [RequireComponent(typeof(Collider))]
    public class Room1DoorInteractable : MonoBehaviour
    {
        private void OnMouseDown()
        {
            if (!RoomViewController.Instance.IsOverview) return;

            // foreach中にUseItem(コレクション変更)しないよう先に参照を取得
            ItemData keyItem = null;
            foreach (var item in InventoryManager.Instance.GetItems())
                if (item.ItemId == ItemIds.RoomKey) { keyItem = item; break; }

            if (keyItem == null)
            {
                PopupUI.Instance?.Show("扉は鍵がかかっている。");
                return;
            }

            InventoryManager.Instance.UseItem(keyItem);
            AudioManager.Instance?.PlaySE("SE_CameraMove");
            var mr = GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;
            RoomViewController.Instance.WalkThroughDoor();
            Debug.Log("[Room1DoorInteractable] Room2へ移動 (WalkThrough)");
        }
    }
}
