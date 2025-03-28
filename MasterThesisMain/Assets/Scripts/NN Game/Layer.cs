using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Alexwsu.EventChannels;

public class Layer : MonoBehaviour
{
    public List<Node> nodes;

    [SerializeField] Parameters _parameters;
    [SerializeField] Transform _nodePanel;
    [SerializeField] IntEventChannel _channel;

    void Start()
    {
        nodes.AddRange(GetComponentsInChildren<Node>());
    }

    public void ActivateLayer()
    {
        foreach (Node node in nodes)
        {
            node.Activate();
        }
    }

    public void AddNode()
    {
        if (nodes.Count >= _parameters.maxNodes) return;

        _channel.Invoke(0);

        var nodePrefab = Instantiate(_parameters.nodePrefab);

        nodePrefab.transform.SetParent(_nodePanel);

        if (nodePrefab.TryGetComponent<Node>(out Node node))
        {
            nodes.Add(node);
        }

        _channel.Invoke(1);
    }

    public void RemoveNode()
    {
        if (nodes.Count <= 1) return;

        _channel.Invoke(0);

        var node = nodes[nodes.Count - 1];

        nodes.Remove(node);
        Destroy(node.gameObject);


        _channel.Invoke(1);
    }
}
