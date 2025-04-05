using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TutorialData.Model;

public class DataReader : MonoBehaviour
{
    [SerializeField] private TextAsset jsonFile;
    public static DataReader Instance { get; private set; }

    private List<TutorialStep> tutorialSteps;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Init();
        DontDestroyOnLoad(gameObject);
    }

    private void Init()
    {
        tutorialSteps = JsonConvert.DeserializeObject<List<TutorialStep>>(jsonFile.text);
        // Debug.Log("Tutorial Steps: " + tutorialSteps);
        // Debug.Log("Tutorial Steps: " + tutorialSteps[0].Title);
    }
    public List<TutorialStep> GetTutorialSteps()
    {
        return tutorialSteps;
    }
}