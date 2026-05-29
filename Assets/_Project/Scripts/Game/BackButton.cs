using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 戻るボタン：全体視点（Overview / Overview2）から個別エリアへ移動した時だけ表示
    // NoteOverlayなどのUI遮断を回避するため Input System で直接検知
    [RequireComponent(typeof(Button))]
    public class BackButton : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private Button button;
        private Image image;
        private TextMeshProUGUI label;
        private bool isVisible = true;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas != null) rootCanvas = rootCanvas.rootCanvas;
            button = GetComponent<Button>();
            image  = GetComponent<Image>();
            label  = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            button.onClick.AddListener(Execute);
            SetVisible(false); // 起動直後は Overview のため非表示
        }

        private void Update()
        {
            // Overview / Overview2 時は非表示
            var rvc = RoomViewController.Instance;
            if (rvc != null) SetVisible(!rvc.IsOverview);
            if (!isVisible) return;

            // NoteOverlayなど上位UIがレイキャストを遮断していても動作させる
            // Pointer は Mouse / Touchscreen / Pen の共通親。iOS Touch でも反応する
            if (Pointer.current == null) return;
            if (!Pointer.current.press.wasPressedThisFrame) return;
            if (!IsPointerOver()) return;
            Execute();
        }

        private void SetVisible(bool visible)
        {
            if (isVisible == visible) return;
            isVisible = visible;
            if (image  != null) image.enabled = visible;
            if (label  != null) label.enabled = visible;
            if (button != null) button.interactable = visible;
        }

        private void Execute()
        {
            NoteUI.Instance?.Hide();
            HintUI.Instance?.Hide();
            RoomViewController.Instance?.MoveToCurrentOverview();
        }

        private bool IsPointerOver()
        {
            if (rectTransform == null || rootCanvas == null) return false;
            var cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Pointer.current.position.ReadValue(), cam);
        }
    }
}
