using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

public class TimerManager : MonoBehaviour
{
    private static readonly List<Timer> Timers = new();
    private static readonly Queue<Timer> TimersToAdd = new();
    private static readonly Queue<Timer> TimersToRemove = new();
    
    public static TimerHandle Schedule(Action action, float delay,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callerLine = 0)
    {
        if (delay <= 0)
        {
            delay = 0.001f;
        }

        string callerInfo = $"{callerName} in {callerFile}:{callerLine}";
        var timer = new Timer(action, delay, callerInfo);
        var handle = new InternalTimerHandle(timer);
        TimersToAdd.Enqueue(timer);
        return handle;
    }

    public static void Cancel(TimerHandle handle)
    {
        if (handle?.Timer == null) return;
        TimersToRemove.Enqueue(handle.Timer);
        handle.Timer = null;
    }
    
    private class InternalTimerHandle : TimerHandle 
    { 
        public InternalTimerHandle(Timer timer) : base(timer) { }
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
    private readonly string _callerInfo;
    private float _timeRemaining;
    private bool _isCanceled;

    public TimerHandle Handle { get; set; }
    
    public string CallerInfo => _callerInfo;

    public Timer(Action action, float delay, string callerInfo)
    {
        _action = action;
        _timeRemaining = delay;
        _callerInfo = callerInfo;
    }

    public bool Tick(float deltaTime)
    {
        if (_isCanceled) return true;

        _timeRemaining -= deltaTime;
        if (_timeRemaining > 0) return false;
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
