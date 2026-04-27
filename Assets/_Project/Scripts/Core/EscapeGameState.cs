// ゲームの進行状態を定義するEnum
// GameManagerがこの状態を参照し、画面遷移・操作可否を制御する
namespace EscapeGame.Core
{
    public enum EscapeGameState
    {
        Explore,    // 探索フェーズ：部屋を見回してアイテムやギミックを発見
        Puzzle,     // パズルフェーズ：謎解きUIを操作中
        Dialogue,   // 会話フェーズ：キャラクターとの対話中
        Inventory,  // インベントリフェーズ：アイテム一覧・合成操作中
        Clear,      // クリアフェーズ：脱出成功演出・リザルト表示
        Loading,    // ローディング：シーン遷移中
    }
}
