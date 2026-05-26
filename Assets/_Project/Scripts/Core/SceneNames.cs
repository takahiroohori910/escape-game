namespace EscapeGame.Core
{
    // シーン内 GameObject 名の単一情報源。
    // GameObject.Find / transform.Find / name 比較などで文字列を直書きせず、ここを参照する。
    // 値を変更するとリネームと連動する。
    public static class SceneNames
    {
        // ===== 共通 / Managers / Canvas =====
        public const string CanvasMain   = "Canvas_Main";
        public const string Managers     = "Managers";
        public const string Prefabs      = "_Prefabs";

        // ===== UI Panel / Overlay =====
        public const string HintButton           = "HintButton";
        public const string HintPanel            = "HintPanel";
        public const string MenuButton           = "MenuButton";
        public const string TitleOverlay         = "TitleOverlay";
        public const string ClearOverlay         = "ClearOverlay";
        public const string TimerText            = "TimerText";
        public const string SubText              = "SubText";
        public const string PopupPanel           = "PopupPanel";
        public const string NoteOverlay          = "NoteOverlay";
        public const string ItemDetailPanel      = "ItemDetailPanel";
        public const string BookshelfStatusPanel = "BookshelfStatusPanel";
        public const string InventoryBar         = "InventoryBar";
        public const string NumberPadPanel       = "NumberPadPanel";
        public const string DisplayCabinetPanel  = "DisplayCabinetPanel";

        // ===== Room1 主要オブジェクト =====
        public const string Bookshelf      = "Bookshelf";
        public const string DeskTop        = "DeskTop";
        public const string Chest          = "Chest";
        public const string DeskSafe       = "DeskSafe";
        public const string FurnitureCandidates = "FurnitureCandidates";
        public const string FireplacePhoto = "FireplacePhoto";
        public const string FireplacePointLight = "FireplacePointLight";
        public const string FireplaceOpening    = "FireplaceOpening";
        public const string FireEmber           = "FireEmber";
        public const string Clock          = "Clock";
        public const string Painting       = "Painting";
        public const string Telephone        = "Telephone";
        public const string TelephoneHandset = "TelephoneHandset";
        public const string NoteOnDesk       = "NoteOnDesk";
        public const string NoteOnBookshelf  = "NoteOnBookshelf";
        public const string NoteOnFireplace  = "NoteOnFireplace";
        public const string BackWall         = "BackWall";

        // ----- Bookshelf 内部 -----
        public const string S2_01     = "S2_01";
        public const string BS_Left   = "BS_Left";
        public const string BS_Right  = "BS_Right";
        public const string BS_Bottom = "BS_Bottom";
        public const string BS_Shelf1 = "BS_Shelf1";
        public const string BS_Shelf2 = "BS_Shelf2";

        // ----- Chest 内部 -----
        public const string ChestPoint = "ChestPoint";

        // ===== Room2 主要オブジェクト =====
        public const string Room2                  = "Room2";
        public const string R2_StainedGlassRoot    = "R2_StainedGlassRoot";
        public const string R2_DisplayCabinetRoot  = "R2_DisplayCabinetRoot";
        public const string R2_CandelabraRoot      = "R2_CandelabraRoot";
        public const string R2_AltarRoot           = "R2_AltarRoot";

        public const string R2_ClickZone_StainedGlass = "R2_ClickZone_StainedGlass";
        public const string R2_ClickZone_Cabinet      = "R2_ClickZone_Cabinet";
        public const string R2_ClickZone_Candelabra   = "R2_ClickZone_Candelabra";
        public const string R2_ClickZone_Altar        = "R2_ClickZone_Altar";

        public const string CandelabraPuzzle = "CandelabraPuzzle";
        public const string Altar_Lock       = "Altar_Lock";

        // ----- Candelabra 動的命名テンプレ -----
        // 使い方: string.Format(SceneNames.CandleStickFormat, i)
        public const string CandleStickFormat = "Candle_{0}_Stick";

        // ===== Camera Anchors / Points =====
        public const string CameraAnchors       = "CameraAnchors";
        public const string Overview2Point      = "Overview2Point";
        public const string StainedGlassPoint   = "StainedGlassPoint";
        public const string DisplayCabinetPoint = "DisplayCabinetPoint";
        public const string CandelabraPoint     = "CandelabraPoint";
        public const string PortraitPoint       = "PortraitPoint";
        public const string AltarPoint          = "AltarPoint";

        // ===== Lighting / Post Process =====
        public const string DirectionalLight       = "Directional Light";
        public const string GlobalPostProcessVolume = "GlobalPostProcessVolume";

        // ===== Controllers =====
        public const string RoomViewController = "RoomViewController";

        // ===== Tags =====
        public const string Tag_MainCamera = "MainCamera";
    }
}
