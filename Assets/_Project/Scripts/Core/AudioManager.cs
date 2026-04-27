using System.Collections.Generic;
using UnityEngine;

namespace EscapeGame.Core
{
    // BGM・SEの再生を一元管理するマネージャー
    // AudioClipはPhase2BackSetupで配線。ファイルが未設定でも安全にスキップする
    public class AudioManager : SingletonMonoBehaviour<AudioManager>
    {
        [Header("BGM")]
        [SerializeField] private AudioClip bgmMain;
        [SerializeField] private AudioClip bgmClear;

        [Header("SE")]
        [SerializeField] private AudioClip seClick;
        [SerializeField] private AudioClip seHover;
        [SerializeField] private AudioClip seBookMove;
        [SerializeField] private AudioClip sePuzzleSolve;
        [SerializeField] private AudioClip sePuzzleFail;
        [SerializeField] private AudioClip seItemPickup;
        [SerializeField] private AudioClip seCameraMove;
        [SerializeField] private AudioClip seNoteOpen;
        [SerializeField] private AudioClip sePhoneRepair;
        [SerializeField] private AudioClip sePhoneCall;

        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.4f;
        [SerializeField] [Range(0f, 1f)] private float seVolume  = 0.8f;

        private AudioSource bgmSource;
        private AudioSource seSource;
        private int lastInventoryCount;

        private Dictionary<string, AudioClip> seMap;

        protected override void Awake()
        {
            base.Awake();
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop   = true;
            bgmSource.volume = bgmVolume;

            seSource = gameObject.AddComponent<AudioSource>();
            seSource.volume = seVolume;
        }

        private void Start()
        {
            BuildSeMap();
            SubscribeEvents();
            // BGMはユーザー操作後（TitleUI.Dismiss）に開始する
            // WebGL はユーザー操作前の自動再生をブロックするため
        }

        // TitleUI から呼ぶ（ゲーム開始・続きから）
        public void StartGameBGM() => PlayBGM(bgmMain);

        private void BuildSeMap()
        {
            seMap = new Dictionary<string, AudioClip>
            {
                { "SE_Click",        seClick       },
                { "SE_Hover",        seHover       },
                { "SE_BookMove",     seBookMove    },
                { "SE_PuzzleSolve",  sePuzzleSolve },
                { "SE_PuzzleFail",   sePuzzleFail  },
                { "SE_ItemPickup",   seItemPickup  },
                { "SE_CameraMove",   seCameraMove  },
                { "SE_NoteOpen",     seNoteOpen    },
                { "SE_PhoneRepair",  sePhoneRepair },
                { "SE_PhoneCall",    sePhoneCall   },
            };
        }

        private void SubscribeEvents()
        {
            GameManager.Instance.OnStateChanged.AddListener(OnStateChanged);

            var inv = InventoryManager.Instance;
            lastInventoryCount = inv.GetItems().Count;
            inv.OnInventoryChanged.AddListener(OnInventoryChanged);

            var bookshelf = FindAnyObjectByType<EscapeGame.Game.BookshelfPuzzle>();
            if (bookshelf != null) bookshelf.OnSolved.AddListener(() => PlaySE("SE_PuzzleSolve"));

            var desk = FindAnyObjectByType<EscapeGame.Game.DeskPuzzle>();
            if (desk != null) desk.OnSolved.AddListener(() => PlaySE("SE_PuzzleSolve"));

            var phone = FindAnyObjectByType<EscapeGame.Game.PhoneRepairPuzzle>();
            if (phone != null)
            {
                phone.OnRepaired.AddListener(() => PlaySE("SE_PhoneRepair"));
                phone.OnGameClear.AddListener(() => PlaySE("SE_PhoneCall"));
            }
        }

        private void OnStateChanged(EscapeGameState prev, EscapeGameState next)
        {
            if (next == EscapeGameState.Clear) PlayBGM(bgmClear);
        }

        private void OnInventoryChanged(System.Collections.Generic.IReadOnlyList<ItemData> items)
        {
            if (items.Count > lastInventoryCount) PlaySE("SE_ItemPickup");
            lastInventoryCount = items.Count;
        }

        public void PlaySE(string key)
        {
            if (seMap == null || !seMap.TryGetValue(key, out var clip) || clip == null) return;
            seSource.PlayOneShot(clip, seVolume);
        }

        private void PlayBGM(AudioClip clip)
        {
            if (clip == null) return;
            bgmSource.clip = clip;
            bgmSource.Play();
        }

        public void StopBGM() => bgmSource.Stop();
    }
}
