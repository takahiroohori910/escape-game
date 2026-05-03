using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // 本棚パズル解決後に現れる隠し本。肖像画の紋章順序ヒントを読める。
    [RequireComponent(typeof(Collider))]
    public class HiddenBookInteractable : MonoBehaviour
    {
        [SerializeField] private NoteData hiddenBookNote;
        [SerializeField] private GameObject bookVisual; // 通常は非表示、解決後に表示

        private void Start()
        {
            RefreshVisibility();
            FlagManager.Instance.OnFlagChanged.AddListener(OnFlagChanged);
        }

        private void OnDestroy()
        {
            FlagManager.Instance?.OnFlagChanged.RemoveListener(OnFlagChanged);
        }

        private void OnFlagChanged(string flagId, bool value)
        {
            if (flagId == Flags.BookshelfSolved) RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            bool unlocked = FlagManager.Instance.HasFlag(Flags.BookshelfSolved);
            if (bookVisual != null) bookVisual.SetActive(unlocked);
            GetComponent<Collider>().enabled = unlocked;
        }

        private void OnMouseDown()
        {
            if (RoomViewController.Instance.CurrentArea != RoomArea.Bookshelf) return;
            if (!FlagManager.Instance.HasFlag(Flags.BookshelfSolved)) return;

            FlagManager.Instance.SetFlag(Flags.HiddenBookRead);
            AudioManager.Instance?.PlaySE("SE_NoteOpen");
            NoteUI.Instance?.Show(hiddenBookNote);
        }
    }
}
