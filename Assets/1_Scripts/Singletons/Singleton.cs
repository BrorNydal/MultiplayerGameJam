using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T instance;

    /// <summary>
    /// Gets instance, or creates if none has been created yet.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();

                if (instance == null)
                {
                    GameObject go = new GameObject();
                    go.name = typeof(T).Name;
                    instance = go.AddComponent<T>();
                }

                DontDestroyOnLoad(instance);
            }
            
            return instance;
        }
    }

    /// <summary>
    /// Reference when destroying objects, so this won't duplicate and not get destroyed.
    /// </summary>
    public static T SafeInstance
    {
        get
        {
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = gameObject.GetComponent<T>();
            DontDestroyOnLoad(instance);
        }
    }
}
