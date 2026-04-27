using UnityEngine;

namespace EscapeGame.Game
{
    // 時計の文字盤テクスチャをプロシージャルに生成して Renderer に適用する
    [RequireComponent(typeof(Renderer))]
    public class ClockFace : MonoBehaviour
    {
        [SerializeField] private float hours   = 11f;
        [SerializeField] private float minutes = 30f;

        private const int S = 512;

        private void Start()
        {
            var tex = BuildTexture();
            var mat = new Material(GetComponent<Renderer>().sharedMaterial);
            // URP Lit は _BaseMap がメインテクスチャ
            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetColor("_EmissionColor", Color.black);
            mat.DisableKeyword("_EMISSION");
            GetComponent<Renderer>().material = mat;
        }

        private Texture2D BuildTexture()
        {
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
            var px  = new Color32[S * S];
            int cx = S / 2, cy = S / 2, r = S / 2 - 6;

            // 背景クリア
            for (int i = 0; i < px.Length; i++) px[i] = Color.clear;

            // 文字盤（クリーム色）
            FillCircle(px, cx, cy, r, new Color32(245, 238, 215, 255));
            // 外枠（ダークブラウン）
            for (int t = 0; t < 7; t++) RingCircle(px, cx, cy, r - t, new Color32(72, 44, 20, 255));

            // 時間マーカー（12本）
            for (int h = 0; h < 12; h++)
            {
                float a = h * 30f * Mathf.Deg2Rad;
                bool major = (h % 3 == 0);
                int mr = r - 14;
                int mx = cx - Mathf.RoundToInt(Mathf.Sin(a) * mr);
                int my = cy - Mathf.RoundToInt(Mathf.Cos(a) * mr);
                FillCircle(px, mx, my, major ? 5 : 3, new Color32(50, 32, 12, 255));
            }

            // 分針：端まで届く細い黒針
            float minDeg  = minutes / 60f * 360f;
            DrawHand(px, cx, cy, minDeg, (int)(r * 0.90f), 6, new Color32(0, 0, 0, 255));

            // 時針：短く太い濃赤針（分針と確実に区別）
            float hourDeg = (hours + minutes / 60f) / 12f * 360f;
            DrawHand(px, cx, cy, hourDeg, (int)(r * 0.26f), 24, new Color32(120, 20, 20, 255));

            // 中心ボルト
            FillCircle(px, cx, cy, 10, new Color32(80, 50, 20, 255));

            tex.SetPixels32(px);
            tex.Apply(false);
            return tex;
        }

        private static void FillCircle(Color32[] px, int cx, int cy, int radius, Color32 c)
        {
            int r2 = radius * radius;
            for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (dx * dx + dy * dy > r2) continue;
                int x = cx + dx, y = cy + dy;
                if (x >= 0 && x < S && y >= 0 && y < S) px[y * S + x] = c;
            }
        }

        private static void RingCircle(Color32[] px, int cx, int cy, int radius, Color32 c)
        {
            for (int deg = 0; deg < 720; deg++)
            {
                float rad = deg * Mathf.Deg2Rad * 0.5f;
                int x = cx + Mathf.RoundToInt(Mathf.Sin(rad) * radius);
                int y = cy + Mathf.RoundToInt(Mathf.Cos(rad) * radius);
                if (x >= 0 && x < S && y >= 0 && y < S) px[y * S + x] = c;
            }
        }

        private static void DrawHand(Color32[] px, int cx, int cy, float deg, int len, int w, Color32 c)
        {
            float rad = deg * Mathf.Deg2Rad;
            int ex = cx - Mathf.RoundToInt(Mathf.Sin(rad) * len);
            int ey = cy - Mathf.RoundToInt(Mathf.Cos(rad) * len);
            ThickLine(px, cx, cy, ex, ey, w, c);
        }

        private static void ThickLine(Color32[] px, int x0, int y0, int x1, int y1, int w, Color32 c)
        {
            int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx - dy, h = w / 2;
            while (true)
            {
                for (int ty = -h; ty <= h; ty++)
                for (int tx = -h; tx <= h; tx++)
                {
                    int nx = x0 + tx, ny = y0 + ty;
                    if (nx >= 0 && nx < S && ny >= 0 && ny < S) px[ny * S + nx] = c;
                }
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 <  dx) { err += dx; y0 += sy; }
            }
        }
    }
}
