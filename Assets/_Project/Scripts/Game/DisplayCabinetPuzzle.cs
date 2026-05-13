using UnityEngine;
using UnityEngine.Events;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // Room2パズル①：食器棚3段の食器の数を扉脇の3つのカウンタで一致させる
    // 上段の食器数(7) / 中段の食器数(4) / 下段の食器数(2) を各ボタンで入力 → 確定レバーで判定
    public class DisplayCabinetPuzzle : MonoBehaviour
    {
        [SerializeField] private int correctTop = 7;
        [SerializeField] private int correctMid = 4;
        [SerializeField] private int correctBot = 2;
        [SerializeField] private NoteData cabinetHintNote;

        private int topCount, midCount, botCount;
        private bool isSolved;

        public UnityEvent OnSolved;
        public UnityEvent OnCounterChanged;
        public bool IsSolved => isSolved;

        public int GetTop() => topCount;
        public int GetMid() => midCount;
        public int GetBot() => botCount;

        public void IncrementTop() => Increment(ref topCount);
        public void IncrementMid() => Increment(ref midCount);
        public void IncrementBot() => Increment(ref botCount);

        private void Increment(ref int counter)
        {
            if (isSolved) return;
            counter = (counter + 1) % 10;
            AudioManager.Instance?.PlaySE("SE_Click");
            OnCounterChanged?.Invoke();
        }

        public void ResetCounters()
        {
            if (isSolved) return;
            topCount = midCount = botCount = 0;
            AudioManager.Instance?.PlaySE("SE_Click");
            OnCounterChanged?.Invoke();
        }

        public void Submit()
        {
            if (isSolved) return;

            if (topCount == correctTop && midCount == correctMid && botCount == correctBot)
            {
                isSolved = true;
                FlagManager.Instance.SetFlag(Flags.DisplayCabinetSolved);
                AudioManager.Instance?.PlaySE("SE_PuzzleSolve");
                OnSolved?.Invoke();

                if (cabinetHintNote != null)
                    NoteUI.Instance?.Show(cabinetHintNote);

                Debug.Log("[DisplayCabinetPuzzle] 解決！燭台ヒントを表示");
            }
            else
            {
                topCount = midCount = botCount = 0;
                AudioManager.Instance?.PlaySE("SE_PuzzleFail");
                OnCounterChanged?.Invoke();
                Debug.Log($"[DisplayCabinetPuzzle] 不正解 (入力: {topCount}-{midCount}-{botCount})");
            }
        }
    }
}
