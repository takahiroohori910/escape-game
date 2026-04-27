using UnityEngine;
using UnityEngine.UI;

namespace EscapeGame.Game
{
    [RequireComponent(typeof(Button))]
    public class DigitButton : MonoBehaviour
    {
        [SerializeField] private string digit;
        [SerializeField] private bool isClear;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(HandleClick);
        }

        public void HandleClick()
        {
            EscapeGame.Core.AudioManager.Instance?.PlaySE("SE_Click");
            var ui = FindAnyObjectByType<NumberPadUI>();
            if (isClear) ui?.OnClearPressed();
            else ui?.OnDigitPressed(digit);
        }
    }
}
