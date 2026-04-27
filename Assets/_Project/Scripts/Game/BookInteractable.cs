using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 本をクリックして選択 → 別の本をクリックで入れ替え
    [RequireComponent(typeof(Collider))]
    public class BookInteractable : MonoBehaviour
    {
        [SerializeField] private int bookIndex; // 0=左, 1=中, 2=右

        private BookshelfPuzzle puzzle;
        private Renderer bookRenderer;
        private MaterialPropertyBlock mpb;

        private void Awake()
        {
            puzzle       = FindAnyObjectByType<BookshelfPuzzle>();
            bookRenderer = GetComponentInChildren<Renderer>();
            mpb          = new MaterialPropertyBlock();
        }

        private void Start() => ApplyColor();

        private void Update()
        {
            if (puzzle == null || bookRenderer == null || mpb == null) return;
            try { ApplyColor(); } catch { }
        }

        private void ApplyColor()
        {
            var order = puzzle.GetCurrentOrder();
            if (bookIndex >= order.Length) return;
            int colorIdx = order[bookIndex];
            if (colorIdx < 0 || colorIdx >= BookshelfPuzzle.BookColors.Length) return;
            Color base_   = BookshelfPuzzle.BookColors[colorIdx];
            bool selected = puzzle.SelectedIndex == bookIndex;
            mpb.SetColor("_BaseColor", selected ? Color.Lerp(base_, Color.white, 0.4f) : base_);
            bookRenderer.SetPropertyBlock(mpb);
        }

        private void OnMouseDown()
        {
            if (RoomViewController.Instance.CurrentArea != RoomArea.Bookshelf) return;
            AudioManager.Instance?.PlaySE("SE_Click");
            puzzle.OnBookClicked(bookIndex);
        }
    }
}
