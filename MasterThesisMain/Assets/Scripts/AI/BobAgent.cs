using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using UnityEngine.UIElements;

public class BobAgent : Agent
{
    [Header("Movement Settings")]
    [SerializeField] Transform target;
    [SerializeField] float _moveSpeed = 2f;
    [SerializeField] float _rotationSpeed = 2f;
    [Header("Episode Settings")]
    [SerializeField] Vector2 _randomPositionSize = Vector2.one;
    [SerializeField] float _maxTime = 60;

    [ObservableProperty] Vector3 _position => transform.position;
    [ObservableProperty] Quaternion _rotation => transform.rotation;
    [ObservableProperty] Vector3 _scale => transform.localScale;

    public List<Observable> _observationSpace;

    float _timer = 0f;
    Rigidbody _rb;

    // Start is called before the first frame update
    public override void Initialize()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (var observation in _observationSpace)
        {
            var processed = ProcessObservation(observation);
            switch (processed.Item2)
            {
                case TypeOfData.Int: sensor.AddObservation((int) processed.Item1); break;
                case TypeOfData.Float: sensor.AddObservation((float) processed.Item1); break;
                case TypeOfData.Bool: sensor.AddObservation((bool) processed.Item1); break;
                case TypeOfData.Vector3: sensor.AddObservation((Vector3) processed.Item1); break;
                case TypeOfData.Vector2: sensor.AddObservation((Vector2) processed.Item1); break;
                case TypeOfData.Quaternion: sensor.AddObservation((Quaternion) processed.Item1); break;
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
    }

    private void Update()
    {
        if (_timer < _maxTime)
        {
            _timer += Time.deltaTime;
        }
        else
        {
            _timer = 0f;
            EndEpisode();
        }
        
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var distance = Vector3.Distance(transform.position, target.position);
        var dirToGo = Vector3.zero;

        var action = act[0];
        switch (action)
        {
            case 0:
                dirToGo = transform.forward * (_moveSpeed * Time.deltaTime);
                break;
            case 1:
                dirToGo = transform.forward * (-_moveSpeed * Time.deltaTime);
                break;
            case 2:
                transform.Rotate(transform.up * _rotationSpeed * Time.deltaTime);
                break;
            case 3:
                transform.Rotate(transform.up * -_rotationSpeed * Time.deltaTime);
                break;
            case 4:
                // Do nothing
                break;
        }

        transform.position += dirToGo;

        Vector3 relativePosition = target.position - transform.position;
        float movementAlignment = Vector3.Dot(dirToGo.normalized, relativePosition.normalized);

        AddReward(movementAlignment * 0.1f);
        AddReward(-0.01f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == target)
        {
            AddReward(1);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.W)) discreteActions[0] = 1;
        else if (Input.GetKey(KeyCode.S)) discreteActions[0] = 2;
        else if (Input.GetKey(KeyCode.D)) discreteActions[0] = 3;
        else if (Input.GetKey(KeyCode.A)) discreteActions[0] = 4;
    }

    public override void OnEpisodeBegin()
    {
        var x = Random.Range(-_randomPositionSize.x, _randomPositionSize.x);
        var z = Random.Range(-_randomPositionSize.y, _randomPositionSize.y);

        var targetX = Random.Range(-_randomPositionSize.x, _randomPositionSize.x);
        var targetZ = Random.Range(-_randomPositionSize.y, _randomPositionSize.y);

        transform.position = new Vector3(x, 1, z);
        target.position = new Vector3(targetX, 1, targetZ);
    }

    (object, TypeOfData) ProcessObservation(object value)
    {
        if (value is float floatVal)
        {
            return (floatVal, TypeOfData.Float);
        }
        else if (value is int intVal)
        {
            return (intVal, TypeOfData.Int);
        }
        else if (value is bool boolVal)
        {
            return (boolVal ? 1 : 0, TypeOfData.Bool);
        }
        else if (value is Vector3 vector3Val)
        {
            return (vector3Val, TypeOfData.Vector3);
        }
        else if (value is Vector2 vector2Val) 
        {
            return (vector2Val, TypeOfData.Vector2);
        }
        else if (value is Quaternion quaternion)
        {
            return (quaternion, TypeOfData.Quaternion);
        }
        else
        {
            Debug.LogWarning($"Unknown type: {value.GetType()}");
            return (null,TypeOfData.None);
        }
    }

    enum TypeOfData
    {
        None,
        Int,
        Float,
        Bool,
        Vector3,
        Vector2,
        Quaternion
    }

}
