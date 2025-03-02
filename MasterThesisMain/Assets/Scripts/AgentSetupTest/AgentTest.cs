using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentLeftRight : Agent
{
    public GameObject largeTarget;
    private float moveForce = 0.5f;
    private Rigidbody m_AgentRb;
    private float timeLimit = 20f;
    private float timer = 0f;
    private Vector3 initialPosition;

    public override void Initialize()
    {
        initialPosition = transform.position;
        m_AgentRb = GetComponent<Rigidbody>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 relativePosition = largeTarget.transform.position - transform.position;
        sensor.AddObservation(relativePosition.normalized);
        sensor.AddObservation(m_AgentRb.velocity)X
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;

        switch (act[0])
        {
            case 1:
                dirToGo = transform.forward;
                break;
            case 2:
                dirToGo = -transform.forward;
                break;
            case 3:
                dirToGo = transform.right;
                break;
            case 4:
                dirToGo = -transform.right;
                break;
        }

        m_AgentRb.AddForce(dirToGo * moveForce, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);

        Vector3 relativePosition = largeTarget.transform.position - transform.position;

        // Reward agent based on moving in the correct direction
        float movementAlignment = Vector3.Dot(m_AgentRb.velocity.normalized, relativePosition.normalized);
        AddReward(movementAlignment * 0.1f);  // Positive reward for moving towards target

        // Small time penalty for efficiency
        AddReward(-0.01f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == largeTarget)
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
        transform.position = initialPosition;
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
        transform.position = initialPosition;
        m_AgentRb.velocity = Vector3.zero;
    }
}
