using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // チェストの引き出し（上/中/下）。クリックで「開ける」演出 + ChestPuzzle に通知。
    // 同時に開くのは常に1つ（直前の引き出しは ChestPuzzle 側で Close() される）。
    public class ChestDrawerInteractable : MonoBehaviour
    {
        [SerializeField] private int drawerIndex; // 0=上 / 1=中 / 2=下
        [SerializeField] private ChestPuzzle puzzle;
        [SerializeField] private float openOffset   = 0.3f;  // 引き出しを引く距離（world Z 軸）
        [SerializeField] private float animDuration = 0.25f; // スライド時間

        private Vector3 closedWorldPos;
        private bool   posInitialized;
        private Coroutine animCoroutine;

        private void Awake()
        {
            closedWorldPos = transform.position;
            posInitialized = true;
        }

        private void Update()
        {
            if (RoomViewController.Instance == null) return;
            if (RoomViewController.Instance.CurrentArea != RoomArea.Chest) return;
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

            var cam = Camera.main;
            if (cam == null) return;

            var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit)) return;
            if (hit.transform != transform) return;

            puzzle?.OnDrawerOpened(drawerIndex, this);
        }

        public void Open()
        {
            if (!posInitialized) { closedWorldPos = transform.position; posInitialized = true; }
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            // 家具 Y=0 配置の正面方向 = world -Z（部屋手前）。そちらへ引き出す
            animCoroutine = StartCoroutine(SlideTo(closedWorldPos + Vector3.back * openOffset));
        }

        public void Close()
        {
            if (!posInitialized) return;
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(SlideTo(closedWorldPos));
        }

        private IEnumerator SlideTo(Vector3 target)
        {
            Vector3 start = transform.position;
            float t = 0f;
            while (t < animDuration)
            {
                transform.position = Vector3.Lerp(start, target, t / animDuration);
                t += Time.deltaTime;
                yield return null;
            }
            transform.position = target;
        }
    }
}
