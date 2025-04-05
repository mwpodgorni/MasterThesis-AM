// using Radishmouse;
using UnityEngine;

// [ExecuteInEditMode]
public class Weight : MonoBehaviour
{
    public Node from;
    public Node to;

    public float weight;
    public float gradient;

    [SerializeField] Mode _mode;

    Parameters _parameters;


    public Weight(Node fromNode, Node toNode)
    {
        from = fromNode;
        to = toNode;


        if (_parameters != null)
        {
            weight = Random.Range(GP.Instance.WeightRange.Item1, _parameters.WeightRange.Item2);
        }
        else
        {
            Debug.LogError("Parameters ScriptableObject not found!");
        }
    }

    void Start()
    {
        weight = Random.Range(
            _parameters.WeightRange.Item1,
            _parameters.WeightRange.Item2
        );
    }


    public float GetWeightSum()
    {
        return weight * from.value;
    }


    public enum Mode
    {
        WorldSpace,
        UI,
    }
}
