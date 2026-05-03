using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 本棚・机の2謎クリア後に鍵を付与する。扉は最初から表示済み。
    public class Room1ClearManager : MonoBehaviour
    {
        [SerializeField] private ItemData roomKeyItem;

        private bool given;

        private void Start()
        {
            FlagManager.Instance.OnFlagChanged.AddListener(OnFlagChanged);
            CheckClear();
        }

        private void OnDestroy()
        {
            FlagManager.Instance?.OnFlagChanged.RemoveListener(OnFlagChanged);
        }

        private void OnFlagChanged(string flagId, bool value)
        {
            if (flagId == Flags.BookshelfSolved || flagId == Flags.DeskSolved)
                CheckClear();
        }

        private void CheckClear()
        {
            if (!FlagManager.Instance.HasFlag(Flags.BookshelfSolved)) return;
            if (!FlagManager.Instance.HasFlag(Flags.DeskSolved)) return;
            GiveKey();
        }

        public void GiveKey()
        {
            if (given) return;
            given = true;

            FlagManager.Instance.SetFlag(Flags.Room1Cleared);
            InventoryManager.Instance.AddItem(roomKeyItem);
            AudioManager.Instance?.PlaySE("SE_PuzzleSolve");

            PopupUI.Instance?.Show("机の引き出しの奥から鍵が出てきた！\n奥の扉に使えそうだ……", 4f);
            Invoke(nameof(ReturnToOverview), 1.5f);
            Debug.Log("[Room1ClearManager] 本棚+机クリア！鍵を付与");
        }

        private void ReturnToOverview()
        {
            RoomViewController.Instance?.MoveToCurrentOverview();
        }
    }
}
