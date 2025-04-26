
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ActivityTracker : MonoBehaviour
{
    public static ActivityTracker Instance;

    private float gameStartTime;
    private Dictionary<string, int> actionCounts = new();
    private Dictionary<string, float> timers = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameStartTime = Time.time;
        }
        else
        {
            Destroy(gameObject);
        }
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
        {
            timers[timerName] = Time.time - timers[timerName];
        }
    }

    public void SaveTrackingData()
    {
        var path = Application.dataPath + "/TrackingData/TrackingData_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("Total Play Time: " + (Time.time - gameStartTime));
            foreach (var action in actionCounts)
                writer.WriteLine($"{action.Key}: {action.Value} times");
            foreach (var timer in timers)
                writer.WriteLine($"{timer.Key}: {timer.Value} seconds");
        }
    }
}
