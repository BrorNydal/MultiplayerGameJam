using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DcClass;

public class EventDictionary : MonoBehaviour
{
    [SerializeField] SerializableDictionary<string, UnityEvent> events;

    void Awake()
    {
        events.Awake();
    }

    public void Invoke(string ev)
    {
        if (events.KeyExists(ev))
            events[ev].Invoke();
    }
}
