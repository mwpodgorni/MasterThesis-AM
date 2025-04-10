using UnityEngine;

public class Weight
{
    public Node from;
    public Node to;

    public float weight;
    public float gradient;

    public Weight(Node fromNode, Node toNode)
    {
        from = fromNode;
        to = toNode;

        weight = Random.Range(GP.Instance.WeightRange.Item1, GP.Instance.WeightRange.Item2);
    }

    public float GetWeightSum()
    {
        return weight * from.value;
    }
}
