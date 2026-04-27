using UnityEngine;
using TMPro;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // ゲーム中の経過時間を表示し、クリア時にタイムを記録する
    public class TimerUI : MonoBehaviour
    {
        public static TimerUI Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI clearTimeText;

        private float elapsed;
        private bool running;

        private void Awake()
        {
            Instance = this;
            if (timerText) timerText.text = "00:00";
        }

        private void Start()
        {
            GameManager.Instance.OnStateChanged.AddListener(OnStateChanged);
        }

        private void Update()
        {
            if (!running) return;
            elapsed += Time.deltaTime;
            if (timerText) timerText.text = Format(elapsed);
        }

        public void StartTimer()
        {
            elapsed = 0f;
            running = true;
        }

        private void OnStateChanged(EscapeGameState prev, EscapeGameState next)
        {
            if (next != EscapeGameState.Clear) return;
            running = false;
            SaveManager.Instance?.SaveBestTime(elapsed);

            if (clearTimeText)
            {
                float best = SaveManager.Instance?.GetBestTime() ?? elapsed;
                clearTimeText.text = $"クリアタイム  {Format(elapsed)}"
                    + (best < elapsed ? $"\nベスト  {Format(best)}" : "");
            }
        }

        private static string Format(float s)
        {
            int m = (int)(s / 60);
            int sec = (int)(s % 60);
            return $"{m:00}:{sec:00}";
        }
    }
}
