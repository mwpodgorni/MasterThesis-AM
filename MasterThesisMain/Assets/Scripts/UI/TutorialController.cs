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
        // Debug.Log("TutorialController Awake called");
        ui = GetComponent<UIDocument>().rootVisualElement;
        nextButton = ui.Q<Button>("NextButton");
        tutorialTitle = ui.Q<Label>("TutorialTitle");
        tutorialContent = ui.Q<Label>("TutorialContent");

        simpleDelay = new WaitForSeconds(typingSpeed);
        punctuationDelay = new WaitForSeconds(interpunctuationDelay);
        skipDelay = new WaitForSeconds(1 / (typingSpeed * skipSpeedup));
    }

    private void OnEnable()
    {
        nextButton.clicked += OnNextButtonClicked;
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
            // Debug.Log("Tutorial Complete");
            ui.Q<VisualElement>("TutorialPanel").AddToClassList("opacity-none");
            StartCoroutine(HideTutorialPanel());
            // if (StateManager.Instance.MiniGame3Solved)
            // {
            //     StageOneController.Instance.LoadSecondStage();

            // }
        }
    }
    private IEnumerator HideTutorialPanel(float delay = 0f)
    {
        // Debug.Log("Hiding tutorial panel after delay: " + delay);
        yield return new WaitForSeconds(delay);
        ui.Q<VisualElement>("TutorialPanel").AddToClassList("opacity-none");
        displayTime = 0f;
        yield return new WaitForSeconds(1f);
        // Debug.Log("Hiding tutorial panel");
        ui.Q<VisualElement>("TutorialPanel").style.display = DisplayStyle.None;
    }
    public void StartTypingTutorialStep()
    {
        if (currentStep >= messages.Count) return;

        var step = messages[currentStep];

        // 1) begin typing…
        StopAllCoroutines();
        tutorialTitle.text = "";
        tutorialContent.text = "";
        StartCoroutine(ShowTitle(step.Title));

        // 2) if there’s an EventName in JSON, try to call it
        if (!string.IsNullOrEmpty(step.EventName))
            InvokeStepEvent(step.EventName);
    }

    public void SetTypeText(bool value)
    {
        typeText = value;
    }
    private IEnumerator ShowTitle(string title)
    {
        yield return StartCoroutine(ShowText(tutorialTitle, title));

        if (currentStep < messages.Count)
        {
            StartCoroutine(ShowText(tutorialContent, string.Join("\n", messages[currentStep].Content)));
        }
    }
    // "Tip: Hidden layers of the same size often help the network discover complex relationships more effectively."
    private IEnumerator ShowText(Label label, string text)
    {
        StringBuilder sb = new StringBuilder(text);
        label.text = "";
        int maxVisibleCharacters = 0;

        while (maxVisibleCharacters < sb.Length)
        {
            maxVisibleCharacters++;
            label.text = sb.ToString(0, maxVisibleCharacters);

            char currentChar = sb[maxVisibleCharacters - 1];
            if (IsPunctuation(currentChar))
            {
                yield return typeText ? punctuationDelay : 0f; ;
            }
            else
            {

                yield return typeText ? simpleDelay : 0f;
            }
        }
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
        // look for a method on this class with exactly that name, no args
        var mi = GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );
        if (mi != null)
        {
            mi.Invoke(this, null);
        }
        else
        {
            Debug.LogWarning($"TutorialController: no method named '{methodName}'");
        }
    }

    public void AddToEvent(UnityAction action)
    {
        tutorialCompletedEvent.AddListener(action);
    }


    // Event firing methods
    public void ShowWorkshopButton()
    {
        StageOneController.Instance.ShowWorkshopOpenButton();
        nextButton.style.display = DisplayStyle.None;
    }
    public void RLShowWorkshopButton()
    {
        Debug.Log("RLShowWorkshopButton called");
        RLController.Instance.ShowWorkshopOpenButton();
    }
    public void EndGame()
    {
        Debug.Log("EndGame called");
        Application.Quit();
    }
}
