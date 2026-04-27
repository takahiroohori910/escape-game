namespace EscapeGame.Game
{
    // ゲーム全体で使うフラグID・アイテムIDの定数定義
    // 文字列の直書きによるタイポを防ぐ
    public static class Flags
    {
        public const string BookshelfSolved  = "bookshelf_solved";   // 本棚パズル解決
        public const string DeskSolved       = "desk_solved";        // デスクパズル解決
        public const string PhoneRepaired    = "phone_repaired";     // 電話修理完了
        public const string ClockInspected   = "clock_inspected";    // 時計を調べた（デスクヒント解放）
    }

    public static class ItemIds
    {
        public const string PhoneCord  = "phone_cord";   // 電話の部品①：受話器コード
        public const string CircuitBoard = "circuit_board"; // 電話の部品②：内部基板
    }

    // カメラが映すエリアの定義
    public enum RoomArea
    {
        Overview,   // 部屋全体（初期視点）
        Bookshelf,  // 本棚エリア
        Desk,       // デスクエリア
        Fireplace,  // 暖炉エリア
    }
}
