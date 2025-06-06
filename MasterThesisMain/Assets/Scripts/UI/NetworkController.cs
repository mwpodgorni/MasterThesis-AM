using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
public class NetworkController : MonoBehaviour
{
    [SerializeField] VisualTreeAsset networkLayerTemplate;
    [SerializeField] VisualTreeAsset networkNodeTemplate;

    [SerializeField] List<RobotSorter> robotSorters = new();

    // input layer
    public Button inputNodeAddBtn;
    public Button inputNodeRemoveBtn;
    // hidden layers
    public Button hiddenLayerAddBtn;
    public Button hiddenLayerRemoveBtn;
    // output layer
    public Button outputNodeAddBtn;
    public Button outputNodeRemoveBtn;
    public Button testNetworkButton;
    public Button trainNetworkButton;
    // UI elements
    public VisualElement ui;
    public VisualElement miniGamePanel;
    VisualElement _hiddenLayerPanel;
    VisualElement _inputLayerPanel;
    VisualElement _outputLayerPanel;
    VisualElement networkActionPanel;

    SliderInt trainingCycleSlider;
    Label trainingCycleLabel;
    Slider learningRateSlider;
    Label learningRateLabel;

    Label objectiveText;

    NeuralNetwork neuralNetwork;
    ConnectionLines _connectionLines;
    ProgressBar progressBar;
    ProgressBar outerProgressBar;
    NetworkSolution minigame2Solution = new NetworkSolution(3, 3, 2, new int[] { 4, 4 });
    NetworkSolution minigame2Solution2 = new NetworkSolution(3, 3, 2, new int[] { 3, 3 });

