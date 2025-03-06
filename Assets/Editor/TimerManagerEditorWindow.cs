using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

public class TimerManagerEditorWindow : EditorWindow
{
    private Vector2 scrollPos;

    [MenuItem("Window/Debug/Timer Manager")]
    public static void ShowWindow()
    {
        GetWindow<TimerManagerEditorWindow>("Timer Manager Debug");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Timer Manager Debug", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Uruchom tryb Play, aby debugowaæ TimerManager.", MessageType.Info);
            return;
        }

        // U¿ywamy refleksji, aby pobraæ prywatne pola TimerManagera.
        Type timerManagerType = typeof(TimerManager);
        FieldInfo timersField = timerManagerType.GetField("Timers", BindingFlags.Static | BindingFlags.NonPublic);
        FieldInfo timersToAddField = timerManagerType.GetField("TimersToAdd", BindingFlags.Static | BindingFlags.NonPublic);
        FieldInfo timersToRemoveField = timerManagerType.GetField("TimersToRemove", BindingFlags.Static | BindingFlags.NonPublic);

        if (timersField == null || timersToAddField == null || timersToRemoveField == null)
        {
            EditorGUILayout.HelpBox("Nie uda³o siê odnaleŸæ pól TimerManagera.", MessageType.Error);
            return;
        }

        List<object> timers = timersField.GetValue(null) as List<object>;
        Queue<object> timersToAdd = timersToAddField.GetValue(null) as Queue<object>;
        Queue<object> timersToRemove = timersToRemoveField.GetValue(null) as Queue<object>;

        EditorGUILayout.LabelField("Liczba aktywnych timerów: " + (timers != null ? timers.Count.ToString() : "0"));
        EditorGUILayout.LabelField("Liczba timerów do dodania: " + (timersToAdd != null ? timersToAdd.Count.ToString() : "0"));
        EditorGUILayout.LabelField("Liczba timerów do usuniêcia: " + (timersToRemove != null ? timersToRemove.Count.ToString() : "0"));

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        if (timers != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Aktywne Timery", EditorStyles.boldLabel);
            foreach (object timer in timers)
            {
                if (timer == null) continue;
                DrawTimerInfo(timer);
            }
        }

        if (timersToAdd != null && timersToAdd.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Timery oczekuj¹ce na dodanie", EditorStyles.boldLabel);
            foreach (object timer in timersToAdd)
            {
                if (timer == null) continue;
                DrawTimerInfo(timer);
            }
        }

        if (timersToRemove != null && timersToRemove.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Timery oczekuj¹ce na usuniêcie", EditorStyles.boldLabel);
            foreach (object timer in timersToRemove)
            {
                if (timer == null) continue;
                DrawTimerInfo(timer);
            }
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Odœwie¿"))
        {
            Repaint();
        }
    }

    private void DrawTimerInfo(object timer)
    {
        if (timer == null)
            return;

        Type timerType = timer.GetType();
        // Pobieramy prywatne pola _timeRemaining, _isCanceled oraz _action
        FieldInfo timeRemainingField = timerType.GetField("_timeRemaining", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo isCanceledField = timerType.GetField("_isCanceled", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo actionField = timerType.GetField("_action", BindingFlags.Instance | BindingFlags.NonPublic);

        float timeRemaining = timeRemainingField != null ? (float)timeRemainingField.GetValue(timer) : 0f;
        bool isCanceled = isCanceledField != null ? (bool)isCanceledField.GetValue(timer) : false;
        Delegate actionDel = actionField != null ? actionField.GetValue(timer) as Delegate : null;
        string actionName = actionDel != null && actionDel.Method != null ? actionDel.Method.Name : "brak";

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Timer", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Pozosta³y czas: " + timeRemaining.ToString("F2") + " s");
        EditorGUILayout.LabelField("Anulowany: " + isCanceled);
        EditorGUILayout.LabelField("Akcja: " + actionName);

        // Pobieramy TimerHandle (jeœli jest dostêpny)
        PropertyInfo handleProperty = timerType.GetProperty("Handle", BindingFlags.Instance | BindingFlags.Public);
        object handleValue = handleProperty != null ? handleProperty.GetValue(timer) : null;
        EditorGUILayout.LabelField("TimerHandle: " + (handleValue != null ? handleValue.GetType().Name : "null"));

        EditorGUILayout.EndVertical();
    }
}
