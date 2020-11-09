using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadManager : MonoBehaviour
{
    public static ThreadManager _instance;

    private static readonly List<Action> _executeOnMainThread = new List<Action>();
    private static readonly List<Action> _executeCopiedOnMainThread = new List<Action>();
    private static bool _actionToExecuteOnMainThread = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
            return;
        }
    }

    
    private void Update()
    {
        // Makes sure update is called if timeScale is 0
        UpdateMain();
    }

    private void FixedUpdate()
    {
        UpdateMain();
    }

    /// <summary>Sets an action to be executed on the main thread.</summary>
    /// <param name="action">The action to be executed on the main thread.</param>
    public void ExecuteOnMainThread(Action action)
    {
        if (action == null)
        {
            Debug.Log("No action to execute on main thread!");
            return;
        }

        lock (_executeOnMainThread)
        {
            _executeOnMainThread.Add(action);
            _actionToExecuteOnMainThread = true;
        }
    }

    /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
    private void UpdateMain()
    {
        if (_actionToExecuteOnMainThread)
        {
            _executeCopiedOnMainThread.Clear();
            lock (_executeOnMainThread)
            {
                _executeCopiedOnMainThread.AddRange(_executeOnMainThread);
                _executeOnMainThread.Clear();
                _actionToExecuteOnMainThread = false;
            }

            for (int i = 0; i < _executeCopiedOnMainThread.Count; i++)
            {
                _executeCopiedOnMainThread[i]();
            }
        }
    }
}
