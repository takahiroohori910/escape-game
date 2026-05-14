using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // チェスト解錠フラグを監視して Room1Cleared を確定する。
    // 鍵は ChestPuzzle 側で付与（ItemDetailUI で表示）。ここでは進行表示は行わない。
    public class Room1ClearManager : MonoBehaviour
    {
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
            if (flagId == Flags.ChestSolved) CheckClear();
        }

        private void CheckClear()
        {
            if (!FlagManager.Instance.HasFlag(Flags.ChestSolved)) return;
            MarkCleared();
        }

        public void MarkCleared()
        {
            if (given) return;
            given = true;
            FlagManager.Instance.SetFlag(Flags.Room1Cleared);
            Debug.Log("[Room1ClearManager] チェスト解錠でクリア状態へ");
        }
    }
}
