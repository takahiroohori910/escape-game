using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // ゲーム開始前のタイトル画面オーバーレイ
    public class TitleUI : MonoBehaviour
    {
        public static TitleUI Instance { get; private set; }

        [SerializeField] private GameObject overlay;
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button clearSaveButton;
        [SerializeField] private TextMeshProUGUI versionText;

        public bool IsShowing => overlay.activeSelf;

        private void Awake()
        {
            Instance = this;
            overlay.SetActive(true);

            startButton.onClick.AddListener(OnStart);
            if (continueButton)  continueButton.onClick.AddListener(OnContinue);
            if (clearSaveButton) clearSaveButton.onClick.AddListener(OnClearSave);
        }

        private void Start()
        {
            bool hasSave = SaveManager.HasSave;
            if (continueButton)  continueButton.gameObject.SetActive(hasSave);
            if (clearSaveButton) clearSaveButton.gameObject.SetActive(hasSave);
            if (versionText) versionText.text = "ver 0.2";
        }

        private void OnStart()
        {
            SaveManager.Instance?.ClearSave();
            Dismiss();
        }

        private void OnContinue()
        {
            SaveManager.Instance?.Load();
            RoomViewController.Instance?.MoveTo(RoomArea.Overview);
            Dismiss();
        }

        private void OnClearSave()
        {
            SaveManager.Instance?.ClearSave();
            if (continueButton)  continueButton.gameObject.SetActive(false);
            if (clearSaveButton) clearSaveButton.gameObject.SetActive(false);
        }

        // クリア画面からタイトルへ戻る
        public void ShowFromClear()
        {
            SaveManager.Instance?.ClearSave();
            overlay.SetActive(true);
            bool hasSave = SaveManager.HasSave;
            if (continueButton)  continueButton.gameObject.SetActive(hasSave);
            if (clearSaveButton) clearSaveButton.gameObject.SetActive(hasSave);
        }

        private void Dismiss()
        {
            overlay.SetActive(false);
            TimerUI.Instance?.StartTimer();
            AudioManager.Instance?.StartGameBGM();
        }
    }
}
