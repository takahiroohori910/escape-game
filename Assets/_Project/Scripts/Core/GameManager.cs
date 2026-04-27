using UnityEngine;
using UnityEngine.Events;

namespace EscapeGame.Core
{
    // ゲーム全体の状態管理を担うマネージャー
    // 状態遷移のたびにOnStateChangedイベントを発火し、各UIが自律的に表示切替できる設計
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        [SerializeField] private EscapeGameState initialState = EscapeGameState.Explore;

        public EscapeGameState CurrentState { get; private set; }

        // 状態変化を他のシステムに通知するイベント（旧状態、新状態）
        public UnityEvent<EscapeGameState, EscapeGameState> OnStateChanged = new();

        protected override void Awake()
        {
            base.Awake();
            CurrentState = initialState;
        }

        private void Start()
        {
            PlatformBridge.InitializeAds();
        }

        // 状態を変更する唯一の窓口
        public void ChangeState(EscapeGameState newState)
        {
            if (CurrentState == newState) return;

            var prevState = CurrentState;
            CurrentState = newState;
            OnStateChanged.Invoke(prevState, newState);

            Debug.Log($"[GameManager] 状態遷移: {prevState} → {newState}");
        }

        // クリア時の処理（広告表示 → クリア状態へ遷移）
        public void TriggerClear()
        {
            PlatformBridge.ShowInterstitialAd(() =>
            {
                ChangeState(EscapeGameState.Clear);
            });
        }
    }
}
