using System.Collections;
using UnityEngine;
using TMPro;

namespace EscapeGame.Core
{
    // Play開始時にDynamic fontアトラスへ全文字を事前生成する
    public class FontWarmup : MonoBehaviour
    {
        // ゲーム中に使う全文字（ノート・ヒント・UI全テキスト網羅）
        private const string AllChars =
            // ひらがな全文字
            "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん" +
            "ぁぃぅぇぉっゃゅょ" +
            "がぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽ" +
            // カタカナ全文字
            "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲン" +
            "ァィゥェォッャュョ" +
            "ガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポヴ" +
            // 漢字（全ノート・ヒント・UI文字）
            "嵐洋館脱出成功見回消去続特探壁竣工繋閉決解" +
            "一二三四五六七八九十百千" +
            "以下上中並位何人仕代付作使入内出切前列刻印受古台号" +
            "味冊写冠剣叡儀奇契威守家序座建式引形必押掟揃描換" +
            "明星智炎灯点燭物王現理生真確示祖系約紙置菱薇薔謎" +
            "護走輪認説読金間電順食開隣封封壇変図庫章端管" +
            "部屋本棚机暖炉近移動画手掛数字暗証番号力調必要品揃救助呼" +
            "電話機内基板修理時計指青赤緑色付桁観察錠選択肖像" +
            "古年先生引鍵使様切端意味写真左走書壊受器" +
            "画掛指桁観察錠選択肖像中奇燭炎灯点" +
            "紋様個記録薔薇十字星菱形個錠鍵刻祭壇封印" +
            "三冊本棚正順番並替絵画掛数字暗証番号" +
            "器四図基壇変奇始字守察屋年座庫建引形" +
            "指掛揃換数暗書替板棚権正注消火炉" +
            "画番目真確示祖移章端管系約紙置肖" +
            "菱観計記証話認説調謎護走輪近部金録鍵" +
            "開間隠電順食（）：・！──「」、。";

        private IEnumerator Start()
        {
            yield return null;

            // 全TMP初回更新
            foreach (var tmp in FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include))
                tmp.ForceMeshUpdate();

            // Canvas内にプレウォームTMPを生成（TextMeshProUGUIはCanvas必須）
            var canvas = FindAnyObjectByType<Canvas>();
            var firstTmp = FindAnyObjectByType<TextMeshProUGUI>();
            if (canvas != null && firstTmp != null)
            {
                var go = new GameObject("_FontPrewarm");
                go.transform.SetParent(canvas.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(-9999f, -9999f); // 画面外
                rt.sizeDelta = new Vector2(1f, 1f);

                var t = go.AddComponent<TextMeshProUGUI>();
                t.font = firstTmp.font;
                t.fontSize = 1f;
                t.color = new Color(0f, 0f, 0f, 0f); // 完全透明
                t.text = AllChars;
                t.ForceMeshUpdate();

                yield return null;

                // アトラス更新後に全TMP再描画
                foreach (var tmp in FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include))
                    tmp.ForceMeshUpdate();

                Destroy(go);
            }
        }
    }
}
