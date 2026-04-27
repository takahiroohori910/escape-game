using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // パズル解決イベントとインベントリ報酬をコードで配線する
    // UnityEventのインスペクタ配線が保存されない問題を回避
    public class PuzzleWirer : MonoBehaviour
    {
        [SerializeField] private ItemData phoneCordItem;
        [SerializeField] private ItemData circuitBoardItem;

        private void Awake()
        {
            var bookshelf = FindAnyObjectByType<BookshelfPuzzle>();
            if (bookshelf != null && phoneCordItem != null)
                bookshelf.OnSolved.AddListener(() =>
                {
                    InventoryManager.Instance.AddItem(phoneCordItem);
                    Debug.Log("[PuzzleWirer] 本棚クリア → 受話器コード入手");
                });

            var desk = FindAnyObjectByType<DeskPuzzle>();
            if (desk != null && circuitBoardItem != null)
                desk.OnSolved.AddListener(() =>
                {
                    InventoryManager.Instance.AddItem(circuitBoardItem);
                    Debug.Log("[PuzzleWirer] デスククリア → 内部基板入手");
                });
        }
    }
}
