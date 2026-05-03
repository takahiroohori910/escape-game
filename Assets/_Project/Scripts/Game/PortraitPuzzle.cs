using UnityEngine;
using UnityEngine.Events;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // Room2パズル③：肖像画の4つの紋章を正しい順番でクリックする
    // 正解：指輪(0)→剣(1)→王冠(2)→書物(3)
    // 前提：CandelabraSolvedフラグが必要
    public class PortraitPuzzle : MonoBehaviour
    {
        private readonly int[] correctOrder = { 0, 1, 2, 3 };
        private readonly int[] inputOrder = new int[4];
        private int inputCount;

        private bool isSolved;
        public bool IsSolved => isSolved;

        [SerializeField] private NoteData portraitSecretNote; // "7429"が書かれたメモ

        public UnityEvent OnSolved;

        public static readonly string[] SymbolNames = { "指輪", "剣", "王冠", "書物" };

        // PortraitSymbolInteractable から呼ぶ（0-indexed）
        public void OnSymbolClicked(int symbolIndex)
        {
            if (isSolved) return;

            if (!FlagManager.Instance.HasFlag(Flags.CandelabraSolved))
            {
                PopupUI.Instance?.Show("肖像画は静かに掛かっているだけだ。\n何かが足りない気がする。");
                return;
            }

            inputOrder[inputCount] = symbolIndex;
            inputCount++;
            AudioManager.Instance?.PlaySE("SE_Click");

            if (inputCount < correctOrder.Length) return;

            // 正解チェック
            bool correct = true;
            for (int i = 0; i < correctOrder.Length; i++)
                if (inputOrder[i] != correctOrder[i]) { correct = false; break; }

            if (!correct)
            {
                inputCount = 0;
                AudioManager.Instance?.PlaySE("SE_PuzzleFail");
                PopupUI.Instance?.Show("何も起きなかった。順番が違うようだ。");
                return;
            }

            isSolved = true;
            FlagManager.Instance.SetFlag(Flags.PortraitSolved);
            AudioManager.Instance?.PlaySE("SE_PuzzleSolve");
            OnSolved.Invoke();

            if (portraitSecretNote != null)
                NoteUI.Instance?.Show(portraitSecretNote);

            Debug.Log("[PortraitPuzzle] 解決！祭壇コードを表示");
        }
    }
}
