using UnityEngine;

namespace EscapeGame.Game
{
    // クリック可能なオブジェクトにアタッチしてホバー時に色ハイライト
    [RequireComponent(typeof(Collider))]
    public class HoverHighlight : MonoBehaviour
    {
        [SerializeField] private Color highlightColor = new Color(1f, 0.95f, 0.6f);

        private Renderer rend;
        private Color originalColor;
        private bool hasRenderer;

        private void Awake()
        {
            rend = GetComponent<Renderer>();
            hasRenderer = rend != null && rend.material != null;
            if (hasRenderer) originalColor = rend.material.color;
        }

        private void OnMouseEnter()
        {
            if (TitleUI.Instance != null && TitleUI.Instance.IsShowing) return;
            if (hasRenderer) rend.material.color = highlightColor;
        }

        private void OnMouseExit()
        {
            if (hasRenderer) rend.material.color = originalColor;
        }
    }
}
