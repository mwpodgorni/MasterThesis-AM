using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentLeftRight : Agent
{
    public GameObject smallTarget;
    public GameObject largeTarget;
    private float moveForce = 0.5f;
    Rigidbody m_AgentRb;
    private float timeLimit = 8f;
    private float timer = 0f;

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(smallTarget.transform.position);
        sensor.AddObservation(largeTarget.transform.position);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                dirToGo = transform.right * 1f;
                break;
            case 4:
                dirToGo = transform.right * -1f;
                break;
        }
        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        m_AgentRb.AddForce(dirToGo * moveForce, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        AddReward(-0.01f);
        MoveAgent(actionBuffers.DiscreteActions);
        float distanceToLargeTarget = Vector3.Distance(transform.position, largeTarget.transform.position);
        AddReward(1f / (distanceToLargeTarget + 1f));  // Higher reward the closer to the large target
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == smallTarget)
        {
            AddReward(0.1f);
            EndEpisode();
        }
        else if (other.gameObject == largeTarget)
        {
            AddReward(1f);
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
        transform.position = new Vector3(0f, 1f, 0f);
        m_AgentRb.velocity = Vector3.zero;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= timeLimit)
        {
            ResetAgent();
            EndEpisode();
        }
    }

    private void ResetAgent()
    {
        transform.position = new Vector3(0f, 1f, 0f);
        m_AgentRb.velocity = Vector3.zero;
    }
}
