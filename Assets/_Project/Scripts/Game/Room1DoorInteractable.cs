using System.Collections;
using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // Overview視点でクリックすると Room2 に移動する扉。
    // Flag.Room1Cleared が立っていれば、ヒンジを軸に開く演出 → カメラが通過する
    [RequireComponent(typeof(Collider))]
    public class Room1DoorInteractable : MonoBehaviour
    {
        [SerializeField] private float openDuration   = 0.9f;
        [SerializeField] private float openAngle      = -110f;  // 奥方向に押し開く（Y軸）
        [SerializeField] private float pauseAfterOpen = 0.35f;

        private bool isOpening;
        private bool isOpened;
        private Transform pivot;

        private void OnMouseDown()
        {
            if (!RoomViewController.Instance.IsOverview) return;
            if (isOpening || isOpened) return;

            if (!FlagManager.Instance.HasFlag(Flags.Room1Cleared))
            {
                PopupUI.Instance?.Show("扉は鍵がかかっている。");
                return;
            }

            StartCoroutine(OpenDoorAndWalkThrough());
        }

        private IEnumerator OpenDoorAndWalkThrough()
        {
            isOpening = true;
            SetupHinge();

            AudioManager.Instance?.PlaySE("SE_DoorOpen");

            float t = 0f;
            Quaternion start = pivot.rotation;
            Quaternion end   = start * Quaternion.Euler(0f, openAngle, 0f);
            while (t < openDuration)
            {
                // SmoothStep で加減速：開き始めゆっくり → 中盤速く → 終盤ゆっくり
                float u = Mathf.SmoothStep(0f, 1f, t / openDuration);
                pivot.rotation = Quaternion.Slerp(start, end, u);
                t += Time.deltaTime;
                yield return null;
            }
            pivot.rotation = end;
            isOpened = true;

            yield return new WaitForSeconds(pauseAfterOpen);

            AudioManager.Instance?.PlaySE("SE_CameraMove");
            RoomViewController.Instance.WalkThroughDoor();

            Debug.Log("[Room1DoorInteractable] 扉を開いて Room2 へ移動");
            isOpening = false;
        }

        // ヒンジ Pivot を「扉の前面（部屋側）の左端」に作って扉を子にする
        // 押し扉の現実のヒンジ位置（扉の厚みの前面側端）に揃えることで
        // 回転軸が自然に見え、開く動きが「スライド」ではなく「押し開き」として認識される
        private void SetupHinge()
        {
            if (pivot != null) return;

            float halfW = transform.lossyScale.x * 0.5f;
            float halfD = transform.lossyScale.z * 0.5f;
            // 左端へ -right * halfW、前面（カメラ側 = -forward）へ -forward * halfD
            Vector3 hingePos = transform.position
                               - transform.right   * halfW
                               - transform.forward * halfD;

            var pivotGo = new GameObject("DoorPivot");
            pivotGo.transform.position = hingePos;
            pivotGo.transform.rotation = transform.rotation;

            transform.SetParent(pivotGo.transform, true);
            pivot = pivotGo.transform;
        }
    }
}
