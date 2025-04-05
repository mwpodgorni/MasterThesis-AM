using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
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
    public Button trainBtn;
    // UI elements
    public VisualElement ui;
    public VisualElement miniGamePanel;
    Label _layerCount;
    Label _expectedText;
    Label _actualText;
    Label _lossText;
    VisualElement _hiddenLayerPanel;
    VisualElement _inputLayerPanel;
    VisualElement _outputLayerPanel;
    IntegerField _inputTrainingCycle;
    FloatField _inputLearningRate;

    NeuralNetwork neuralNetwork;
    ConnectionLines _connectionLines;
    public void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        miniGamePanel = ui.Q<VisualElement>("MiniGamePanel");
        neuralNetwork = new NeuralNetwork();
    }
    public void OnEnable()
    {
        _layerCount = ui.Q<Label>("LayerCount");
        _expectedText = ui.Q<Label>("ExpectedOutput");
        _actualText = ui.Q<Label>("ActualOutput");
        _lossText = ui.Q<Label>("LossText");
        _hiddenLayerPanel = ui.Q<VisualElement>("HiddenLayers");
        _inputLayerPanel = ui.Q<VisualElement>("InputLayerPanel");
        _outputLayerPanel = ui.Q<VisualElement>("OutputLayerPanel");

        _inputTrainingCycle = ui.Q<IntegerField>("InputTrainingCycle");
        _inputLearningRate = ui.Q<FloatField>("InputLearningRate");

        // buttons
        trainBtn = ui.Q<Button>("TrainBtn");
        trainBtn.clicked += TrainButtonClicked;
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
        _connectionLines.style.position = Position.Absolute;
        _connectionLines.style.top = 0;
        _connectionLines.style.left = 0;
        _connectionLines.style.right = 0;
        _connectionLines.style.bottom = 0;

        _connectionLines.pickingMode = PickingMode.Ignore;

        ui.Add(_connectionLines);

        // TODO: REMOVE THIS LINE
        StartCoroutine(DelayedSetup());

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
        Debug.Log("AddNode:" + layer.name);
        Debug.Log("addNode2:" + GP.Instance);
        if (layer.childCount >= GP.Instance.maxNodes) return;
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
        var nodeWrapper = layer.Q<VisualElement>("NodeWrapper");
        if (nodeWrapper.childCount <= 0) return;
        nodeWrapper.RemoveAt(nodeWrapper.childCount - 1);
        RedrawConnections();
    }
    public void TrainButtonClicked()
    {
        Debug.Log(_inputTrainingCycle.value);
        Debug.Log(_inputLearningRate.value);
        // neuralNetwork.TrainNetwork(_inputTrainingCycle.value, _inputLearningRate.value);
    }
    public void RedrawConnections()
    {
        _connectionLines.ClearLines();

        var inputNodeWrapper = _inputLayerPanel.Q<VisualElement>("NodeWrapper");
        List<Vector2> inputLayerPositions = new List<Vector2>();
        foreach (var inputNode in inputNodeWrapper.Children())
        {
            Vector2 inputPosition = GetCenterScreenPosition(inputNode);
            inputLayerPositions.Add(inputPosition);
        }

        List<List<Vector2>> hiddenLayerPositions = new List<List<Vector2>>();
        foreach (var hiddenLayer in _hiddenLayerPanel.Children())
        {
            var hiddenNodeWrapper = hiddenLayer.Q<VisualElement>("NodeWrapper");
            List<Vector2> currentLayerPositions = new List<Vector2>();

            foreach (var hiddenNode in hiddenNodeWrapper.Children())
            {
                Vector2 hiddenPosition = GetCenterScreenPosition(hiddenNode);
                currentLayerPositions.Add(hiddenPosition);
                // Debug.Log($"Hidden node position: {hiddenPosition}");
            }

            hiddenLayerPositions.Add(currentLayerPositions);
        }

        if (hiddenLayerPositions.Count > 0)
        {
            foreach (var inputNode in inputLayerPositions)
            {
                foreach (var hiddenNode in hiddenLayerPositions[0])
                {
                    // Debug.Log($"Drawing line from input node {inputNode} to hidden node {hiddenNode}");
                    _connectionLines.AddConnection(inputNode, hiddenNode);
                }
            }
        }

        for (int i = 0; i < hiddenLayerPositions.Count - 1; i++)
        {
            var currentLayer = hiddenLayerPositions[i];
            var nextLayer = hiddenLayerPositions[i + 1];

            foreach (var currentNode in currentLayer)
            {
                foreach (var nextNode in nextLayer)
                {
                    //Debug.Log($"Drawing line from hidden node {currentNode} to hidden node {nextNode}");
                    _connectionLines.AddConnection(currentNode, nextNode);
                }
            }
        }

        if (hiddenLayerPositions.Count > 0)
        {
            var lastHiddenLayer = hiddenLayerPositions[hiddenLayerPositions.Count - 1];
            var outputNodeWrapper = _outputLayerPanel.Q<VisualElement>("NodeWrapper");
            foreach (var lastHiddenNode in lastHiddenLayer)
            {
                foreach (var outputNode in outputNodeWrapper.Children())
                {
                    Vector2 outputPosition = GetCenterScreenPosition(outputNode);
                    // Debug.Log($"Drawing line from hidden node {lastHiddenNode} to output node {outputPosition}");
                    _connectionLines.AddConnection(lastHiddenNode, outputPosition);
                }
            }
        }

        _connectionLines.MarkDirtyRepaint();
    }

    Vector2 GetCenterScreenPosition(VisualElement ve)
    {
        var worldPos = ve.worldBound;
        return worldPos.center;
    }
    IEnumerator DelayedSetup()
    {
        yield return null; // wait one frame
        SetupTestNetwork();
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
}
