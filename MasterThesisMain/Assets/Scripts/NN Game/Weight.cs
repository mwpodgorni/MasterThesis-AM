// using Radishmouse;
using UnityEngine;

// [ExecuteInEditMode]
public class Weight : MonoBehaviour
{
    public Node from;
    public Node to;

    public float weight;
    public float gradient;

    // [SerializeField] Mode _mode;

    Parameters _parameters;

    // LineRenderer _line;
    // UILineRenderer _lineUI;

    public Weight(Node fromNode, Node toNode)
    {
        from = fromNode;
        to = toNode;

        _parameters = Resources.Load<Parameters>("ScriptableObjects/Parameters");

        if (_parameters != null)
        {
            weight = Random.Range(_parameters.WeightRange.Item1, _parameters.WeightRange.Item2);
        }
        else
        {
            Debug.LogError("Parameters ScriptableObject not found!");
        }
    }

    // Start is called before the first frame update
    // void Start()
    // {
    //     switch (_mode)
    //     {
    //         case Mode.WorldSpace:
    //             if (!TryGetComponent<LineRenderer>(out _line))
    //             {
    //                 Debug.LogWarning("Missing line rendering component on Weight");
    //                 return;
    //             }
    //             break;
    //         case Mode.UI:
    //             if (!TryGetComponent<UILineRenderer>(out _lineUI))
    //             {
    //                 Debug.LogWarning("Missing line rendering component on Weight");
    //                 return;
    //             }
    //             break;
    //     }

    //     weight = Random.Range(
    //         _parameters.WeightRange.Item1,
    //         _parameters.WeightRange.Item2
    //     );

    //     transform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
    // }

    // private void Update()
    // {
    //     UpdateLine();
    // }

    public float GetWeightSum()
    {
        return weight * from.value;
    }

    void SetLine(Vector3 pos1, Vector3 pos2)
    {
        // _line.SetPositions(new Vector3[2] { pos1, pos2 });
    }

    void SetPosition(Vector3 pos)
    {
        // gameObject.transform.position = pos;
    }

    public void UpdateLine()
    {
        // switch (_mode)
        // {
        //     case Mode.WorldSpace:

        //         SetPosition((from.transform.position + to.transform.position) / 2);
        //         var pos1 = transform.InverseTransformPoint(from.transform.position);
        //         var pos2 = transform.InverseTransformPoint(to.transform.position);
        //         SetLine(pos1, pos2);

        //         break;

        //     case Mode.UI:

        //         _lineUI.points[0] = from.transform.position;
        //         _lineUI.points[1] = to.transform.position;

        //         _lineUI.SetAllDirty();

        //         break;
        // }
    }

    public enum Mode
    {
        WorldSpace,
        UI,
    }
}
