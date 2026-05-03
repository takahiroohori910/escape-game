using UnityEngine;
using UnityEngine.Events;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // Room2パズル④（最終）：祭壇の4桁コードを入力して脱出
    // 正解：肖像画の隠し引き出しに書かれた "7429"
    public class AltarPuzzle : MonoBehaviour
    {
        [SerializeField] private string correctCode = "7429";

        private string enteredCode = "";
        private bool isSolved;

        public UnityEvent OnSolved;
        public bool IsSolved => isSolved;

        public void InputDigit(string digit)
        {
            if (isSolved || enteredCode.Length >= 4) return;

            if (!FlagManager.Instance.HasFlag(Flags.PortraitSolved))
            {
                PopupUI.Instance?.Show("祭壇の錠前……何か手がかりが必要だ。");
                return;
            }

            enteredCode += digit;
            if (enteredCode.Length == 4) CheckSolution();
        }

        public void ClearInput() => enteredCode = "";
        public string GetEnteredCode() => enteredCode;

        private void CheckSolution()
        {
            if (enteredCode != correctCode)
            {
                enteredCode = "";
                AudioManager.Instance?.PlaySE("SE_PuzzleFail");
                return;
            }

            isSolved = true;
            FlagManager.Instance.SetFlag(Flags.AltarSolved);
            AudioManager.Instance?.PlaySE("SE_PuzzleSolve");
            OnSolved.Invoke();
            Debug.Log("[AltarPuzzle] 解決！ゲームクリア");

            GameManager.Instance.TriggerClear();
        }
    }
}
