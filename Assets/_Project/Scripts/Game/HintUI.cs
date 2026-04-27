using UnityEngine;
using TMPro;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 右上の「？」ボタン：現在エリアのヒントを順番に表示。押すたびに次のヒントへ。
    public class HintUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI hintText;

        private bool isVisible;
        private RoomArea lastArea = (RoomArea)(-1);
        private int hintIndex;

        // エリア別ヒント（複数）
        private static readonly string[][] AreaHints =
        {
            // Overview (0)
            new[]
            {
                "本棚・デスク・暖炉の近くをクリックすると移動できる。",
                "3つの謎を解いて電話を修理し、救助を呼ぼう。",
            },
            // Bookshelf (1)
            new[]
            {
                "色付きの本を正しい順番に並べよう。",
                "本を2冊クリックすると入れ替わる。",
                "どこかに正しい並び順を示すものがある。暖炉エリアを調べよう。",
            },
            // Desk (2)
            new[]
            {
                "引き出しの金庫には4桁の暗証番号が必要だ。",
                "部屋にある時計を調べてみよう。",
                "時刻を数字4桁に変換してみよう。",
            },
            // Fireplace (3)
            new[]
            {
                "暖炉エリアを調べよう。写真が手がかりになる。",
                "写真に何かが写っている。本棚パズルのヒントになるかもしれない。",
            },
        };

        private void Awake() => panel.SetActive(false);

        public void Toggle()
        {
            var area = RoomViewController.Instance?.CurrentArea ?? (RoomArea)0;

            if (!isVisible)
            {
                // 初回表示 or エリア変化後：最初のヒントから
                if (area != lastArea)
                {
                    lastArea = area;
                    hintIndex = 0;
                }
                isVisible = true;
                panel.SetActive(true);
                ShowHint(area);
            }
            else
            {
                // 表示中：次のヒントへ（最後まで来たら非表示）
                var hints = GetHints(area);
                hintIndex++;
                if (hintIndex >= hints.Length)
                {
                    hintIndex = 0;
                    isVisible = false;
                    panel.SetActive(false);
                }
                else
                {
                    ShowHint(area);
                }
            }
        }

        public void Hide()
        {
            isVisible = false;
            panel.SetActive(false);
        }

        private void Update()
        {
            if (!isVisible) return;
            var area = RoomViewController.Instance?.CurrentArea ?? (RoomArea)(-1);
            if (area != lastArea)
            {
                lastArea = area;
                hintIndex = 0;
                ShowHint(area);
            }
        }

        private void ShowHint(RoomArea area)
        {
            // 机エリアで時計未調査なら時計ヒントを先に出す
            if (area == RoomArea.Desk && hintIndex >= 1 &&
                !FlagManager.Instance.HasFlag(Flags.ClockInspected))
            {
                hintText.text = "まず時計の時刻を確認しよう。";
                return;
            }

            var hints = GetHints(area);
            hintText.text = hints[Mathf.Clamp(hintIndex, 0, hints.Length - 1)];
        }

        private static string[] GetHints(RoomArea area)
        {
            int idx = (int)area;
            return idx >= 0 && idx < AreaHints.Length ? AreaHints[idx] : new[] { "" };
        }
    }
}
