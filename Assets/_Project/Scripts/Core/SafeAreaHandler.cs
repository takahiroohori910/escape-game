using UnityEngine;

namespace EscapeGame.Core
{
    // iOSのノッチ・ホームインジケーターに対応するSafe Area調整コンポーネント
    // BuildAutomatorが自動でCanvasにアタッチする
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHandler : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            // 画面回転時にSafe Areaが変わるため、変化を検知して再適用する
            if (Screen.safeArea != lastSafeArea)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            lastSafeArea = Screen.safeArea;

            var screenSize = new Vector2(Screen.width, Screen.height);
            var anchorMin = lastSafeArea.position;
            var anchorMax = lastSafeArea.position + lastSafeArea.size;

            // 画面サイズで正規化してanchorに変換
            anchorMin.x /= screenSize.x;
            anchorMin.y /= screenSize.y;
            anchorMax.x /= screenSize.x;
            anchorMax.y /= screenSize.y;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}
