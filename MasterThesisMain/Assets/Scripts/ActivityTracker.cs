using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ActivityTracker : MonoBehaviour
{
    public static ActivityTracker Instance;

    private float gameStartTime;
    private Dictionary<string, int> actionCounts = new();
    private Dictionary<string, float> timers = new();
    private HashSet<string> stoppedTimers = new();
    private string sessionId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameStartTime = Time.realtimeSinceStartup;
            sessionId = Guid.NewGuid().ToString("N");
        }
        else
        {
            Destroy(gameObject);
        }
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
        timers[timerName] = Time.realtimeSinceStartup;
    }

    public void StopTimer(string timerName)
    {
        if (timers.ContainsKey(timerName))
        {
            timers[timerName] = Time.realtimeSinceStartup - timers[timerName];
            stoppedTimers.Add(timerName);
        }
    }
    private void OnApplicationQuit()
    {
        var keys = new List<string>(timers.Keys);
        foreach (var key in keys)
        {
            if (stoppedTimers.Contains(key)) continue;

            float storedValue = timers[key];
            float now = Time.realtimeSinceStartup;
            float duration = now - storedValue;

            timers[key] = duration;
        }

        UploadTrackingData();
    }

    public void UploadTrackingData()
    {
        var data = new TrackingData
        {
            sessionId = sessionId,
            totalTime = Time.realtimeSinceStartup - gameStartTime,
            actions = actionCounts.Select(a => new ActionEntry { name = a.Key, count = a.Value }).ToList(),
            timers = timers.Select(t => new TimerEntry { name = t.Key, duration = t.Value }).ToList()
        };

        string json = JsonUtility.ToJson(data);
        UploadNow(json);
    }

    private IEnumerator SendToGoogleSheets(string json)
    {
        string url = "https://script.google.com/macros/s/AKfycbx5vN_JlWGl5m87mMndjGH3ZZkPaJnSYV3RYKRXKHgck1kYFJPVyCVP1eTnLiO2wCSw/exec";

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log("Tracking data uploaded successfully.");
        else
            Debug.LogError("Upload failed: " + request.error);
    }
    private void UploadNow(string json)
    {
        string url = "https://script.google.com/macros/s/AKfycbx5vN_JlWGl5m87mMndjGH3ZZkPaJnSYV3RYKRXKHgck1kYFJPVyCVP1eTnLiO2wCSw/exec";

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.SendWebRequest();
        while (!request.isDone) { }

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log("Tracking data uploaded successfully.");
        else
            Debug.LogError("Upload failed: " + request.error);
    }
    [Serializable]
    public class TrackingData
    {
        public string sessionId;
        public float totalTime;
        public List<ActionEntry> actions;
        public List<TimerEntry> timers;
    }

    [Serializable]
    public class ActionEntry
    {
        public string name;
        public int count;
    }

    [Serializable]
    public class TimerEntry
    {
        public string name;
        public float duration;
    }
}
