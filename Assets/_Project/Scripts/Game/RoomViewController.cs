using UnityEngine;
using EscapeGame.Core;
using EscapeGame.Game;

namespace EscapeGame.Game
{
    public class RoomViewController : SingletonMonoBehaviour<RoomViewController>
    {
        [SerializeField] private Transform cameraTransform;

        [Header("Room1 カメラポイント")]
        [SerializeField] private Transform overviewPoint;
        [SerializeField] private Transform bookshelfPoint;
        [SerializeField] private Transform deskPoint;
        [SerializeField] private Transform fireplacePoint;

        [Header("Room2 カメラポイント")]
        [SerializeField] private Transform overview2Point;
        [SerializeField] private Transform stainedGlassPoint;
        [SerializeField] private Transform displayCabinetPoint;
        [SerializeField] private Transform candelabraPoint;
        [SerializeField] private Transform portraitPoint;
        [SerializeField] private Transform altarPoint;

        [SerializeField] private float moveDuration = 0.5f;

        public RoomArea CurrentArea { get; private set; } = RoomArea.Overview;

        public bool IsOverview =>
            CurrentArea == RoomArea.Overview || CurrentArea == RoomArea.Overview2;

        public bool IsRoom2Area =>
            CurrentArea == RoomArea.Overview2   ||
            CurrentArea == RoomArea.StainedGlass ||
            CurrentArea == RoomArea.DisplayCabinet ||
            CurrentArea == RoomArea.Candelabra   ||
            CurrentArea == RoomArea.Portrait     ||
            CurrentArea == RoomArea.Altar;

        private Coroutine moveCoroutine;

        public void MoveTo(RoomArea area)
        {
            if (CurrentArea == area) return;
            CurrentArea = area;

            var target = GetPoint(area);
            if (target == null) return;

            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(MoveCamera(target));
        }

        // 扉をくぐり抜けてRoom2へ移動する演出
        public void WalkThroughDoor()
        {
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            CurrentArea = RoomArea.Overview2;
            moveCoroutine = StartCoroutine(DoorTransitionRoutine());
        }

        private System.Collections.IEnumerator DoorTransitionRoutine()
        {
            var overview2 = GetPoint(RoomArea.Overview2);
            if (overview2 == null) yield break;

            // Stage 1: 扉に近づく
            yield return MoveCameraTo(new Vector3(0f, 1.7f, 4.8f), Quaternion.identity, 0.6f);
            // Stage 2: 扉をくぐり抜ける
            yield return MoveCameraTo(new Vector3(0f, 1.7f, 7.0f), Quaternion.identity, 0.8f);
            // Stage 3: Room2全体を見渡す位置へ
            yield return MoveCameraTo(overview2.position, overview2.rotation, 0.8f);
        }

        private System.Collections.IEnumerator MoveCameraTo(Vector3 targetPos, Quaternion targetRot, float duration)
        {
            var startPos = cameraTransform.position;
            var startRot = cameraTransform.rotation;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                cameraTransform.position = Vector3.Lerp(startPos, targetPos, t);
                cameraTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }
            cameraTransform.SetPositionAndRotation(targetPos, targetRot);
        }

        public void MoveToOverview() => MoveTo(RoomArea.Overview);

        // BackButton用：現在のルームに合わせた全体視点に戻る
        public void MoveToCurrentOverview() =>
            MoveTo(IsRoom2Area ? RoomArea.Overview2 : RoomArea.Overview);

        private Transform GetPoint(RoomArea area) => area switch
        {
            RoomArea.Overview        => overviewPoint,
            RoomArea.Bookshelf       => bookshelfPoint,
            RoomArea.Desk            => deskPoint,
            RoomArea.Fireplace       => fireplacePoint,
            RoomArea.Overview2       => overview2Point,
            RoomArea.StainedGlass    => stainedGlassPoint,
            RoomArea.DisplayCabinet  => displayCabinetPoint,
            RoomArea.Candelabra      => candelabraPoint,
            RoomArea.Portrait        => portraitPoint,
            RoomArea.Altar           => altarPoint,
            _                        => overviewPoint,
        };

        private System.Collections.IEnumerator MoveCamera(Transform target)
        {
            var startPos = cameraTransform.position;
            var startRot = cameraTransform.rotation;
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);
                cameraTransform.position = Vector3.Lerp(startPos, target.position, t);
                cameraTransform.rotation = Quaternion.Slerp(startRot, target.rotation, t);
                yield return null;
            }

            cameraTransform.SetPositionAndRotation(target.position, target.rotation);
        }
    }
}
