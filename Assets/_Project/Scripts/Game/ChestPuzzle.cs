using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // Room1パズル③：チェストの3段引き出しを正しい5回の順番で開けて解錠。
    // 凡例（机金庫から入手）：月=上段(0)・雲=中段(1)・花=下段(2)
    // 順序（本棚から入手）：月→花→雲→月→花 → 翻訳して上→下→中→上→下
    // 解錠 = 部屋扉の解錠通知のみ（鍵アイテムは廃止、Flag.ChestSolved → Room1Cleared でフラグ判定）
    public class ChestPuzzle : MonoBehaviour
    {
        // 上=0 / 中=1 / 下=2
        [SerializeField] private int[] correctSequence = { 0, 2, 1, 0, 2 };

        private readonly List<int> input = new();
        private bool isSolved;
        private RoomArea lastArea = RoomArea.Overview;
        private ChestDrawerInteractable currentOpenDrawer;

        public UnityEvent OnSolved;
        public bool IsSolved => isSolved;
        public int InputCount => input.Count;
        public int RequiredCount => correctSequence != null ? correctSequence.Length : 0;

        private void Update()
        {
            if (isSolved) return;
            var cur = RoomViewController.Instance != null
                ? RoomViewController.Instance.CurrentArea
                : RoomArea.Overview;
            if (lastArea == RoomArea.Chest && cur != RoomArea.Chest && input.Count > 0)
            {
                ResetInput();
                Debug.Log("[ChestPuzzle] エリア離脱で入力リセット");
            }
            lastArea = cur;
        }

        public void OnDrawerOpened(int drawerIndex, ChestDrawerInteractable drawer)
        {
            if (isSolved) return;

            // 直前の引き出しを閉じてから新しい引き出しを開ける（同時に1つだけ開く）
            if (currentOpenDrawer != null && currentOpenDrawer != drawer)
                currentOpenDrawer.Close();
            drawer.Open();
            currentOpenDrawer = drawer;

            input.Add(drawerIndex);
            AudioManager.Instance?.PlaySE("SE_Click");

            if (input.Count >= correctSequence.Length) CheckSolution();
        }

        private void CheckSolution()
        {
            bool ok = true;
            for (int i = 0; i < correctSequence.Length; i++)
                if (input[i] != correctSequence[i]) { ok = false; break; }

            if (ok) Solve();
            else FailReset();
        }

        private void Solve()
        {
            isSolved = true;
            FlagManager.Instance.SetFlag(Flags.ChestSolved);
            AudioManager.Instance?.PlaySE("SE_PuzzleSolve");
            OnSolved?.Invoke();
            PopupUI.Instance?.Show("どこかで扉がカチッと音がした……", 3.5f);
            Debug.Log("[ChestPuzzle] 解錠！");
        }

        private void FailReset()
        {
            ResetInput();
            AudioManager.Instance?.PlaySE("SE_PuzzleFail");
            PopupUI.Instance?.Show("何かが違ったようだ……", 2.5f);
            Debug.Log("[ChestPuzzle] 不正解。入力リセット");
        }

        private void ResetInput()
        {
            input.Clear();
            if (currentOpenDrawer != null)
            {
                currentOpenDrawer.Close();
                currentOpenDrawer = null;
            }
        }
    }
}
