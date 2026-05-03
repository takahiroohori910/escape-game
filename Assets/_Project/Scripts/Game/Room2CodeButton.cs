using UnityEngine;
using UnityEngine.UI;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // Room2の数字入力ボタン。CurrentAreaに応じてDisplayCabinetUI/AltarUIに振り分ける。
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
            var area = RoomViewController.Instance?.CurrentArea;
            if (area == RoomArea.DisplayCabinet)
            {
                var ui = FindAnyObjectByType<DisplayCabinetUI>();
                if (isClear) ui?.OnClearPressed(); else ui?.OnDigitPressed(digit);
            }
            else if (area == RoomArea.Altar)
            {
                var ui = FindAnyObjectByType<AltarUI>();
                if (isClear) ui?.OnClearPressed(); else ui?.OnDigitPressed(digit);
            }
        }
    }
}
