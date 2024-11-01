using UnityEngine;

namespace _KMH_Framework._TS_Module
{
    /// <summary>
    /// Hierachy���� ������ MonoSingleton(GameObject)�� Awake()�� ��� <para/>
    /// T.Instance�� ������ MonoSingleton�̸� Initialized()�� ��� <para/>
    /// </summary>
    abstract public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance = null;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(T)) as T;
                    Instance = _instance ?? (new GameObject(typeof(T).Name)).AddComponent<T>();
                }
                return _instance;
            }

            private set
            {
                if (value != null)
                {
                    _instance = value;
                }
            }            
        }
        abstract public void Awake();
    }
}