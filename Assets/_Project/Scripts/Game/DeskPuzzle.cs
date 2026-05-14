using UnityEngine;
using UnityEngine.Events;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // パズル②：デスクの引き出しに4桁の暗証番号を入力して開ける
    // ヒント：部屋の壁掛け時計が 11:30 を指している → 1130
    public class DeskPuzzle : MonoBehaviour
    {
        [SerializeField] private string correctCode = "1130";
        [SerializeField] private ItemData hintItem; // チェスト解錠用ヒントメモ（暗号B：シンボル凡例）

        private string enteredCode = "";
        private bool isSolved;

        public UnityEvent OnSolved;

        public void InputDigit(string digit)
        {
            if (isSolved || enteredCode.Length >= 4) return;
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
                Debug.Log("[DeskPuzzle] 暗号が違います");
                return;
            }

            isSolved = true;
            FlagManager.Instance.SetFlag(Flags.DeskSolved);
            AudioManager.Instance?.PlaySE("SE_PuzzleSolve");
            OnSolved.Invoke();

            if (hintItem != null)
            {
                InventoryManager.Instance?.AddItem(hintItem);
                FindAnyObjectByType<ItemDetailUI>()?.Show(hintItem);
            }
            Debug.Log("[DeskPuzzle] 解錠！シンボル凡例を入手");
        }
    }
}
