using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // MENUボタン：NoteOverlayなどのUI遮断を回避するためInput Systemで直接検知
    [RequireComponent(typeof(Button))]
    public class BackButton : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Canvas rootCanvas;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas != null) rootCanvas = rootCanvas.rootCanvas;
        }

        private void Start()
        {
            // 通常クリック（NoteOverlayなし時）の保険
            GetComponent<Button>().onClick.AddListener(Execute);
        }

        private void Update()
        {
            // NoteOverlayなど上位UIがレイキャストを遮断していても動作させる
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            if (!IsPointerOver()) return;
            Execute();
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
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Mouse.current.position.ReadValue(), cam);
        }
    }
}
