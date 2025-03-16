using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Layer : MonoBehaviour
{
    public List<Node> nodes;

    [SerializeField] Parameters _parameters;
    [SerializeField] Transform _nodePanel;

    public void ActivateLayer()
    {
        foreach (Node node in nodes)
        {
            node.Activate();
        }
    }

    public void AddNode()
    {
        var nodePrefab = Instantiate(_parameters.nodePrefab);

        nodePrefab.transform.SetParent(_nodePanel);

        if (nodePrefab.TryGetComponent<Node>(out Node node))
        {
            nodes.Add(node);
        }
    }

    public void RemoveNode()
    {
        var node = nodes[nodes.Count - 1];

        nodes.Remove(node);
        Destroy(node.gameObject);
    }
}
