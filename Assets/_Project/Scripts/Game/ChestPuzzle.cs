using UnityEngine;
using UnityEngine.Events;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // Room1パズル③：チェストの天板にあるシンボルダイヤル4個を正しい順に合わせる
    // ヒント源：本棚解錠で「シンボル順序メモ」、机金庫解錠で「シンボル凡例メモ」を入手
    // 解錠 → 部屋鍵を入手 → 扉へ
    public class ChestPuzzle : MonoBehaviour
    {
        // 0=月, 1=星, 2=鍵, 3=花, 4=剣 を想定。正解は 月→花→星→鍵
        [SerializeField] private int[] correctOrder = { 0, 3, 1, 2 };
        [SerializeField] private ChestSymbolDial[] dials;
        [SerializeField] private ItemData roomKeyItem;

        public int SymbolCount = 5;
        public UnityEvent OnSolved;

        private bool isSolved;
        public bool IsSolved => isSolved;
        public int[] CorrectOrder => correctOrder;

        public void OnDialChanged()
        {
            if (isSolved) return;
            if (IsAllCorrect()) Solve();
        }

        private bool IsAllCorrect()
        {
            if (dials == null || dials.Length < correctOrder.Length) return false;
            for (int i = 0; i < correctOrder.Length; i++)
                if (dials[i].CurrentSymbol != correctOrder[i]) return false;
            return true;
        }

        private void Solve()
        {
            isSolved = true;
            FlagManager.Instance.SetFlag(Flags.ChestSolved);
            AudioManager.Instance?.PlaySE("SE_PuzzleSolve");
            OnSolved?.Invoke();

            if (roomKeyItem != null)
            {
                InventoryManager.Instance?.AddItem(roomKeyItem);
                FindAnyObjectByType<ItemDetailUI>()?.Show(roomKeyItem);
            }
            Debug.Log("[ChestPuzzle] 解錠！部屋鍵を入手");
        }
    }
}
