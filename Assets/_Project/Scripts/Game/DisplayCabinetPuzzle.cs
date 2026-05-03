using UnityEngine;
using UnityEngine.Events;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // Room2パズル①：ステンドグラスの図形カウントから4桁コードを導き食器棚を開ける
    // 正解：薔薇×2, 十字×3, 星×1, 菱形×4 → "2314"
    public class DisplayCabinetPuzzle : MonoBehaviour
    {
        [SerializeField] private string correctCode = "2314";
        [SerializeField] private NoteData cabinetHintNote; // 開錠後に読める燭台ヒント

        private string enteredCode = "";
        private bool isSolved;

        public UnityEvent OnSolved;
        public bool IsSolved => isSolved;

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
                return;
            }

            isSolved = true;
            FlagManager.Instance.SetFlag(Flags.DisplayCabinetSolved);
            AudioManager.Instance?.PlaySE("SE_PuzzleSolve");
            OnSolved.Invoke();

            if (cabinetHintNote != null)
                NoteUI.Instance?.Show(cabinetHintNote);

            Debug.Log("[DisplayCabinetPuzzle] 解決！燭台ヒントを表示");
        }
    }
}
