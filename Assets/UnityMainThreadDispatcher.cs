using System;
using System.Collections.Concurrent;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;

    private readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();
    private static readonly object _lock = new object(); // Dedicated lock object

    public static UnityMainThreadDispatcher Instance
    {
        get
        {
            // Thread-safe check to ensure the instance is only created once
            if (_instance == null)
            {
                // Use a lock to prevent multiple threads from creating the instance simultaneously
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Double-check to prevent race condition
                        GameObject go = new GameObject("UnityMainThreadDispatcher");
                        _instance = go.AddComponent<UnityMainThreadDispatcher>();
                        DontDestroyOnLoad(go); // Prevent destruction on scene changes
                    }
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        // Ensure that only one instance exists
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Another instance of UnityMainThreadDispatcher already exists. Destroying this one.");
        }
    }


    void Update()
    {
        while (_executionQueue.Count > 0)
        {
            if (_executionQueue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }
    }

    public static void Enqueue(Action action)
    {
        Instance._executionQueue.Enqueue(action);
    }
}