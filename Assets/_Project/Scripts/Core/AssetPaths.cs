namespace EscapeGame.Core
{
    // AssetDatabase.LoadAssetAtPath / Resources.Load などで渡すアセットパスの単一情報源。
    // Editor スクリプトを中心に参照される。
    public static class AssetPaths
    {
        // ===== Fonts =====
        // TMP 動的アトラスの都合で英数字維持。
        public const string Font_NotoSansJP_Fresh = "Assets/_Project/Fonts/NotoSansJP_Fresh.asset";

        // ===== Materials (固有) =====
        // 旧アセット。既に削除済みでファイル自体は存在しないが、参照側コード保持のため const は残す。
        public const string Mat_Wall  = "Assets/_Project/Materials/WallMaterial.mat";
        public const string Mat_Desk  = "Assets/_Project/Materials/DeskMaterial.mat";
        public const string Mat_Floor = "Assets/_Project/Materials/FloorMaterial.mat";

        // ===== Materials (Generated) =====
        public const string MatGen_Wall        = "Assets/_Project/Materials/Generated/壁マテリアル_生成.mat";
        public const string MatGen_Floor       = "Assets/_Project/Materials/Generated/床マテリアル_生成.mat";
        public const string MatGen_Ceiling     = "Assets/_Project/Materials/Generated/天井マテリアル_生成.mat";
        public const string MatGen_WoodDark    = "Assets/_Project/Materials/Generated/暗木マテリアル_生成.mat";
        public const string MatGen_WindowGlass = "Assets/_Project/Materials/Generated/窓ガラスマテリアル_生成.mat";

        // ===== ScriptableObjects (Items) =====
        // PhoneCord / CircuitBoard は旧アセット（削除済み）。const は参照側コード保持のため残置。
        public const string Item_PhoneCord    = "Assets/_Project/ScriptableObjects/Items/PhoneCord.asset";
        public const string Item_CircuitBoard = "Assets/_Project/ScriptableObjects/Items/CircuitBoard.asset";
        public const string Item_FlamePattern = "Assets/_Project/ScriptableObjects/Items/炎パターン.asset";

        // ===== ScriptableObjects (Notes) =====
        public const string Note_StainedGlassPlaque = "Assets/_Project/ScriptableObjects/Notes/ステンドグラス銘板メモ.asset";
        public const string Note_CabinetHint        = "Assets/_Project/ScriptableObjects/Notes/食器棚ヒントメモ.asset";

        // ===== Textures =====
        public const string Tex_Window_4Arches = "Assets/_Project/Textures/アーチ窓画像.png";
    }
}
