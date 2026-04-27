// BaseScene を自動構成するEditor拡張
// メニュー: EscapeGame > Setup > Create Base Scene
// ターミナルから: unity -batchmode -executeMethod SceneSetup.CreateBaseScene -quit
#if UNITY_EDITOR
using EscapeGame.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SceneSetup
{
    [MenuItem("EscapeGame/Setup/Create Base Scene")]
    public static void CreateBaseScene()
    {
        Debug.Log("[SceneSetup] BaseScene の構成を開始");

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ========== カメラ ==========
        var cameraGO = new GameObject("MainCamera");
        var cam = cameraGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        cameraGO.tag = "MainCamera";
        cameraGO.transform.position = new Vector3(0, 1, -10);

        // ========== ライト ==========
        var lightGO = new GameObject("DirectionalLight");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // ========== EventSystem ==========
        // Unity 2022以降はInputSystemUIInputModuleを推奨（旧InputSystem使用時はStandaloneのままでよい）
        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<StandaloneInputModule>();

        // ========== メインCanvas ==========
        var canvasGO = new GameObject("Canvas_Main");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1170, 2532); // iPhone 14 Pro基準
        canvasScaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<SafeAreaHandler>();

        // ========== インベントリバー（HorizontalLayoutGroupで均等配置）==========
        var inventoryBarGO = CreateStretchedPanel(canvasGO.transform, "InventoryBar",
            anchorMinY: 0f, anchorMaxY: 0f, height: 160f);
        inventoryBarGO.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        var layout = inventoryBarGO.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 16f;
        layout.padding = new RectOffset(24, 24, 15, 15);
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        for (int i = 0; i < 5; i++)
        {
            var slot = new GameObject($"ItemSlot_{i:00}");
            slot.transform.SetParent(inventoryBarGO.transform, false);
            var slotRect = slot.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(120f, 120f);
            slot.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);
            slot.AddComponent<Button>();
        }

        // ========== メニューボタン ==========
        var menuBtnGO = new GameObject("MenuButton");
        menuBtnGO.transform.SetParent(canvasGO.transform, false);
        var menuRect = menuBtnGO.AddComponent<RectTransform>();
        menuRect.anchorMin = menuRect.anchorMax = menuRect.pivot = new Vector2(1f, 1f);
        menuRect.anchoredPosition = new Vector2(-30f, -50f);
        menuRect.sizeDelta = new Vector2(120f, 60f);
        menuBtnGO.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.85f);
        menuBtnGO.AddComponent<Button>();
        AddTMPLabel(menuBtnGO.transform, "MENU", 28);

        // ========== メッセージウィンドウ ==========
        var msgWindowGO = CreateStretchedPanel(canvasGO.transform, "MessageWindow",
            anchorMinY: 0.55f, anchorMaxY: 0.55f, height: 220f);
        msgWindowGO.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.15f, 0.92f);
        msgWindowGO.SetActive(false);

        var msgTextGO = new GameObject("MessageText");
        msgTextGO.transform.SetParent(msgWindowGO.transform, false);
        var msgRect = msgTextGO.AddComponent<RectTransform>();
        msgRect.anchorMin = Vector2.zero;
        msgRect.anchorMax = Vector2.one;
        msgRect.offsetMin = new Vector2(20f, 20f);
        msgRect.offsetMax = new Vector2(-20f, -20f);
        var tmp = msgTextGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "メッセージがここに表示されます。";
        tmp.fontSize = 32;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft;

        // ========== シーンを保存 ==========
        const string scenePath = "Assets/_Project/Scenes/BaseScene.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        Debug.Log($"[SceneSetup] BaseScene を保存しました: {scenePath}");
    }

    // 横いっぱいに伸びるパネルを生成する（anchorX=0〜1固定、Y方向はanchorMinY/anchorMaxYで制御）
    private static GameObject CreateStretchedPanel(Transform parent, string name,
        float anchorMinY, float anchorMaxY, float height)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, anchorMinY);
        rect.anchorMax = new Vector2(1f, anchorMaxY);
        rect.offsetMin = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, height);
        go.AddComponent<Image>();
        return go;
    }

    // TextMeshProUGUI ラベルを子オブジェクトとして追加する
    private static void AddTMPLabel(Transform parent, string content, float fontSize)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }
}
#endif
