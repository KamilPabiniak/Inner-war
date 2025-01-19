using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private static readonly List<Timer> _timers = new List<Timer>();
    private static readonly Queue<Timer> _timersToAdd = new Queue<Timer>();
    private static readonly Queue<Timer> _timersToRemove = new Queue<Timer>();

    public static TimerHandle Schedule(Action action, float delay)
    {
        if (delay <= 0)
        {
            action?.Invoke();
            return null;
        }

        Timer timer = new Timer(action, delay);
        _timersToAdd.Enqueue(timer);
        return new TimerHandle(timer);
    }

    public static void Cancel(TimerHandle handle)
    {
        if (handle?.Timer != null)
        {
            _timersToRemove.Enqueue(handle.Timer);
            handle.Timer = null;
        }
    }
    
    private void Update()
    {
        while (_timersToAdd.Count > 0)
        {
            _timers.Add(_timersToAdd.Dequeue());
        }
        
        while (_timersToRemove.Count > 0)
        {
            Timer timerToRemove = _timersToRemove.Dequeue();
            if (_timers.Contains(timerToRemove))
            {
                timerToRemove.Cancel();
                _timers.Remove(timerToRemove);
            }
        }

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
    private bool _isCanceled;

    public TimerHandle Handle { get; set; }
    public Timer(Action action, float delay)
    {
        _action = action;
        _timeRemaining = delay;
    }

    public bool Tick(float deltaTime)
    {
        if (_isCanceled) return true;

        _timeRemaining -= deltaTime;
        if (_timeRemaining <= 0)
        {
            _action?.Invoke();
            Handle?.Invalidate();
            return true;
        }
        return false;
    }

    public void Cancel()
    {
        _isCanceled = true;
        Handle?.Invalidate();
    }
}

public class TimerHandle
{
    internal Timer Timer { get; set; }

    public TimerHandle(Timer timer)
    {
        Timer = timer;
        Timer.Handle = this;
    }

    public void Invalidate()
    {
        Timer = null; 
    }
}
