using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TutorialData.Model;

public class DataReader : MonoBehaviour
{
    [SerializeField] private TextAsset jsonFile;
    [SerializeField] private TextAsset helpTextJson;

    public static DataReader Instance { get; private set; }

    private Dictionary<string, HelpText> helpTexts;

    private Dictionary<string, List<TutorialStep>> tutorialData;

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
        tutorialData = JsonConvert.DeserializeObject<Dictionary<string, List<TutorialStep>>>(jsonFile.text);
        helpTexts = JsonConvert.DeserializeObject<Dictionary<string, HelpText>>(helpTextJson.text);
        // Debug.Log("Tutorial Steps: " + tutorialSteps);
        // Debug.Log("Tutorial Steps: " + tutorialSteps[0].Title);
    }

    public List<TutorialStep> GetTutorialSteps(string name)
    {
        return tutorialData.TryGetValue(name, out var steps) ? steps : new List<TutorialStep>();
    }

    public List<TutorialStep> GetIntroductionSteps()
    {
        return tutorialData.TryGetValue("introduction", out var steps) ? steps : new List<TutorialStep>();
    }
    public Dictionary<string, HelpText> GetHelpTexts()
    {
        return helpTexts;
    }

    public HelpText GetHelpText(string key)
    {
        return helpTexts != null && helpTexts.TryGetValue(key, out HelpText value) ? value : null;
    }
    public List<TutorialStep> FirstNetworkNotValid()
    {
        return tutorialData.TryGetValue("firstNetworkNotValid", out var steps) ? steps : new List<TutorialStep>();
    }

    public List<TutorialStep> FirstNetworkValid()
    {
        return tutorialData.TryGetValue("firstNetworkValid", out var steps) ? steps : new List<TutorialStep>();
    }
    public List<TutorialStep> SecondNetworkNotValid()
    {
        return tutorialData.TryGetValue("secondNetworkNotValid", out var steps) ? steps : new List<TutorialStep>();
    }
    public List<TutorialStep> SecondNetworkValid()
    {
        return tutorialData.TryGetValue("secondNetworkValid", out var steps) ? steps : new List<TutorialStep>();
    }
    public List<TutorialStep> SecondNetworkTrainedBad()
    {
        return tutorialData.TryGetValue("secondNetworkTrainedBad", out var steps) ? steps : new List<TutorialStep>();
    }
    public List<TutorialStep> SecondNetworkTrainedGood()
    {
        return tutorialData.TryGetValue("secondNetworkTrainedGood", out var steps) ? steps : new List<TutorialStep>();
    }
    public List<TutorialStep> GetRLIntroduction()
    {
        return tutorialData.TryGetValue("thirdPuzzleSolved", out var steps) ? steps : new List<TutorialStep>();
    }
}