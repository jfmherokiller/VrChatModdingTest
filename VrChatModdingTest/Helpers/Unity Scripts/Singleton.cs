using UnityEngine;

namespace SR_PluginLoader
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance = null;
        public static T Instance {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject(string.Format("(Singleton) {0}", typeof(T).FullName));
                    _instance = obj.AddComponent<T>();
                    UnityEngine.Object.DontDestroyOnLoad(obj);
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
        }
    }
}
