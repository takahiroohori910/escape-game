using UnityEngine;
using UnityEngine.UI;
using EscapeGame.Core;

namespace EscapeGame.Game
{
    // MENUボタン（UIボタン）でOverview視点に戻る
    [RequireComponent(typeof(Button))]
    public class BackButton : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(() =>
                RoomViewController.Instance.MoveToOverview());
        }
    }
}
