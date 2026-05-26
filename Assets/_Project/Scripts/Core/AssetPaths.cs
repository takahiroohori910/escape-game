namespace EscapeGame.Core
{
    // AssetDatabase.LoadAssetAtPath / Resources.Load などで渡すアセットパスの単一情報源。
    // Editor スクリプトを中心に参照される。
    public static class AssetPaths
    {
        // ===== Fonts =====
        public const string Font_NotoSansJP_Fresh = "Assets/_Project/Fonts/NotoSansJP_Fresh.asset";

        // ===== Materials (固有) =====
        public const string Mat_Wall  = "Assets/_Project/Materials/WallMaterial.mat";
        public const string Mat_Desk  = "Assets/_Project/Materials/DeskMaterial.mat";
        public const string Mat_Floor = "Assets/_Project/Materials/FloorMaterial.mat";

        // ===== Materials (Generated) =====
        public const string MatGen_Wall        = "Assets/_Project/Materials/Generated/Mat_Wall.mat";
        public const string MatGen_Floor       = "Assets/_Project/Materials/Generated/Mat_Floor.mat";
        public const string MatGen_Ceiling     = "Assets/_Project/Materials/Generated/Mat_Ceiling.mat";
        public const string MatGen_WoodDark    = "Assets/_Project/Materials/Generated/Mat_Wood_Dark.mat";
        public const string MatGen_WindowGlass = "Assets/_Project/Materials/Generated/Mat_WindowGlass.mat";

        // ===== ScriptableObjects (Items) =====
        public const string Item_PhoneCord    = "Assets/_Project/ScriptableObjects/Items/PhoneCord.asset";
        public const string Item_CircuitBoard = "Assets/_Project/ScriptableObjects/Items/CircuitBoard.asset";
        public const string Item_FlamePattern = "Assets/_Project/ScriptableObjects/Items/FlamePattern.asset";

        // ===== ScriptableObjects (Notes) =====
        public const string Note_StainedGlassPlaque = "Assets/_Project/ScriptableObjects/Notes/NoteStainedGlassPlaque.asset";
        public const string Note_CabinetHint        = "Assets/_Project/ScriptableObjects/Notes/NoteCabinetHint.asset";

        // ===== Textures =====
        public const string Tex_Window_4Arches = "Assets/_Project/Textures/Window_4Arches.png";
    }
}
