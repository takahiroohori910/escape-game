using System.Collections;
using UnityEngine;
using TMPro;

namespace EscapeGame.Game
{
    // 一時的なテキストポップアップ（絵画ヒント・アイテム詳細など）
    public class PopupUI : MonoBehaviour
    {
        public static PopupUI Instance { get; private set; }

        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI messageText;

        private Coroutine hideCoroutine;

        private void Awake()
        {
            Instance = this;
            panel.SetActive(false);
        }

        public void Show(string message, float duration = 3f)
        {
            messageText.text = message;
            panel.SetActive(true);
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HideAfter(duration));
        }

        public void Hide()
        {
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            panel.SetActive(false);
        }

        private IEnumerator HideAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            panel.SetActive(false);
        }
    }
}