    public void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        miniGamePanel = ui.Q<VisualElement>("MiniGamePanel");
        neuralNetwork = new NeuralNetwork(this);
    }
    public void OnEnable()
    {
        string levelName = "StageOne_";

        objectiveText = ui.Q<Label>("ObjectiveText");
        _hiddenLayerPanel = ui.Q<VisualElement>("HiddenLayers");
        _inputLayerPanel = ui.Q<VisualElement>("InputLayerPanel");
        _outputLayerPanel = ui.Q<VisualElement>("OutputLayerPanel");
        networkActionPanel = ui.Q<VisualElement>("NetworkActionPanel");
        HideNetworkActionPanel();

        InitializeSliders();


        // buttons
        testNetworkButton = ui.Q<Button>("TestNetworkButton");
        testNetworkButton.clicked += OnTestNetworkButtonClicked;
        trainNetworkButton = ui.Q<Button>("TrainNetworkButton");
        trainNetworkButton.clicked += OnTrainNetworkButtonClicked;
        inputNodeAddBtn = ui.Q<Button>("InputNodeAddBtn");
        inputNodeAddBtn.clicked += () => AddNode(_inputLayerPanel);
        inputNodeRemoveBtn = ui.Q<Button>("InputNodeRemoveBtn");
        inputNodeRemoveBtn.clicked += () => RemoveNode(_inputLayerPanel);

        hiddenLayerAddBtn = ui.Q<Button>("HiddenLayerAddBtn");
        hiddenLayerAddBtn.clicked += AddHiddenLayer;
        hiddenLayerRemoveBtn = ui.Q<Button>("HiddenLayerRemoveBtn");
        hiddenLayerRemoveBtn.clicked += RemoveHiddenLayer;

        outputNodeAddBtn = ui.Q<Button>("OutputNodeAddBtn");
        outputNodeAddBtn.clicked += () => AddNode(_outputLayerPanel);
        outputNodeRemoveBtn = ui.Q<Button>("OutputNodeRemoveBtn");
        outputNodeRemoveBtn.clicked += () => RemoveNode(_outputLayerPanel);

        // Tracking
        if (ActivityTracker.Instance != null)
        {
            testNetworkButton.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "Test_Pressed"); });
            trainNetworkButton.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "Train_Pressed"); });

            inputNodeAddBtn.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "Input_AddNode_Pressed"); });
            inputNodeRemoveBtn.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "Input_RemoveNode_Pressed"); });

            hiddenLayerAddBtn.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "Hidden_AddLayer_Pressed"); });
            hiddenLayerAddBtn.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "Hidden_RemoveLayer_Pressed"); });

            outputNodeAddBtn.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "Output_AddNode_Pressed"); });
            outputNodeRemoveBtn.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "Output_RemoveNode_Pressed"); });
        }
        // line renderer
        _connectionLines = new ConnectionLines();
        var miniGamePanel = ui.Q<VisualElement>("WorkshopPanel");
        miniGamePanel.Add(_connectionLines);
        _connectionLines.style.position = Position.Absolute;
        _connectionLines.style.top = 0;
        _connectionLines.style.left = 0;
        _connectionLines.style.right = 0;
        _connectionLines.style.bottom = 0;
        _connectionLines.AddToClassList("connection-lines");

        _connectionLines.pickingMode = PickingMode.Ignore;

        neuralNetwork.OnEvaluationUpdate = UpdateEvaluationData;
        neuralNetwork.OnTrainingCompleted = UpdateTrainingCompleted;

        progressBar = ui.Q<ProgressBar>("ProgressBar");
        progressBar.lowValue = 0;
        progressBar.value = 0;
        outerProgressBar = ui.Q<ProgressBar>("OuterProgressBar");
        outerProgressBar.lowValue = 0;
        outerProgressBar.value = 0;
        StartCoroutine(DelayedSetup());

        if (robotSorters.Count > 0)
        {
            foreach (var sorter in robotSorters)
                neuralNetwork.OnEvaluationUpdate += sorter.UpdateAccuracy;
        }
    }
    public void ClassifyNetworkData()
    {
        if (neuralNetwork == null)
        {
            return;
        }

        neuralNetwork.ClassifyExamplesFromTrainingSet();
    }

    public void UpdateEvaluationData(EvaluationData data)
    {
        StageOneController.Instance.EvaluationController().UpdateEvaluationData(data);
        progressBar.value = neuralNetwork.GetCurrentEpoch();
        outerProgressBar.value = neuralNetwork.GetCurrentEpoch();
    }
    public void UpdateTrainingCompleted()
    {
        StateManager.Instance.SetState(GameStage.SecondNetworkTrained);
        StageOneController.Instance.SetFinishedTraining(true);
        StageOneController.Instance.ChangeStateIfEvaluationOpen();
        progressBar.value = 0;
        outerProgressBar.value = 0;
    }
    public void AddHiddenLayer()
    {
        if (_hiddenLayerPanel.childCount >= GP.Instance.maxLayers) return;
        VisualElement layer = networkLayerTemplate.Instantiate();

        layer.Q<Button>("LayerAddButton").clicked += () => AddNode(layer);
        layer.Q<Button>("LayerRemoveButton").clicked += () => RemoveNode(layer);

        if (ActivityTracker.Instance != null)
        {
            layer.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction("StageOne_" + "Hidden_AddNode_Pressed"); });
            layer.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction("StageOne_" + "Hidden_RemoveNode_Pressed"); });
        }

        _hiddenLayerPanel.Q<VisualElement>("HiddenLayers").Add(layer);
        neuralNetwork.AddHiddenLayer();
        RedrawConnections();
        layer.RegisterCallback<GeometryChangedEvent>((evt) => RedrawConnections());
    }
    public void RemoveHiddenLayer()
    {
        var hiddenLayersContainer = _hiddenLayerPanel.Q<VisualElement>("HiddenLayers");

        if (hiddenLayersContainer.childCount <= 0) return;

        hiddenLayersContainer.RemoveAt(hiddenLayersContainer.childCount - 1);
        neuralNetwork.RemoveHiddenLayer();
        RedrawConnections();
        hiddenLayersContainer.RegisterCallback<GeometryChangedEvent>((evt) => RedrawConnections());
    }
    public void DisableTrainingButton()
    {
        trainNetworkButton.SetEnabled(false);
    }
    public void EnableTrainingButton()
    {
        trainNetworkButton.SetEnabled(true);
    }
    public void DisableTestButton()
    {
        testNetworkButton.SetEnabled(false);
    }
    public void EnableTestButton()
    {
        testNetworkButton.SetEnabled(true);
    }

    public void DisableNetworkEditing()
    {
        DisableHiddenLayerButtons();
        DisableInputLayerButtons();
        DisableOutputLayerButtons();
    }

    public void EnableNetworkEditing()
    {
        EnableHiddenLayerButtons();
        EnableInputLayerButtons();
        EnableOutputLayerButtons();
    }
    public void DisableLearningRateSlider()
    {
        learningRateSlider.SetEnabled(false);
    }
    public void EnableLearningRateSlider()
    {
        learningRateSlider.SetEnabled(true);
    }
    public void DisableTrainingCycleSlider()
    {
        trainingCycleSlider.SetEnabled(false);
    }
    public void EnableTrainingCycleSlider()
    {
        trainingCycleSlider.SetEnabled(true);
    }

    public void AddNode(VisualElement layer)
    {
        var childCount = layer.Q<VisualElement>("NodeWrapper").childCount;
        if (childCount >= GP.Instance.maxNodes) return;
        if (layer.name == "InputLayerPanel")
        {
            neuralNetwork.AddInputLayerNode();
        }
        else if (layer.name == "OutputLayerPanel")
        {
            neuralNetwork.AddOutputLayerNode();
        }
        else
        {
            neuralNetwork.AddHiddenLayerNode(_hiddenLayerPanel.IndexOf(layer));
        }

        VisualElement node = networkNodeTemplate.Instantiate();
        node.style.width = 90;
        node.style.height = 90;
        layer.Q<VisualElement>("NodeWrapper").Add(node);
        node.RegisterCallback<GeometryChangedEvent>((evt) => RedrawConnections());
    }
    public void RemoveNode(VisualElement layer)
    {
        var nodeWrapper = layer.Q<VisualElement>("NodeWrapper");
        if (nodeWrapper.childCount <= 0) return;
        nodeWrapper.RemoveAt(nodeWrapper.childCount - 1);
        if (layer.name == "InputLayerPanel")
        {
            neuralNetwork.RemoveInputLayerNode();
        }
        else if (layer.name == "OutputLayerPanel")
        {
            neuralNetwork.RemoveOutputLayerNode();
        }
        else
        {
            neuralNetwork.RemoveHiddenLayerNode(_hiddenLayerPanel.IndexOf(layer));
        }
        RedrawConnections();
    }
    public void OnTestNetworkButtonClicked()
    {
        if (StateManager.Instance.CurrentStage == GameStage.FirstHelpOpen)
        {
            if (neuralNetwork.IsNetworkValid())
            {
                StateManager.Instance.SetState(GameStage.FirstNetworkValidated);
            }
            else
            {
                StageOneController.Instance.TutorialController().HideNextButton();
                StageOneController.Instance.TutorialController().SetTypeText(false);
                StageOneController.Instance.TutorialController().SetDisplayTime(5f);
                StageOneController.Instance.TutorialController().SetTutorialSteps(DataReader.Instance.FirstNetworkNotValid());
                StageOneController.Instance.TutorialController().StartTutorial();
            }
        }
        else if (StateManager.Instance.CurrentStage == GameStage.FirstNetworkValidated)
        {
            if (minigame2Solution.Matches(neuralNetwork) || minigame2Solution2.Matches(neuralNetwork))
            {
                StageOneController.Instance.TutorialController().SetTypeText(true);
                StageOneController.Instance.TutorialController().ShowNextButton();
                StageOneController.Instance.TutorialController().SetTutorialSteps(DataReader.Instance.FirstNetworkValid());
                StageOneController.Instance.TutorialController().StartTutorial();

                StateManager.Instance.SetState(GameStage.SecondNetworkValidated);
            }
            else
            {
                StageOneController.Instance.TutorialController().HideNextButton();
                StageOneController.Instance.TutorialController().SetTypeText(false);
                StageOneController.Instance.TutorialController().SetDisplayTime(5f);
                StageOneController.Instance.TutorialController().SetTutorialSteps(DataReader.Instance.SecondNetworkNotValid());
                StageOneController.Instance.TutorialController().StartTutorial();
            }
        }
    }
    public void OnTrainNetworkButtonClicked()
    {
        if (neuralNetwork.IsNetworkValid())
        {
            neuralNetwork.TrainNetwork(trainingCycleSlider.value, learningRateSlider.value);
            StateManager.Instance.SetState(GameStage.SecondNetworkTraining);
        }
    }
    public void ClearLines()
    {
        _connectionLines.ClearLines();

    }
    public void RedrawConnections()
    {
        _connectionLines.ClearLines();

        // 1 Get the same parent that contains _connectionLines
        var miniGamePanel = ui.Q<VisualElement>("WorkshopPanel");
        if (miniGamePanel == null)
        {
            return;
        }

        // 2 Convert node positions to local coords
        var inputNodeWrapper = _inputLayerPanel.Q<VisualElement>("NodeWrapper");
        List<Vector2> inputLayerPositions = new List<Vector2>();
        foreach (var inputNode in inputNodeWrapper.Children())
        {
            Vector2 screenPos = GetCenterScreenPosition(inputNode);
            Vector2 localPos = miniGamePanel.WorldToLocal(screenPos);
            inputLayerPositions.Add(localPos);
        }

        List<List<Vector2>> hiddenLayerPositions = new List<List<Vector2>>();
        foreach (var hiddenLayer in _hiddenLayerPanel.Children())
        {
            var hiddenNodeWrapper = hiddenLayer.Q<VisualElement>("NodeWrapper");
            List<Vector2> currentLayerPositions = new List<Vector2>();

            foreach (var hiddenNode in hiddenNodeWrapper.Children())
            {
                Vector2 screenPos = GetCenterScreenPosition(hiddenNode);
                Vector2 localPos = miniGamePanel.WorldToLocal(screenPos);
                currentLayerPositions.Add(localPos);
            }

            hiddenLayerPositions.Add(currentLayerPositions);
        }

        if (hiddenLayerPositions.Count > 0)
        {
            foreach (var inputPos in inputLayerPositions)
            {
                foreach (var hiddenPos in hiddenLayerPositions[0])
                {
                    _connectionLines.AddConnection(inputPos, hiddenPos);
                }
            }
        }

        for (int i = 0; i < hiddenLayerPositions.Count - 1; i++)
        {
            var currentLayer = hiddenLayerPositions[i];
            var nextLayer = hiddenLayerPositions[i + 1];

            foreach (var currentPos in currentLayer)
            {
                foreach (var nextPos in nextLayer)
                {
                    _connectionLines.AddConnection(currentPos, nextPos);
                }
            }
        }

        if (hiddenLayerPositions.Count > 0)
        {
            var lastHiddenLayer = hiddenLayerPositions[hiddenLayerPositions.Count - 1];
            var outputNodeWrapper = _outputLayerPanel.Q<VisualElement>("NodeWrapper");
            foreach (var lastHiddenPos in lastHiddenLayer)
            {
                foreach (var outputNode in outputNodeWrapper.Children())
                {
                    Vector2 outputScreenPos = GetCenterScreenPosition(outputNode);
                    Vector2 outputLocalPos = miniGamePanel.WorldToLocal(outputScreenPos);
                    _connectionLines.AddConnection(lastHiddenPos, outputLocalPos);
                }
            }
        }

        // Force a redraw
        _connectionLines.MarkDirtyRepaint();
    }


    Vector2 GetCenterScreenPosition(VisualElement ve)
    {
        var worldPos = ve.worldBound;
        return worldPos.center;
    }
    IEnumerator DelayedSetup()
    {
        yield return null;
        // SetupTestNetwork();
        SetUpHelpClickEvents();
    }
    void SetupTestNetwork()
    {
        // Setup a test network with 2 hidden layers and 3 nodes in each layer
        AddNode(_inputLayerPanel);
        AddNode(_inputLayerPanel);
        AddNode(_inputLayerPanel);

        for (int i = 0; i < 2; i++)
        {
            AddHiddenLayer();
            for (int j = 0; j < 3; j++)
            {
                AddNode(_hiddenLayerPanel.Children().ElementAt(i));
            }
        }

        AddNode(_outputLayerPanel);
        AddNode(_outputLayerPanel);
        AddNode(_outputLayerPanel);
        RedrawConnections();
    }
    public void SetUpHelpClickEvents()
    {
        MakeLabelClickable(ui.Q<Label>("HelpInputLayer"), "InputLayer");
        MakeLabelClickable(ui.Q<Label>("HelpHiddenLayers"), "HiddenLayers");
        MakeLabelClickable(ui.Q<Label>("HelpOutputLayer"), "OutputLayer");
        MakeLabelClickable(ui.Q<Label>("HelpTrainingCycle"), "TrainingCycle");
        MakeLabelClickable(ui.Q<Label>("HelpLearningRate"), "LearningRate");
    }
    void MakeLabelClickable(Label label, string helpKey)
    {
        label.RegisterCallback<ClickEvent>(_ => StageOneController.Instance.HelpController().ShowHelp(helpKey));
        {
            if (ActivityTracker.Instance != null)
            {
                label.RegisterCallback<ClickEvent>(_ => { ActivityTracker.Instance.RecordAction("StageOne_" + helpKey + "_Help_Pressed"); });
            }
        }
    }
    public void ResetNetwork()
    {
        _inputLayerPanel.Q<VisualElement>("NodeWrapper").Clear();
        _outputLayerPanel.Q<VisualElement>("NodeWrapper").Clear();
        var hiddenLayersContainer = _hiddenLayerPanel.Q<VisualElement>("HiddenLayers");
        hiddenLayersContainer.Clear();
        RedrawConnections();
        StageOneController.Instance.SetFinishedTraining(false);
        neuralNetwork.ResetNetwork();
    }
    private void InitializeSliders()
    {
        trainingCycleSlider = ui.Q<SliderInt>("TrainingCycleSlider");
        trainingCycleLabel = ui.Q<Label>("TrainingCycleLabel");
        learningRateSlider = ui.Q<Slider>("LearningRateSlider");
        learningRateLabel = ui.Q<Label>("LearningRateLabel");
        trainingCycleLabel.text = trainingCycleSlider.value.ToString();
        learningRateLabel.text = learningRateSlider.value.ToString("F3");

        trainingCycleSlider.RegisterValueChangedCallback(evt =>
        {
            trainingCycleLabel.text = evt.newValue.ToString();
            progressBar.highValue = evt.newValue;
            outerProgressBar.highValue = evt.newValue;

        });

        learningRateSlider.RegisterValueChangedCallback(evt =>
        {
            learningRateLabel.text = evt.newValue.ToString("F3");
        });

        // Tracking
        if (ActivityTracker.Instance != null)
        {
            learningRateSlider.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction("StageOne_LearningRate_Interacted"); });
            trainingCycleSlider.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction("StageOne_TrainingCycle_Interacted"); });
        }
    }
    public void DisableInputLayerButtons()
    {
        inputNodeAddBtn.SetEnabled(false);
        inputNodeRemoveBtn.SetEnabled(false);
    }
    public void EnableInputLayerButtons()
    {
        inputNodeAddBtn.SetEnabled(true);
        inputNodeRemoveBtn.SetEnabled(true);
    }
    public void DisableHiddenLayerButtons()
    {
        hiddenLayerAddBtn.SetEnabled(false);
        hiddenLayerRemoveBtn.SetEnabled(false);

        _hiddenLayerPanel.SetEnabled(false);
    }
    public void EnableHiddenLayerButtons()
    {
        hiddenLayerAddBtn.SetEnabled(true);
        hiddenLayerRemoveBtn.SetEnabled(true);

        _hiddenLayerPanel.SetEnabled(true);
    }
    public void DisableOutputLayerButtons()
    {
        outputNodeAddBtn.SetEnabled(false);
        outputNodeRemoveBtn.SetEnabled(false);
    }
    public void EnableOutputLayerButtons()
    {
        outputNodeAddBtn.SetEnabled(true);
        outputNodeRemoveBtn.SetEnabled(true);
    }
    public void HideNetworkActionPanel()
    {
        networkActionPanel.style.display = DisplayStyle.None;
    }
    public void ShowNetworkActionPanel()
    {
        networkActionPanel.style.display = DisplayStyle.Flex;
    }
    public void HideTrainingCycleForm()
    {
        ui.Q<VisualElement>("TrainingCycleForm").style.display = DisplayStyle.None;
    }
    public void ShowTrainingCycleForm()
    {
        ui.Q<VisualElement>("TrainingCycleForm").style.display = DisplayStyle.Flex;
    }
    public void HideLearningRateForm()
    {
        ui.Q<VisualElement>("LearningRateForm").style.display = DisplayStyle.None;
    }
    public void ShowLearningRateForm()
    {
        ui.Q<VisualElement>("LearningRateForm").style.display = DisplayStyle.Flex;
    }
    public void ShowTestButton()
    {
        ui.Q<Button>("TestNetworkButton").style.display = DisplayStyle.Flex;
    }
    public void HideTestButton()
    {
        ui.Q<Button>("TestNetworkButton").style.display = DisplayStyle.None;
    }
    public void ShowTrainButton()
    {
        ui.Q<Button>("TrainNetworkButton").style.display = DisplayStyle.Flex;
    }
    public void HideTrainButton()
    {
        ui.Q<Button>("TrainNetworkButton").style.display = DisplayStyle.None;
    }

}

class NetworkSolution
{
    public int inputNodes;
    public int outputNodes;
    public int hiddenLayers;
    public int[] hiddenLayerNodes;

    public NetworkSolution(int inputNodes, int outputNodes, int hiddenLayers, int[] hiddenLayerNodes)
    {
        this.inputNodes = inputNodes;
        this.outputNodes = outputNodes;
        this.hiddenLayers = hiddenLayers;
        this.hiddenLayerNodes = hiddenLayerNodes;
    }

    public bool Matches(NeuralNetwork network)
    {
        if (network.inputLayer.nodes.Count != inputNodes) return false;
        if (network.outputLayer.nodes.Count != outputNodes) return false;
        if (network.hiddenLayers.Count != hiddenLayers) return false;
        for (int i = 0; i < hiddenLayers; i++)
        {
            if (network.hiddenLayers[i].nodes.Count != hiddenLayerNodes[i]) return false;
        }
        return true;
    }
}