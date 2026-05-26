namespace EscapeGame.Core
{
    // シーン内 GameObject 名の単一情報源。
    // GameObject.Find / transform.Find / name 比較などで文字列を直書きせず、ここを参照する。
    // 値を変更するとリネームと連動する。
    public static class SceneNames
    {
        // ===== 共通 / Managers / Canvas =====
        public const string CanvasMain   = "メインキャンバス";
        public const string Managers     = "マネージャー";
        public const string Prefabs      = "_プレハブ群";

        // ===== UI Panel / Overlay =====
        public const string HintButton           = "ヒントボタン";
        public const string HintPanel            = "ヒントパネル";
        public const string MenuButton           = "戻るボタン";
        public const string TitleOverlay         = "タイトルオーバーレイ";
        public const string ClearOverlay         = "クリアオーバーレイ";
        public const string TimerText            = "タイマーテキスト";
        public const string SubText              = "サブテキスト";
        public const string PopupPanel           = "ポップアップパネル";
        public const string NoteOverlay          = "ノートオーバーレイ";
        public const string ItemDetailPanel      = "アイテム詳細パネル";
        public const string BookshelfStatusPanel = "本棚ステータスパネル";
        public const string InventoryBar         = "インベントリバー";
        public const string NumberPadPanel       = "テンキーパネル";
        public const string DisplayCabinetPanel  = "食器棚操作パネル";

        // ===== Room1 主要オブジェクト =====
        public const string Bookshelf      = "本棚";
        public const string DeskTop        = "机";
        public const string Chest          = "チェスト";
        public const string DeskSafe       = "机の金庫";
        public const string FurnitureCandidates = "家具候補群";
        public const string FireplacePhoto = "暖炉跡の写真";
        public const string FireplacePointLight = "暖炉ライト";
        public const string FireplaceOpening    = "暖炉開口部";
        public const string FireEmber           = "火の燃え種";
        public const string Clock          = "時計";
        public const string Painting       = "絵画";
        public const string Telephone        = "電話";
        public const string TelephoneHandset = "電話受話器";
        public const string NoteOnDesk       = "机のメモ";
        public const string NoteOnBookshelf  = "本棚のメモ";
        public const string NoteOnFireplace  = "暖炉のメモ";
        public const string BackWall         = "奥壁";

        // ----- Bookshelf 内部 -----
        public const string S2_01     = "本棚棚2_書架01";
        public const string BS_Left   = "本棚_左板";
        public const string BS_Right  = "本棚_右板";
        public const string BS_Bottom = "本棚_底板";
        public const string BS_Shelf1 = "本棚_棚板1";
        public const string BS_Shelf2 = "本棚_棚板2";

        // ----- Chest 内部 -----
        public const string ChestPoint = "チェストカメラ位置";

        // ===== Room2 主要オブジェクト =====
        public const string Room2                  = "部屋2";
        public const string R2_StainedGlassRoot    = "部屋2_ステンドグラス群";
        public const string R2_DisplayCabinetRoot  = "部屋2_食器棚群";
        public const string R2_CandelabraRoot      = "部屋2_燭台群";
        public const string R2_AltarRoot           = "部屋2_祭壇群";

        public const string R2_ClickZone_StainedGlass = "部屋2_クリック領域_ステンドグラス";
        public const string R2_ClickZone_Cabinet      = "部屋2_クリック領域_食器棚";
        public const string R2_ClickZone_Candelabra   = "部屋2_クリック領域_燭台";
        public const string R2_ClickZone_Altar        = "部屋2_クリック領域_祭壇";

        public const string CandelabraPuzzle = "燭台パズル";
        public const string Altar_Lock       = "祭壇の錠前";

        // ----- Candelabra 動的命名テンプレ -----
        // 使い方: string.Format(SceneNames.CandleStickFormat, i)
        // 動的生成オブジェクトはリネーム未対応のためテンプレ値も従来のまま。
        public const string CandleStickFormat = "Candle_{0}_Stick";

        // ===== Camera Anchors / Points =====
        public const string CameraAnchors       = "カメラアンカー群";
        public const string Overview2Point      = "部屋2全景カメラ位置";
        public const string StainedGlassPoint   = "ステンドグラスカメラ位置";
        public const string DisplayCabinetPoint = "食器棚カメラ位置";
        public const string CandelabraPoint     = "燭台カメラ位置";
        public const string PortraitPoint       = "肖像画カメラ位置";
        public const string AltarPoint          = "祭壇カメラ位置";

        // ===== Lighting / Post Process =====
        public const string DirectionalLight       = "方向ライト";
        public const string GlobalPostProcessVolume = "ポストプロセス全体ボリューム";

        // ===== Controllers =====
        public const string RoomViewController = "部屋ビュー制御";

        // ===== Tags =====
        public const string Tag_MainCamera = "MainCamera";
    }
}
