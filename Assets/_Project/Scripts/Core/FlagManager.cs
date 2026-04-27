using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeGame.Core
{
    // ギミックの解錠・進行フラグを一括管理するマネージャー
    // フラグIDは文字列（例: "door_unlocked", "drawer_opened"）で管理する
    public class FlagManager : SingletonMonoBehaviour<FlagManager>
    {
        private readonly HashSet<string> flags = new();

        // フラグが変化したときのイベント（フラグID、立った/下りたの真偽値）
        public UnityEvent<string, bool> OnFlagChanged = new();

        // フラグを立てる
        public void SetFlag(string flagId)
        {
            if (flags.Add(flagId))
            {
                OnFlagChanged.Invoke(flagId, true);
                Debug.Log($"[FlagManager] フラグON: {flagId}");
            }
        }

        // フラグを下ろす
        public void ClearFlag(string flagId)
        {
            if (flags.Remove(flagId))
            {
                OnFlagChanged.Invoke(flagId, false);
                Debug.Log($"[FlagManager] フラグOFF: {flagId}");
            }
        }

        // フラグが立っているか確認する
        public bool HasFlag(string flagId) => flags.Contains(flagId);

        // 複数フラグが全て立っているか確認する（AND条件）
        public bool HasAllFlags(params string[] flagIds)
        {
            foreach (var id in flagIds)
            {
                if (!flags.Contains(id)) return false;
            }
            return true;
        }

        // セーブ用：フラグIDをJSON配列文字列に変換する
        // カンマ区切りはフラグID内のカンマと衝突するためJsonUtilityを使用
        public string SerializeFlags()
        {
            return JsonUtility.ToJson(new FlagList { ids = new List<string>(flags) });
        }

        // ロード用：JSON配列文字列からフラグを復元する
        public void DeserializeFlags(string data)
        {
            flags.Clear();
            if (string.IsNullOrEmpty(data)) return;
            var list = JsonUtility.FromJson<FlagList>(data);
            if (list?.ids == null) return;
            foreach (var id in list.ids)
            {
                if (!string.IsNullOrEmpty(id)) flags.Add(id);
            }
        }

        [System.Serializable]
        private class FlagList { public List<string> ids; }
    }
}
