using UnityEngine;
using UnityEngine.Events;
using EscapeGame.Core;
using EscapeGame.Game;

namespace EscapeGame.Game
{
    // パズル③：電話の部品①②をインベントリから使って修理し、救助を呼ぶ
    public class PhoneRepairPuzzle : MonoBehaviour
    {
        public UnityEvent OnRepaired; // 修理完了時
        public UnityEvent OnGameClear; // 脱出成功時（修理→通話→クリア）

        private bool isRepaired;

        // 暖炉エリアの電話をタップしたときに呼ぶ
        public void TryRepair()
        {
            if (isRepaired) return;

            var inventory = InventoryManager.Instance;
            bool hasCord         = HasItem(ItemIds.PhoneCord);
            bool hasCircuitBoard = HasItem(ItemIds.CircuitBoard);

            if (!hasCord || !hasCircuitBoard)
            {
                string missing = !hasCord ? "受話器コード" : "内部基板";
                AudioManager.Instance?.PlaySE("SE_PuzzleFail");
                PopupUI.Instance?.Show($"電話を修理するには {missing} が必要です");
                Debug.Log($"[PhoneRepairPuzzle] {missing}がありません");
                return;
            }

            // 部品を消費して修理完了
            UseItem(ItemIds.PhoneCord);
            UseItem(ItemIds.CircuitBoard);

            isRepaired = true;
            FlagManager.Instance.SetFlag(Flags.PhoneRepaired);
            OnRepaired.Invoke();
            Debug.Log("[PhoneRepairPuzzle] 電話を修理しました");
        }

        // 修理済みの電話をタップして救助を呼ぶ
        public void CallForRescue()
        {
            if (!isRepaired) return;
            OnGameClear.Invoke();
            GameManager.Instance.TriggerClear();
        }

        private bool HasItem(string itemId)
        {
            foreach (var item in InventoryManager.Instance.GetItems())
            {
                if (item.ItemId == itemId) return true;
            }
            return false;
        }

        private void UseItem(string itemId)
        {
            foreach (var item in InventoryManager.Instance.GetItems())
            {
                if (item.ItemId == itemId)
                {
                    InventoryManager.Instance.UseItem(item);
                    return;
                }
            }
        }
    }
}
