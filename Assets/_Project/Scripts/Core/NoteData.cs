using UnityEngine;

namespace EscapeGame.Core
{
    [CreateAssetMenu(fileName = "NewNote", menuName = "EscapeGame/NoteData")]
    public class NoteData : ScriptableObject
    {
        [SerializeField] private string noteId;
        [SerializeField] private string title;
        [TextArea(3, 10)]
        [SerializeField] private string content;

        public string NoteId  => noteId;
        public string Title   => title;
        public string Content => content;
    }
}
