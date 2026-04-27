using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 机エリアの電話をクリック/タップしたときの処理
    [RequireComponent(typeof(Collider))]
    public class TelephoneInteractable : MonoBehaviour
    {
        private PhoneRepairPuzzle puzzle;

        private void Awake() => puzzle = GetComponent<PhoneRepairPuzzle>();

        private void OnMouseDown()
        {
            if (RoomViewController.Instance.CurrentArea != RoomArea.Desk) return;

            if (FlagManager.Instance.HasFlag(Flags.PhoneRepaired))
                puzzle.CallForRescue();
            else
                puzzle.TryRepair();
        }
    }
}
