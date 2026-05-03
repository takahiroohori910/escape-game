using UnityEngine;
using UnityEngine.Events;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // Room2パズル②：5本の燭台を正しいパターンで点灯させる
    // 正解：1番・3番・5番（左から奇数）を点灯
    public class CandelabraPuzzle : MonoBehaviour
    {
        // true=点灯, false=消灯。正解は [true,false,true,false,true]
        private readonly bool[] state = { false, false, false, false, false };
        private readonly bool[] correct = { true, false, true, false, true };

        private bool isSolved;
        public bool IsSolved => isSolved;

        public UnityEvent OnSolved;

        // CandleInteractable から呼ぶ（0-indexed）
        public void ToggleCandle(int index)
        {
            if (isSolved || index < 0 || index >= state.Length) return;
            state[index] = !state[index];
            AudioManager.Instance?.PlaySE("SE_Click");
            CheckSolution();
        }

        public bool IsLit(int index) => index >= 0 && index < state.Length && state[index];

        private void CheckSolution()
        {
            for (int i = 0; i < correct.Length; i++)
                if (state[i] != correct[i]) return;

            isSolved = true;
            FlagManager.Instance.SetFlag(Flags.CandelabraSolved);
            AudioManager.Instance?.PlaySE("SE_PuzzleSolve");
            OnSolved.Invoke();
            PopupUI.Instance?.Show("燭台の炎が揺れ……肖像画に何かが起きた。");
            Debug.Log("[CandelabraPuzzle] 解決！肖像画が解放された");
        }
    }
}
