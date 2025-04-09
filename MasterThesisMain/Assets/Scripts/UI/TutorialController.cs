using System.Collections;
using System.Collections.Generic;
using TutorialData.Model;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using System.Text;

public class TutorialController : MonoBehaviour
{
    public VisualElement ui;
    public Button nextButton;
    public Label tutorialTitle;
    public Label tutorialContent;
    private List<TutorialStep> tutorialSteps;
    private int currentStep = 0;
    private float typingSpeed = 0.03f;
    private float interpunctuationDelay = 0.4f;
    private float skipSpeedup = 5f;

    private WaitForSeconds simpleDelay;
    private WaitForSeconds punctuationDelay;
    private WaitForSeconds skipDelay;

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
        currentStep = 0;
        StartTypingTutorialStep();
    }

    void Start()
    {
        tutorialSteps = DataReader.Instance.GetTutorialSteps();
        StartTypingTutorialStep();
    }

    private void OnNextButtonClicked()
    {
        currentStep++;
        if (currentStep < tutorialSteps.Count)
        {
            StartTypingTutorialStep();
        }
        else
        {
            tutorialCompletedEvent?.Invoke();
            Debug.Log("Tutorial Complete");
            ui.Q<VisualElement>("TutorialPanel").AddToClassList("opacity-none");
        }
    }

    private void StartTypingTutorialStep()
    {
        if (currentStep < tutorialSteps.Count)
        {
            StopAllCoroutines();
            tutorialTitle.text = "";
            tutorialContent.text = "";
            StartCoroutine(ShowTitle(tutorialSteps[currentStep].Title));
        }
    }

    private IEnumerator ShowTitle(string title)
    {
        yield return StartCoroutine(ShowText(tutorialTitle, title));

        StartCoroutine(ShowText(tutorialContent, string.Join("\n", tutorialSteps[currentStep].Content)));
    }

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
                yield return punctuationDelay;
            }
            else
            {
                yield return simpleDelay;
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
}
