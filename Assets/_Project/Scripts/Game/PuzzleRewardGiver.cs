using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // パズル解決時に指定アイテムをインベントリへ追加する
    public class PuzzleRewardGiver : MonoBehaviour
    {
        [SerializeField] private ItemData rewardItem;

        public void GiveReward()
        {
            if (rewardItem == null) return;
            InventoryManager.Instance.AddItem(rewardItem);
        }
    }
}
