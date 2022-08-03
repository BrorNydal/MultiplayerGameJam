using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DcClass
{
    [System.Serializable]
    public class KeyValuePair<Key, Val>
    {
        public Key key;
        public Val val;
    }

    [System.Serializable]
    public class SerializableDictionary<Key, Val>
    {
        [SerializeField] public List<KeyValuePair<Key, Val>> list = new List<KeyValuePair<Key, Val>>();

        Dictionary<Key, Val> dictionary = new Dictionary<Key, Val>();

        public void Awake()
        {
            foreach (var pair in list)
            {
                dictionary.Add(pair.key, pair.val);
            }
        }

        public bool KeyExists(Key key)
        { return dictionary.ContainsKey(key); }

        public Val this[Key key]
        {
            //called when we ask for something = mySession["value"]
            get
            {
                if (dictionary.ContainsKey(key))
                    return dictionary[key];
                else
                    return default(Val);
            }
            //called when we assign mySession["value"] = something
            set
            {
                if (dictionary.ContainsKey(key))
                    this[key] = value;
                else
                    dictionary.Add(key, value);
            }
        }
    }

    public class PersistentSingleton<T> : MonoBehaviour where T : Component
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

        protected virtual void Awake()
        {
            if (instance)
            {
                Destroy(this);
                Destroy(gameObject);
            }
            else
            {
                instance = gameObject.GetComponent<T>();
                DontDestroyOnLoad(instance);
            }
        }
    }

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
                    else
                    {
                        T[] all = FindObjectsOfType<T>();

                        foreach(var ob in all)
                            if(ob != instance)
                                Destroy(ob);
                    }
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
}
