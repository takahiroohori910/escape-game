using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // パズル解決イベントのログ出力（アイテム付与は Room1ClearManager が担当）
    // phoneCordItem / circuitBoardItem は電話修理がクリティカルパスだった頃の名残。削除可。
    public class PuzzleWirer : MonoBehaviour
    {
        [SerializeField] private ItemData phoneCordItem;
        [SerializeField] private ItemData circuitBoardItem;
    }
}
