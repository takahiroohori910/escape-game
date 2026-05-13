using UnityEngine;
using UnityEngine.UI;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // Room2 の数字入力ボタン（祭壇用）。CurrentArea が Altar の時のみ反応。
    // 食器棚はカウンタ式に変更されたため、ここからは振り分けない。
    [RequireComponent(typeof(Button))]
    public class Room2CodeButton : MonoBehaviour
    {
        [SerializeField] private string digit;
        [SerializeField] private bool isClear;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            AudioManager.Instance?.PlaySE("SE_Click");
            if (RoomViewController.Instance?.CurrentArea != RoomArea.Altar) return;

            var ui = FindAnyObjectByType<AltarUI>();
            if (isClear) ui?.OnClearPressed(); else ui?.OnDigitPressed(digit);
        }
    }
}
