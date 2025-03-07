using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

public class TimerManagerEditorWindow : EditorWindow
{
    private Vector2 _scrollPos;

    [MenuItem("Window/Debug/Timer Manager")]
    public static void ShowWindow()
    {
        GetWindow<TimerManagerEditorWindow>("Timer Manager Debug");
    }

    private void OnEnable()
    {
        // Subscribe to the editor update event for continuous refresh.
        EditorApplication.update += UpdateWindow;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateWindow;
    }

    private void UpdateWindow()
    {
        Repaint();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Timer Manager Debug", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play mode to debug TimerManager.", MessageType.Info);
            return;
        }

        // Using reflection to retrieve the private fields of TimerManager.
        Type timerManagerType = typeof(TimerManager);
        FieldInfo timersField = timerManagerType.GetField("Timers", BindingFlags.Static | BindingFlags.NonPublic);
        FieldInfo timersToAddField = timerManagerType.GetField("TimersToAdd", BindingFlags.Static | BindingFlags.NonPublic);
        FieldInfo timersToRemoveField = timerManagerType.GetField("TimersToRemove", BindingFlags.Static | BindingFlags.NonPublic);

        if (timersField == null || timersToAddField == null || timersToRemoveField == null)
        {
            EditorGUILayout.HelpBox("Could not find TimerManager fields.", MessageType.Error);
            return;
        }

        // Cast to System.Collections.IList for List<Timer>
        IList timers = timersField.GetValue(null) as IList;
        // Cast to System.Collections.IEnumerable for Queue<Timer>
        IEnumerable timersToAdd = timersToAddField.GetValue(null) as IEnumerable;
        IEnumerable timersToRemove = timersToRemoveField.GetValue(null) as IEnumerable;

        int timersCount = timers?.Count ?? 0;
        int timersToAddCount = 0;
        int timersToRemoveCount = 0;

        if (timersToAdd != null)
        {
            IEnumerable toAdd = timersToAdd.Cast<object>().ToList();
            foreach (object _ in toAdd)
                timersToAddCount++;

            if (timersToRemove != null)
            {
                IEnumerable toRemove = timersToRemove.Cast<object>().ToList();
                foreach (object _ in toRemove)
                    timersToRemoveCount++;

                EditorGUILayout.LabelField("Active Timers: " + timersCount);
                EditorGUILayout.LabelField("Timers to Add: " + timersToAddCount);
                EditorGUILayout.LabelField("Timers to Remove: " + timersToRemoveCount);

                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

                if (timers != null && timersCount > 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Active Timers", EditorStyles.boldLabel);
                    foreach (object timer in timers)
                    {
                        if (timer == null) continue;
                        DrawTimerInfo(timer);
                    }
                }

                {
                    bool headerDrawn = false;
                    foreach (object timer in toAdd)
                    {
                        if (timer == null) continue;
                        if (!headerDrawn)
                        {
                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Timers Pending Addition", EditorStyles.boldLabel);
                            headerDrawn = true;
                        }
                        DrawTimerInfo(timer);
                    }
                }

                {
                    bool headerDrawn = false;
                    foreach (object timer in toRemove)
                    {
                        if (timer == null) continue;
                        if (!headerDrawn)
                        {
                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Timers Pending Removal", EditorStyles.boldLabel);
                            headerDrawn = true;
                        }
                        DrawTimerInfo(timer);
                    }
                }
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawTimerInfo(object timer)
    {
        if (timer == null)
            return;

        Type timerType = timer.GetType();
        // Retrieve private fields: _timeRemaining, _isCanceled, _action and the new _callerInfo.
        FieldInfo timeRemainingField = timerType.GetField("_timeRemaining", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo isCanceledField = timerType.GetField("_isCanceled", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo actionField = timerType.GetField("_action", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo callerInfoField = timerType.GetField("_callerInfo", BindingFlags.Instance | BindingFlags.NonPublic);

        float timeRemaining = timeRemainingField != null ? Convert.ToSingle(timeRemainingField.GetValue(timer)) : 0f;
        bool isCanceled = isCanceledField != null && Convert.ToBoolean(isCanceledField.GetValue(timer));
        Delegate actionDel = actionField != null ? actionField.GetValue(timer) as Delegate : null;
        string actionName = actionDel != null ? actionDel.Method.Name : "none";
        string callerInfo = callerInfoField != null ? callerInfoField.GetValue(timer) as string : "unknown";

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Timer", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Time Remaining: " + timeRemaining.ToString("F2") + " s");
        EditorGUILayout.LabelField("Canceled: " + isCanceled);
        EditorGUILayout.LabelField("Action: " + actionName);
        EditorGUILayout.LabelField("Caller Info: " + callerInfo);

        // Retrieve TimerHandle (if available)
        PropertyInfo handleProperty = timerType.GetProperty("Handle", BindingFlags.Instance | BindingFlags.Public);
        object handleValue = handleProperty != null ? handleProperty.GetValue(timer) : null;
        EditorGUILayout.LabelField("TimerHandle: " + (handleValue != null ? handleValue.GetType().Name : "null"));

        EditorGUILayout.EndVertical();
    }
}
