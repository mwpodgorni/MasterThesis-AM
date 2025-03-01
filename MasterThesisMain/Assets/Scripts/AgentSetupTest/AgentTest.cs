using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentTest : Agent
{
    public GameObject smallTarget;
    public GameObject largeTarget;
    private Rigidbody m_AgentRb;
    const int k_MinPosition = 0;
    const int k_MaxPosition = 20;
    const float maxDistanceFromLargeTarget = 20f;  // Max distance before reset
    private float initialDistanceToLargeTarget;

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_AgentRb.useGravity = true;
        initialDistanceToLargeTarget = Vector3.Distance(transform.localPosition, largeTarget.transform.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(smallTarget.transform.localPosition);
        sensor.AddObservation(largeTarget.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        Debug.Log("MoveAgent" + act[0]);
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
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        Debug.Log("dirToGo" + dirToGo);
        m_AgentRb.AddForce(dirToGo, ForceMode.VelocityChange);

        // Calculate the distance to the large target
        float currentDistanceToLargeTarget = Vector3.Distance(transform.localPosition, largeTarget.transform.localPosition);
        float distanceReward = Mathf.Clamp01((initialDistanceToLargeTarget - currentDistanceToLargeTarget) / initialDistanceToLargeTarget);

        // Reward the agent for moving closer to the large target
        SetReward(distanceReward);

        // Check if agent reaches the small target
        if (gameObject.transform.localPosition == smallTarget.transform.localPosition)
        {
            SetReward(0.1f);
            EndEpisode();
            ResetAgent();
        }

        // Check if agent reaches the large target
        if (gameObject.transform.localPosition == largeTarget.transform.localPosition)
        {
            SetReward(1f);  // Reward for reaching the large target
            EndEpisode();
            ResetAgent();
        }

        // Check if the agent is too far from the large target
        if (currentDistanceToLargeTarget > maxDistanceFromLargeTarget)
        {
            Debug.Log("Agent is too far from large target, resetting...");
            EndEpisode();
            ResetAgent();
        }
    }

    public override void OnEpisodeBegin()
    {
        ResetAgent();
    }

    void ResetAgent()
    {
        transform.localPosition = new Vector3(0, 0.5f, 0);
        m_AgentRb.velocity = Vector3.zero;
        initialDistanceToLargeTarget = Vector3.Distance(transform.localPosition, largeTarget.transform.localPosition);
    }
}
