using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ActivityTracker : MonoBehaviour
{
    public static ActivityTracker Instance;

    private float gameStartTime;
    private Dictionary<string, int> actionCounts = new();
    private Dictionary<string, float> timers = new();
    private string sessionId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameStartTime = Time.time;
            sessionId = GenerateSessionId();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private string GenerateSessionId()
    {
        return Guid.NewGuid().ToString("N");
    }

    public string GetSessionId()
    {
        return sessionId;
    }

    public void RecordAction(string actionName)
    {
        if (!actionCounts.ContainsKey(actionName))
            actionCounts[actionName] = 0;
        actionCounts[actionName]++;
    }

    public void StartTimer(string timerName)
    {
        timers[timerName] = Time.time;
    }

    public void StopTimer(string timerName)
    {
        if (timers.ContainsKey(timerName))
            timers[timerName] = Time.time - timers[timerName];
    }

    public void SaveTrackingData()
    {
        var dir = Application.dataPath + "/TrackingData";
        Directory.CreateDirectory(dir);
        var path = dir + "/TrackingData_" + sessionId + ".txt";

        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("Session ID: " + sessionId);
            writer.WriteLine("Total Play Time: " + (Time.time - gameStartTime));
            foreach (var action in actionCounts)
                writer.WriteLine($"{action.Key}: {action.Value} times");
            foreach (var timer in timers)
                writer.WriteLine($"{timer.Key}: {timer.Value} seconds");
        }
    }

    private void OnApplicationQuit()
    {
        var keys = new List<string>(timers.Keys);
        foreach (var key in keys)
        {
            if (timers[key] >= 0)
                timers[key] = Time.time - timers[key];
        }
        SaveTrackingData();
    }
}
