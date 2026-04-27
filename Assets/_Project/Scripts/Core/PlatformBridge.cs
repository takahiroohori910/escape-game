using UnityEngine;

namespace EscapeGame.Core
{
    // iOS / WebGL のプラットフォーム差異を吸収する抽象化レイヤー
    // セーブ・ロード・広告をここ経由で呼び出すことで、各システムがプラットフォームを意識しない構造にする
    public static class PlatformBridge
    {
        // ========== セーブ / ロード ==========

        // 文字列データを保存する
        // iOS=PlayerPrefs、WebGL=localStorage（Unity自動マッピング）、Editor=PlayerPrefs
        // TODO: iOS本番では暗号化レイヤーをここに追加する
        public static void SaveString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        // 文字列データを読み込む（存在しない場合は defaultValue を返す）
        public static string LoadString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        // セーブデータが存在するかを確認する
        public static bool HasSave(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        // 指定キーのセーブデータを削除する
        public static void DeleteSave(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        // ========== 広告（AdMob）スタブ ==========
        // 本番実装時はここにAdMob SDKの呼び出しを追加する

        // 広告SDKを初期化する（アプリ起動時に一度だけ呼ぶ）
        public static void InitializeAds()
        {
#if UNITY_IOS
            Debug.Log("[PlatformBridge] iOS: AdMob SDK 初期化（スタブ）");
            // TODO: MobileAds.Initialize() を実装する
#elif UNITY_WEBGL
            Debug.Log("[PlatformBridge] WebGL: 広告はスキップ");
#else
            Debug.Log("[PlatformBridge] Editor: 広告初期化スキップ");
#endif
        }

        // インタースティシャル広告を表示する（クリア時などに呼ぶ）
        public static void ShowInterstitialAd(System.Action onComplete = null)
        {
#if UNITY_IOS
            Debug.Log("[PlatformBridge] iOS: インタースティシャル広告表示（スタブ）");
            // TODO: 広告ロード・表示・完了コールバックを実装する
            onComplete?.Invoke();
#elif UNITY_WEBGL
            // WebGLでは広告をスキップしてコールバックのみ実行
            Debug.Log("[PlatformBridge] WebGL: 広告スキップ");
            onComplete?.Invoke();
#else
            Debug.Log("[PlatformBridge] Editor: 広告スキップ");
            onComplete?.Invoke();
#endif
        }

        // バナー広告を表示する
        public static void ShowBannerAd()
        {
#if UNITY_IOS
            Debug.Log("[PlatformBridge] iOS: バナー広告表示（スタブ）");
            // TODO: バナー広告の実装
#else
            Debug.Log("[PlatformBridge] WebGL/Editor: バナー広告スキップ");
#endif
        }

        // バナー広告を非表示にする
        public static void HideBannerAd()
        {
#if UNITY_IOS
            Debug.Log("[PlatformBridge] iOS: バナー広告非表示（スタブ）");
#else
            // 何もしない
#endif
        }
    }
}
