using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Newtonsoft.Json;

public class Layer
{

    public List<Node> nodes = new List<Node>();
    public Layer()
    {
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
        GP.ChannelInstance.Invoke(0);

        Node node = new Node();

        nodes.Add(node);

        GP.ChannelInstance.Invoke(1);
    }

    public void RemoveNode()
    {
        // Debug.Log("LAYER: RemoveNode");
        if (nodes.Count <= 0) return;

        GP.ChannelInstance.Invoke(0);

        var node = nodes[nodes.Count - 1];

        nodes.Remove(node);


        GP.ChannelInstance.Invoke(1);
    }
    public int GetNodeCount()
    {
        return nodes.Count;
    }
}
