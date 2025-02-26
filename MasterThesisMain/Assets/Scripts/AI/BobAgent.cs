using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class BobAgent : Agent
{
    public Transform target;
    
    Rigidbody _rb;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
    }
}
