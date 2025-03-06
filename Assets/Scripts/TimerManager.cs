using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private static readonly List<Timer> Timers = new List<Timer>();
    private static readonly Queue<Timer> TimersToAdd = new Queue<Timer>();
    private static readonly Queue<Timer> TimersToRemove = new Queue<Timer>();

    public static void Schedule(Action action, float delay)
    {
        if (delay <= 0)
        {
            action?.Invoke();
            return;
        }

        Timer timer = new Timer(action, delay);
        TimersToAdd.Enqueue(timer);
    }

    public static void Cancel(TimerHandle handle)
    {
        if (handle?.Timer == null) return;
        TimersToRemove.Enqueue(handle.Timer);
        handle.Timer = null;
    }
    
    private void Update()
    {
        while (TimersToAdd.Count > 0)
        {
            Timers.Add(TimersToAdd.Dequeue());
        }
        
        while (TimersToRemove.Count > 0)
        {
            Timer timerToRemove = TimersToRemove.Dequeue();
            if (Timers.Contains(timerToRemove))
            {
                timerToRemove.Cancel();
                Timers.Remove(timerToRemove);
            }
        }

        float deltaTime = Time.deltaTime;
        for (int i = Timers.Count - 1; i >= 0; i--)
        {
            if (Timers[i].Tick(deltaTime))
            {
                Timers.RemoveAt(i);
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
        if (!(_timeRemaining <= 0)) return false;
        _action?.Invoke();
        Handle?.Invalidate();
        return true;
    }

    public void Cancel()
    {
        _isCanceled = true;
        Handle?.Invalidate();
    }
}

public abstract class TimerHandle
{
    internal Timer Timer { get; set; }

    protected TimerHandle(Timer timer)
    {
        Timer = timer;
        Timer.Handle = this;
    }

    public void Invalidate()
    {
        Timer = null; 
    }
}
