using UnityEngine;
using TMPro;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 本棚エリアで選択状態のフィードバックを表示する
    public class BookshelfStatusUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI instructionText;

        private BookshelfPuzzle puzzle;

        private void Awake()
        {
            puzzle = FindAnyObjectByType<BookshelfPuzzle>();
            panel.SetActive(false);
        }

        private void Update()
        {
            bool inShelf = RoomViewController.Instance.CurrentArea == RoomArea.Bookshelf;
            if (!inShelf || puzzle.IsSolved) { panel.SetActive(false); return; }

            int sel = puzzle.SelectedIndex;
            bool show = sel >= 0;
            if (panel.activeSelf != show) panel.SetActive(show);
            if (show)
                statusText.text = $"「{BookshelfPuzzle.ColorNames[puzzle.GetCurrentOrder()[sel]]}」の本を選択中…";
        }
    }
}
