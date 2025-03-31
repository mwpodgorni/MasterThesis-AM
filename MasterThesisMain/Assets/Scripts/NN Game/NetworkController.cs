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

    // hidden layers
    public Button addLayerButton;
    public Button removeLayerButton;
    // output layer

    // UI elements
    public VisualElement ui;
    public VisualElement miniGamePanel;
    Label _layerCount;
    Label _expectedText;
    Label _actualText;
    Label _lossText;
    VisualElement _hiddenLayerPanel;
    VisualElement _weightPanel;
    IntegerField _inputField;

    public void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        miniGamePanel = ui.Q<VisualElement>("MiniGamePanel");
        epoch = 0;
        // inputLayer = Instantiate(_parameters.layerPrefab).GetComponent<Layer>();
    }
    public void OnEnable()
    {
        _layerCount = ui.Q<Label>("LayerCount");
        _expectedText = ui.Q<Label>("ExpectedOutput");
        _actualText = ui.Q<Label>("ActualOutput");
        _lossText = ui.Q<Label>("LossText");
        _hiddenLayerPanel = ui.Q<VisualElement>("HiddenLayerPanel");
        _weightPanel = ui.Q<VisualElement>("WeightPanel");
        _inputField = ui.Q<IntegerField>("EpochInputField");
        addLayerButton = ui.Q<Button>("AddHiddenLayerBtn");
        addLayerButton.clicked += AddHiddenLayer;

        _inputField.value = 1;
    }
    public void AddHiddenLayer()
    {
        // if (hiddenLayers.Count >= _parameters.maxLayers) return;

        // RemoveWeights();

        VisualTreeAsset uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/NetworkLayer.uxml");
        VisualElement layer = uiAsset.Instantiate();

        // .Add(layer);
        _hiddenLayerPanel.Q<VisualElement>("HiddenLayers").Add(layer);

        // layer.transform.SetParent(_hiddenLayerPanel);
        // _layerCount.text = hiddenLayers.Count.ToString();

        // CreateWeights();
    }
}
