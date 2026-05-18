using UnityEngine;
using TMPro;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 右上の「？」ボタン：現在エリアのヒントを順番に表示。押すたびに次のヒントへ。
    public class HintUI : SingletonMonoBehaviour<HintUI>
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI hintText;

        private bool isVisible;
        private RoomArea lastArea = (RoomArea)(-1);
        private int hintIndex;

        // エリア別ヒント（RoomArea enumの順番に対応）
        private static readonly string[][] AreaHints =
        {
            // Overview (0)
            new[]
            {
                "本棚・デスク・チェストの近くをクリックすると移動できる。壁の風景画も調べられる。",
                "3つの謎を解くと扉の鍵が外れる。",
            },
            // Bookshelf (1)
            new[]
            {
                "色付きの本を正しい順番に並べよう。",
                "本を2冊クリックすると入れ替わる。",
                "壁に掛かった風景画を調べると、色の並び順がわかる。",
            },
            // Desk (2)
            new[]
            {
                "引き出しの金庫には4桁の暗証番号が必要だ。",
                "部屋にある時計を調べてみよう。",
                "時刻を数字4桁に変換してみよう。",
            },
            // Fireplace (3) — 撤去済み（enum 互換のため空要素を残す）
            new[] { "" },
            // Chest (4)
            new[]
            {
                "引き出しは上・中・下の3段。正しい順番で5回開けると鍵が外れる。",
                "2つのメモを組み合わせると、引き出しを開ける順番がわかる。",
                "シンボルの並びを引き出しの位置（上・中・下）に変換しよう。",
            },
            // Overview2 (5)
            new[]
            {
                "ステンドグラス・食器棚・燭台・肖像画・祭壇を調べよう。",
                "まずはステンドグラスをよく観察することから始めよう。",
            },
            // StainedGlass (6)
            new[]
            {
                "ステンドグラスに描かれた紋様の数を数えよう。",
                "説明板に紋様の順番が書いてある。その順に個数を並べると数字になる。",
                "数えた数字を食器棚の錠前に入力してみよう。",
            },
            // DisplayCabinet (7)
            new[]
            {
                "食器棚には4桁の錠前がついている。",
                "ステンドグラスの紋様の個数が鍵になる。",
                "錠前を開けると中に何か手がかりがある。",
            },
            // Candelabra (8)
            new[]
            {
                "燭台の炎をクリックして点灯・消灯できる。",
                "食器棚の中の紙に正しいパターンのヒントがある。",
                "奇数番目の燭台に注目しよう。",
            },
            // Portrait (8)
            new[]
            {
                "肖像画には4つの紋章が描かれている。",
                "燭台パズルを解くと肖像画の仕掛けが作動する。",
                "本棚の隠し本に紋章を押す順番のヒントがある。",
            },
            // Altar (9)
            new[]
            {
                "祭壇の錠前には4桁のコードが必要だ。",
                "肖像画の仕掛けを解くと数字が現れる。",
            },
        };

        protected override void Awake()
        {
            base.Awake();
            if (panel != null) panel.SetActive(false);
        }

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
                // エリア変更（MENU押下など）でヒントを自動的に閉じる
                isVisible = false;
                panel?.SetActive(false);
                lastArea = area;
                hintIndex = 0;
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
