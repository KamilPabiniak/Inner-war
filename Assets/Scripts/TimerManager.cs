using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private static List<Timer> _timers = new List<Timer>();

    public static TimerHandle Schedule(Action action, float delay)
    {
        Timer timer = new Timer(action, delay);
        _timers.Add(timer);
        return new TimerHandle(timer);
    }

    public static void Cancel(TimerHandle handle)
    {
        if (handle.Timer != null && _timers.Contains(handle.Timer))
        {
            _timers.Remove(handle.Timer);
            handle.Timer = null;
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        for (int i = _timers.Count - 1; i >= 0; i--)
        {
            if (_timers[i].Tick(deltaTime))
            {
                _timers.RemoveAt(i);
            }
        }
    }
}

public class Timer
{
    private readonly Action _action;
    private float _timeRemaining;

    public Timer(Action action, float delay)
    {
        _action = action;
        _timeRemaining = delay;
    }

    public bool Tick(float deltaTime)
    {
        _timeRemaining -= deltaTime;
        if (_timeRemaining <= 0)
        {
            _action?.Invoke();
            return true;
        }
        return false;
    }
}

public class TimerHandle
{
    internal Timer Timer { get; set; }

    public TimerHandle(Timer timer)
    {
        Timer = timer;
    }
}
