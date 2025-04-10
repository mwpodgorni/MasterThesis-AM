using System.Collections;
using System.Collections.Generic;
using Alexwsu.EventChannels;
using UnityEngine;
using UnityEngine.UI;

public class GP : MonoBehaviour
{

    public static Parameters Instance { get; private set; }
    public static IntEventChannel ChannelInstance { get; private set; }


    [SerializeField]
    private Parameters _parameters;

    [SerializeField]
    private IntEventChannel _channel;
    [SerializeField]
    TextAsset firstMiniGameDataset;


    private void Awake()
    {
        if (Instance == null || ChannelInstance == null)
        {
            Instance = _parameters;
            ChannelInstance = _channel;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static Parameters GetParameters()
    {
        return Instance;
    }
    public static TextAsset GetFirstMiniGameDataset()
    {
        GP gpInstance = FindObjectOfType<GP>();
        if (gpInstance != null)
        {
            return gpInstance.firstMiniGameDataset;
        }
        else
        {
            Debug.LogError("GP instance not found!");
            return null;
        }
    }

}
