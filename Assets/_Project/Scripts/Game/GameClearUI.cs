using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // ゲームクリア時のフルスクリーンオーバーレイ
    public class GameClearUI : MonoBehaviour
    {
        [SerializeField] private GameObject overlay;
        [SerializeField] private TextMeshProUGUI clearText;
        [SerializeField] private TextMeshProUGUI subText;
        [SerializeField] private Button backToTitleButton;

        private void Awake()
        {
            overlay.SetActive(false);
            GameManager.Instance.OnStateChanged.AddListener(OnStateChanged);
            if (backToTitleButton)
                backToTitleButton.onClick.AddListener(OnBackToTitle);
        }

        private void OnBackToTitle()
        {
            overlay.SetActive(false);
            TitleUI.Instance?.ShowFromClear();
        }

        private void Start()
        {
            // Dynamic fontのグリフを事前生成するためフレーム一瞬表示してすぐ戻す
            if (subText != null)
            {
                subText.gameObject.SetActive(true);
                subText.ForceMeshUpdate();
            }
        }

        private void OnStateChanged(EscapeGameState prev, EscapeGameState next)
        {
            if (next != EscapeGameState.Clear) return;
            overlay.SetActive(true);
            if (clearText != null) clearText.text = "脱出成功！";
            if (subText  != null) subText.text  = "あなたは嵐の洋館から脱出した！";
        }
    }
}
