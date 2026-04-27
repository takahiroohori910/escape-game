using UnityEngine;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // シーン内のメモ・日記オブジェクトにアタッチして調べられるようにする
    [RequireComponent(typeof(Collider))]
    public class NoteInteractable : MonoBehaviour
    {
        [SerializeField] private NoteData noteData;
        [SerializeField] private RoomArea requiredArea;

        private void OnMouseDown()
        {
            if (RoomViewController.Instance.CurrentArea != requiredArea) return;
            EscapeGame.Core.AudioManager.Instance?.PlaySE("SE_NoteOpen");
            NoteUI.Instance?.Show(noteData);
        }
    }
}
