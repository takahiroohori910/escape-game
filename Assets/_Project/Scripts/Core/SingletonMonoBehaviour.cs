using UnityEngine;

namespace EscapeGame.Core
{
    // 各マネージャークラスが継承する汎用シングルトン基底クラス（DontDestroyOnLoad込み）
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    // FindObjectOfType より高速（ソート不要）
                    instance = FindAnyObjectByType<T>();
                    if (instance == null)
                    {
                        var obj = new GameObject(typeof(T).Name);
                        instance = obj.AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}
