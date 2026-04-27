using UnityEngine;
using UnityEngine.Events;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // パズル①：色付きの本を写真と同じ順番に並べ替える
    // 正解: 青(1)→赤(0)→緑(2)
    public class BookshelfPuzzle : MonoBehaviour
    {
        // 色インデックス: 0=赤, 1=青, 2=緑
        [SerializeField] private int[] correctOrder = { 1, 0, 2 };
        [SerializeField] private int[] currentOrder  = { 0, 1, 2 };

        public static readonly string[] ColorNames = { "赤", "青", "緑" };
        public static readonly Color[]  BookColors  =
        {
            new Color(0.80f, 0.20f, 0.20f), // 赤
            new Color(0.20f, 0.40f, 0.85f), // 青
            new Color(0.20f, 0.70f, 0.30f), // 緑
        };

        public UnityEvent OnSolved;

        private bool isSolved;
        private int selectedIndex = -1; // -1 = 未選択

        public bool IsSolved      => isSolved;
        public int  SelectedIndex => selectedIndex;
        public int[] GetCurrentOrder() => currentOrder;

        // BookInteractable から呼ぶ
        public void OnBookClicked(int bookIndex)
        {
            if (isSolved) return;

            if (selectedIndex < 0)
            {
                selectedIndex = bookIndex;
            }
            else if (selectedIndex == bookIndex)
            {
                selectedIndex = -1; // 同じ本 → 選択解除
            }
            else
            {
                (currentOrder[selectedIndex], currentOrder[bookIndex]) =
                    (currentOrder[bookIndex], currentOrder[selectedIndex]);
                selectedIndex = -1;
                AudioManager.Instance?.PlaySE("SE_BookMove");
                CheckSolution();
            }
        }

        private void CheckSolution()
        {
            if (isSolved) return;
            for (int i = 0; i < correctOrder.Length; i++)
                if (currentOrder[i] != correctOrder[i]) return;

            isSolved = true;
            FlagManager.Instance.SetFlag(Flags.BookshelfSolved);
            AudioManager.Instance?.PlaySE("SE_PuzzleSolve");
            OnSolved.Invoke();
            Debug.Log("[BookshelfPuzzle] 解決！受話器コードを入手");
        }
    }
}
