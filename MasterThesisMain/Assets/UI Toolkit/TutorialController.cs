using System.Collections;
using System.Collections.Generic;
using TutorialData.Model;
using UnityEngine;
using UnityEngine.UIElements;

public class TutorialController : MonoBehaviour
{
    public VisualElement ui;
    public Button nextButton;
    public Label tutorialTitle;
    public Label tutorialContent;
    private List<TutorialStep> tutorialSteps;
    private int currentTutorialStep = 0;
    private float typingSpeed = 0.01f;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        nextButton = ui.Q<Button>("NextButton");
        nextButton.clicked += OnNextButtonClicked;

        tutorialTitle = ui.Q<Label>("TutorialTitle");
        tutorialContent = ui.Q<Label>("TutorialContent");
    }

    void Start()
    {
        tutorialTitle.text = "";
        tutorialContent.text = "";
        tutorialSteps = DataReader.Instance.GetTutorialSteps();
    }
    public void StartTutorial()
    {
        StopAllCoroutines();
        currentTutorialStep = 0;
        StartCoroutine(ShowTutorialStep(tutorialSteps[currentTutorialStep]));
        currentTutorialStep++;
    }

    private void OnNextButtonClicked()
    {
        StopAllCoroutines();

        if (currentTutorialStep < tutorialSteps.Count)
        {
            StartCoroutine(ShowTutorialStep(tutorialSteps[currentTutorialStep]));
            currentTutorialStep++;
        }
    }

    private IEnumerator ShowTutorialStep(TutorialStep step)
    {
        tutorialTitle.text = "";
        tutorialContent.text = "";

        yield return StartCoroutine(TypeText(tutorialTitle, step.Title, typingSpeed));

        yield return StartCoroutine(TypeText(tutorialContent, string.Join("\n", step.Content), typingSpeed));
    }

    private IEnumerator TypeText(Label label, string text, float delay)
    {
        foreach (char c in text)
        {
            label.text += c;
            yield return new WaitForSeconds(delay);
        }
    }
}
