namespace EscapeGame.Game
{
    // ゲーム全体で使うフラグID・アイテムIDの定数定義
    public static class Flags
    {
        // Room1
        public const string BookshelfSolved  = "bookshelf_solved";
        public const string DeskSolved       = "desk_solved";
        public const string ChestSolved      = "chest_solved";
        public const string PhoneRepaired    = "phone_repaired";
        public const string ClockInspected   = "clock_inspected";
        public const string HiddenBookRead   = "hidden_book_read";
        public const string Room1Cleared     = "room1_cleared";
        // Room2
        public const string DisplayCabinetSolved = "display_cabinet_solved";
        public const string CandelabraSolved     = "candelabra_solved";
        public const string PortraitSolved       = "portrait_solved";
        public const string AltarSolved          = "altar_solved";
    }

    public static class ItemIds
    {
        public const string PhoneCord    = "phone_cord";
        public const string CircuitBoard = "circuit_board";
        public const string RoomKey      = "room_key";
        public const string FlamePattern = "flame_pattern"; // 食器棚解錠で入手、燭台パズルのヒント
        public const string ChestHintOrder  = "chest_hint_order";  // 本棚解錠で入手：シンボル順序（暗号A）
        public const string ChestHintLegend = "chest_hint_legend"; // 机金庫解錠で入手：シンボル凡例（暗号B）
    }

    // カメラが映すエリアの定義
    public enum RoomArea
    {
        // Room1
        Overview,       // 部屋全体（初期視点）
        Bookshelf,      // 本棚エリア
        Desk,           // デスクエリア
        Fireplace,      // 暖炉エリア（撤去予定、互換のため残置）
        Chest,          // チェスト（暖炉跡）エリア
        // Room2
        Overview2,      // 祭壇の間（全体視点）
        StainedGlass,   // ステンドグラスエリア
        DisplayCabinet, // 食器棚エリア
        Candelabra,     // 燭台エリア
        Portrait,       // 肖像画エリア
        Altar,          // 祭壇エリア
    }
}
