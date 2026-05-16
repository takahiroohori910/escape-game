using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // Overview視点でクリックするとRoom2に移動する扉
    // Flag.Room1Cleared が立っていれば通過できる（鍵アイテム不要、ChestPuzzle 解錠時に立つ）
    [RequireComponent(typeof(Collider))]
    public class Room1DoorInteractable : MonoBehaviour
    {
        private void OnMouseDown()
        {
            if (!RoomViewController.Instance.IsOverview) return;

            if (!FlagManager.Instance.HasFlag(Flags.Room1Cleared))
            {
                PopupUI.Instance?.Show("扉は鍵がかかっている。");
                return;
            }

            AudioManager.Instance?.PlaySE("SE_CameraMove");
            var mr = GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;
            RoomViewController.Instance.WalkThroughDoor();
            Debug.Log("[Room1DoorInteractable] Room2へ移動 (WalkThrough)");
        }
    }
}
