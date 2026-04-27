using System.Collections;
using UnityEngine;
using TMPro;

namespace EscapeGame.Core
{
    // Play開始時にDynamic fontアトラスへ全漢字を事前生成する
    public class FontWarmup : MonoBehaviour
    {
        // ゲーム中に使う全文字をここに列挙（ひらがな・カタカナは全文字）
        private const string AllChars =
            "嵐洋館脱出成功あなたはから" +
            "ゲーム開始続きデータ消去バージョン" +
            "部屋見回本棚机暖炉近くクリックすると移動できる" +
            "位置変正しい順番並替絵画手掛数字暗証番号入力" +
            "電話機調必要品揃救助呼" +
            "古びたメモ館建年先生引出鍵使様おっしゃっていた" +
            "切端三冊意味写真左四走書壊受話器内部基板修理" +
            "タイムベスト閉調解決繋掛" +
            "竣工年号鍵！──「」、。" +
            "壁隠探" +
            "時計指青赤緑色付桁観察錠選択入肖像特中" +
            "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん" +
            "ぁぃぅぇぉっゃゅょ" +
            "がぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽ" +
            "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲン" +
            "ァィゥェォッャュョ" +
            "ガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポヴ";

        private IEnumerator Start()
        {
            // 1フレーム待って全TMPをまず更新
            yield return null;
            foreach (var tmp in FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include))
                tmp.ForceMeshUpdate();

            // 事前生成用の非表示テキストで残り文字を強制生成
            var firstTmp = FindAnyObjectByType<TextMeshProUGUI>();
            if (firstTmp != null)
            {
                var go = new GameObject("_FontPrewarm");
                go.transform.SetParent(transform);
                var t = go.AddComponent<TextMeshProUGUI>();
                t.font = firstTmp.font;
                t.fontSize = 32;
                t.color = new Color(0, 0, 0, 0); // 透明
                t.text = AllChars;
                t.ForceMeshUpdate();
                yield return null;
                // 全TMP再更新（アトラス更新後）
                foreach (var tmp in FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include))
                    tmp.ForceMeshUpdate();
                Destroy(go);
            }
        }
    }
}
