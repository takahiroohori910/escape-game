using UnityEngine;
using EscapeGame.Core;
using EscapeGame.Game;

namespace EscapeGame.Game
{
    // 書斎の3エリア（本棚・デスク・暖炉）へのカメラ切り替えを管理する
    public class RoomViewController : SingletonMonoBehaviour<RoomViewController>
    {
        [SerializeField] private Transform cameraTransform;

        [Header("各エリアのカメラ位置・向き")]
        [SerializeField] private Transform overviewPoint;
        [SerializeField] private Transform bookshelfPoint;
        [SerializeField] private Transform deskPoint;
        [SerializeField] private Transform fireplacePoint;

        [SerializeField] private float moveDuration = 0.5f;

        public RoomArea CurrentArea { get; private set; } = RoomArea.Overview;

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

        // 戻るボタン用：Overview視点に戻る
        public void MoveToOverview() => MoveTo(RoomArea.Overview);

        private Transform GetPoint(RoomArea area) => area switch
        {
            RoomArea.Overview   => overviewPoint,
            RoomArea.Bookshelf  => bookshelfPoint,
            RoomArea.Desk       => deskPoint,
            RoomArea.Fireplace  => fireplacePoint,
            _                   => overviewPoint,
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
