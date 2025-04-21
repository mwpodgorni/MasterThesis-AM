using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;
public class NetworkController : MonoBehaviour
{
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
    NetworkSolution minigame2Solution = new NetworkSolution(3, 3, 2, new int[] { 4, 4 });

    public void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        miniGamePanel = ui.Q<VisualElement>("MiniGamePanel");
        neuralNetwork = new NeuralNetwork(this);
    }
    public void OnEnable()
    {
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

        StartCoroutine(DelayedSetup());
    }
    public void UpdateEvaluationData(EvaluationData data)
    {
        StageOneController.Instance.EvaluationController().UpdateEvaluationData(data);
        progressBar.value = neuralNetwork.GetCurrentEpoch();
    }
    public void UpdateTrainingCompleted()
    {

        StageOneController.Instance.SetFinishedTraining(true);
        progressBar.value = 0;
    }
    public void AddHiddenLayer()
    {
        if (_hiddenLayerPanel.childCount >= GP.Instance.maxLayers) return;
        VisualTreeAsset uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/NetworkLayer.uxml");
        VisualElement layer = uiAsset.Instantiate();

        layer.Q<Button>("LayerAddButton").clicked += () => AddNode(layer);
        layer.Q<Button>("LayerRemoveButton").clicked += () => RemoveNode(layer);

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
        hiddenLayersContainer.RegisterCallback<GeometryChangedEvent>((evt) => RedrawConnections());
    }
    public void AddNode(VisualElement layer)
    {
        // Debug.Log("ADding node to 1:" + GP.Instance.maxNodes);
        // Debug.Log("ADding node to 2:" + layer.childCount);
        var childCount = layer.Q<VisualElement>("NodeWrapper").childCount;
        // Debug.Log("ADding node to 3:" + childCount);
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

        VisualTreeAsset uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/NetworkNode.uxml");
        VisualElement node = uiAsset.Instantiate();
        node.style.width = 90;
        node.style.height = 90;
        layer.Q<VisualElement>("NodeWrapper").Add(node);
        node.RegisterCallback<GeometryChangedEvent>((evt) => RedrawConnections());

    }
    public void RemoveNode(VisualElement layer)
    {
        Debug.Log($"removeing node from {layer.name}");
        var nodeWrapper = layer.Q<VisualElement>("NodeWrapper");
        if (nodeWrapper.childCount <= 0) return;
        nodeWrapper.RemoveAt(nodeWrapper.childCount - 1);
        if (layer.name == "InputLayerPanel")
        {
            Debug.Log("removing input node");
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
        Debug.Log("TestNetworkButton clicked" + StateManager.Instance.CurrentStage);
        if (StateManager.Instance.CurrentStage == GameStage.FirstHelpOpen)
        {
            if (neuralNetwork.IsNetworkValid())
            {

                StateManager.Instance.SetState(GameStage.FirstNetworkValidated);
            }
            else
            {
                Debug.Log("Network is not valid. Please add nodes to the network.");
                StageOneController.Instance.TutorialController().HideNextButton();
                StageOneController.Instance.TutorialController().SetTypeText(false);
                StageOneController.Instance.TutorialController().SetDisplayTime(5f);
                StageOneController.Instance.TutorialController().SetTutorialSteps(DataReader.Instance.FirstNetworkNotValid());
                StageOneController.Instance.TutorialController().StartTutorial();
            }
        }
        else if (StateManager.Instance.CurrentStage == GameStage.FirstNetworkValidated)
        {
            Debug.Log("TestNetworkButton clicked2");
            if (minigame2Solution.Matches(neuralNetwork))
            {
                Debug.Log("TestNetworkButton clicked3");
                StateManager.Instance.SetState(GameStage.SecondNetworkValidated);
            }
            else
            {
                Debug.Log("Network is not valid. Please add nodes to the network.");
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
            StateManager.Instance.SetState(GameStage.SecondNetworkTrained);
        }
        else
        {
            Debug.Log("Network is not valid. Please add nodes to the network.");
        }
    }
    public void ClearLines()
    {
        _connectionLines.ClearLines();

    }
    public void RedrawConnections()
    {
        _connectionLines.ClearLines();

        // 1) Get the same parent that contains _connectionLines
        var miniGamePanel = ui.Q<VisualElement>("WorkshopPanel");
        if (miniGamePanel == null)
        {
            Debug.LogError("WorkshopPanel not found!");
            return;
        }

        // 2) Convert node positions to local coords
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
        Debug.Log("SetupTestNetwork");
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

        });

        learningRateSlider.RegisterValueChangedCallback(evt =>
        {
            learningRateLabel.text = evt.newValue.ToString("F3");
        });
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
    }
    public void EnableHiddenLayerButtons()
    {
        hiddenLayerAddBtn.SetEnabled(true);
        hiddenLayerRemoveBtn.SetEnabled(true);
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