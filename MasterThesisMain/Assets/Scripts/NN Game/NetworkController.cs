using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class NetworkController : MonoBehaviour
{
    public Layer inputLayer;
    public Layer outputLayer;
    public List<Layer> hiddenLayers;

    public float expectedOutput;
    public float actualOutput;

    public int epoch;
    public float loss;

    [SerializeField] Parameters _parameters;

    // input layer
    public Button inputNodeAddBtn;
    public Button inputNodeRemoveBtn;
    // hidden layers
    public Button hiddenLayerAddBtn;
    public Button hiddenLayerRemoveBtn;
    // output layer
    public Button outputNodeAddBtn;
    public Button outputNodeRemoveBtn;
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
    VisualElement _weightPanel;
    IntegerField _inputField;

    public void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        miniGamePanel = ui.Q<VisualElement>("MiniGamePanel");
        epoch = 0;
    }
    public void OnEnable()
    {
        _layerCount = ui.Q<Label>("LayerCount");
        _expectedText = ui.Q<Label>("ExpectedOutput");
        _actualText = ui.Q<Label>("ActualOutput");
        _lossText = ui.Q<Label>("LossText");
        _hiddenLayerPanel = ui.Q<VisualElement>("HiddenLayerPanel");
        _inputLayerPanel = ui.Q<VisualElement>("InputLayerPanel");
        _outputLayerPanel = ui.Q<VisualElement>("OutputLayerPanel");
        _weightPanel = ui.Q<VisualElement>("WeightPanel");
        _inputField = ui.Q<IntegerField>("EpochInputField");

        // buttons
        inputNodeAddBtn = ui.Q<Button>("InputNodeAddBtn");
        inputNodeAddBtn.clicked += () => AddNode(_inputLayerPanel);
        inputNodeRemoveBtn = ui.Q<Button>("InputNodeRemoveBtn");
        inputNodeRemoveBtn.clicked += () => AddNode(_inputLayerPanel);

        hiddenLayerAddBtn = ui.Q<Button>("HiddenLayerAddBtn");
        hiddenLayerAddBtn.clicked += AddHiddenLayer;
        hiddenLayerRemoveBtn = ui.Q<Button>("HiddenLayerRemoveBtn");
        hiddenLayerRemoveBtn.clicked += RemoveHiddenLayer;

        outputNodeAddBtn = ui.Q<Button>("OutputNodeAddBtn");
        outputNodeAddBtn.clicked += () => AddNode(_outputLayerPanel);
        outputNodeRemoveBtn = ui.Q<Button>("OutputNodeRemoveBtn");
        outputNodeRemoveBtn.clicked += () => AddNode(_outputLayerPanel);

        // _inputField.value = 1;
    }
    public void AddHiddenLayer()
    {
        // if (hiddenLayers.Count >= _parameters.maxLayers) return;

        // RemoveWeights();
        VisualTreeAsset uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/NetworkLayer.uxml");
        VisualElement layer = uiAsset.Instantiate();

        layer.Q<Button>("LayerAddButton").clicked += () => AddNode(layer);
        layer.Q<Button>("LayerRemoveButton").clicked += () => RemoveNode(layer);

        // .Add(layer);
        _hiddenLayerPanel.Q<VisualElement>("HiddenLayers").Add(layer);

        // _layerCount.text = hiddenLayers.Count.ToString();

        // CreateWeights();
    }
    public void RemoveHiddenLayer()
    {
        var hiddenLayersContainer = _hiddenLayerPanel.Q<VisualElement>("HiddenLayers");

        if (hiddenLayersContainer.childCount <= 0) return;

        hiddenLayersContainer.RemoveAt(hiddenLayersContainer.childCount - 1);

    }
    public void AddNode(VisualElement layer)
    {
        Debug.Log("AddNode");

        VisualTreeAsset uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/NetworkNode.uxml");
        VisualElement node = uiAsset.Instantiate();
        layer.Q<VisualElement>("NodeWrapper").Add(node);

    }
    public void RemoveNode(VisualElement layer)
    {
        Debug.Log("RemoveNode");
        var nodeWrapper = layer.Q<VisualElement>("NodeWrapper");
        if (nodeWrapper.childCount <= 0) return;
        nodeWrapper.RemoveAt(nodeWrapper.childCount - 1);

    }

}
