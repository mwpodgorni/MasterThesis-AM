using System.Collections;
using System.Collections.Generic;
using TutorialData.Model;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using System.Text;
using System.Linq;
using System.Reflection;
public class TutorialController : MonoBehaviour
{
    public GameObject typingAudioPrefab;
    public ObjectiveController objectiveController;
    AudioSource typingAudio;
    public VisualElement ui;
    public Button nextButton;
    public Label tutorialTitle;
    public Label tutorialContent;
    private List<TutorialStep> messages;
    private int currentStep = 0;
    private float typingSpeed = 0.01f;
    private float interpunctuationDelay = 0.4f;
    private float skipSpeedup = 5f;

    private WaitForSeconds simpleDelay;
    private WaitForSeconds punctuationDelay;
    private WaitForSeconds skipDelay;
    private bool typeText = true;
    private float displayTime = 0f;

    [SerializeField]
    private UnityEvent tutorialCompletedEvent;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        nextButton = ui.Q<Button>("NextButton");
        tutorialTitle = ui.Q<Label>("TutorialTitle");
        tutorialContent = ui.Q<Label>("TutorialContent");

        simpleDelay = new WaitForSeconds(typingSpeed);
        punctuationDelay = new WaitForSeconds(interpunctuationDelay);
        skipDelay = new WaitForSeconds(1 / (typingSpeed * skipSpeedup));
        objectiveController = FindObjectOfType<ObjectiveController>();
    }

    private void OnEnable()
    {
        nextButton.clicked += OnNextButtonClicked;
        if (typingAudioPrefab != null)
        {
            typingAudio = Instantiate(typingAudioPrefab).GetComponent<AudioSource>();
        }
    }

    private void OnDisable()
    {
        nextButton.clicked -= OnNextButtonClicked;
    }

    public void StartTutorial()
    {
        ui.Q<VisualElement>("TutorialPanel").style.display = DisplayStyle.Flex;
        ui.Q<VisualElement>("TutorialPanel").RemoveFromClassList("opacity-none");
        currentStep = 0;
        StartTypingTutorialStep();
        if (displayTime != 0f)
        {
            StartCoroutine(HideTutorialPanel(displayTime));
        }
    }

    void Start()
    {
        messages = DataReader.Instance.GetIntroductionSteps();
        StartTypingTutorialStep();
    }
    public void SetTutorialSteps(List<TutorialStep> steps)
    {
        messages = steps;
        currentStep = 0;
    }
    public void OnNextButtonClicked()
    {
        currentStep++;
        if (currentStep < messages.Count)
        {
            StartTypingTutorialStep();
        }
        else
        {
            typeText = true;
            tutorialCompletedEvent?.Invoke();
            ui.Q<VisualElement>("TutorialPanel").AddToClassList("opacity-none");
            StartCoroutine(HideTutorialPanel());
        }
    }
    private IEnumerator HideTutorialPanel(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        ui.Q<VisualElement>("TutorialPanel").AddToClassList("opacity-none");
        displayTime = 0f;
        yield return new WaitForSeconds(1f);
        ui.Q<VisualElement>("TutorialPanel").style.display = DisplayStyle.None;
        if (typingAudio != null && typingAudio.isPlaying)
            typingAudio.Stop();
    }
    public void StartTypingTutorialStep()
    {
        if (currentStep >= messages.Count) return;

        var step = messages[currentStep];

        StopAllCoroutines();
        tutorialTitle.text = "";
        tutorialContent.text = "";
        StartCoroutine(ShowTitle(step.Title));

        if (!string.IsNullOrEmpty(step.EventName))
            InvokeStepEvent(step.EventName);
    }

    public void SetTypeText(bool value)
    {
        typeText = value;
    }
    private IEnumerator ShowTitle(string title)
    {
        yield return StartCoroutine(ShowText(tutorialTitle, title, false));

        if (currentStep < messages.Count)
        {
            StartCoroutine(ShowText(tutorialContent, string.Join("\n", messages[currentStep].Content), true)); // sound for text
        }
    }
    private IEnumerator ShowText(Label label, string text, bool playSound)
    {
        StringBuilder sb = new StringBuilder(text);
        label.text = "";
        int maxVisibleCharacters = 0;

        if (playSound && typingAudio != null && !typingAudio.isPlaying)
            typingAudio.Play();

        while (maxVisibleCharacters < sb.Length)
        {
            maxVisibleCharacters++;
            label.text = sb.ToString(0, maxVisibleCharacters);

            char currentChar = sb[maxVisibleCharacters - 1];
            if (IsPunctuation(currentChar))
            {
                yield return typeText ? punctuationDelay : 0f;
            }
            else
            {
                yield return typeText ? simpleDelay : 0f;
            }
        }

        if (playSound && typingAudio != null && typingAudio.isPlaying)
            typingAudio.Stop();
    }

    private bool IsPunctuation(char c)
    {
        return c == '.'
        // || c == ',' 
        || c == '?'
        || c == '!'
         || c == ';'
         || c == ':';
    }
    public void HideNextButton()
    {
        nextButton.style.display = DisplayStyle.None;
    }
    public void ShowNextButton()
    {
        nextButton.style.display = DisplayStyle.Flex;
    }
    public void SetDisplayTime(float time)
    {
        displayTime = time;
    }
    private void InvokeStepEvent(string methodName)
    {
        var mi = GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );
        if (mi != null)
        {
            mi.Invoke(this, null);
        }

    }

    public void AddToEvent(UnityAction action)
    {
        tutorialCompletedEvent.AddListener(action);
    }


    public void ShowWorkshopButton()
    {
        StageOneController.Instance.ShowWorkshopOpenButton();
        nextButton.style.display = DisplayStyle.None;
    }
    public void RLShowWorkshopButton()
    {
        RLController.Instance.ShowWorkshopOpenButton();
    }
    public void ShowEndScreen()
    {
        StartCoroutine(ShowEndScreenCoroutine());
    }
    IEnumerator ShowEndScreenCoroutine()
    {
        yield return new WaitForSeconds(5f);
        RLController.Instance.ShowEndScreen();
    }
    public void EndGame()
    {
        Application.Quit();
    }
    public void SetObjective()
    {
        if (StateManager.Instance.CurrentStage == GameStage.FirstHelpOpen)
        {
            objectiveController.SetObjective("Add nodes to all layers and press Test to validate the network.");
            objectiveController.ShowObjective();
        }
        if (StateManager.Instance.CurrentStage == GameStage.FirstNetworkValidated)
        {
            objectiveController.SetObjective("Build a network to analyze robot parts and predict their type. Configure the layers, then test and train it.");
            objectiveController.ShowObjective();
        }
        if (StateManager.Instance.CurrentStage == GameStage.RLOneStart)
        {
            objectiveController.SetObjective("Adjust rewards and training configuration to guide the robot to the goal tile. Then start and observe the training.");
            objectiveController.ShowObjective();
        }
        if (StateManager.Instance.CurrentStage == GameStage.RLTwoStart)
        {
            objectiveController.SetObjective("Configure rewards and training settings to teach the robot to collect power-ups and defuse threats efficiently.");
            objectiveController.ShowObjective();
        }
        if (StateManager.Instance.CurrentStage == GameStage.RLThreeStart)
        {
            objectiveController.SetObjective("Balance rewards and configuration to train the robot to collect items, avoid threats, and reach the goal efficiently.");
            objectiveController.ShowObjective();
        }
    }
    public void HideObjective()
    {
        objectiveController.HideObjective();
    }
}
