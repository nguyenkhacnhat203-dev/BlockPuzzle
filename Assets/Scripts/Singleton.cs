using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] protected bool _dontDestroyOnLoad = true;

    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject(typeof(T).Name);
                    _instance
                    = singleton.AddComponent<T>();
                    DontDestroyOnLoad(singleton);
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (_dontDestroyOnLoad)
            {
                var root = transform.root;

                if (root = transform)
                {
                    DontDestroyOnLoad(root);
                }
                else
                {
                    DontDestroyOnLoad(this.gameObject);
                }
            }

        }
        else
        {
            Destroy(gameObject);
        }
    }
}
