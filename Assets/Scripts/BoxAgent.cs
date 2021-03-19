using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BoxAgent : Agent
{
    // reference to the roller ball
    Rigidbody rBody;

    // set reference to Rigidbody
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    // reference to the target box
    public Transform TargetTransform;
    public Rigidbody TargetRigidBody;

    // intiialize everything at start of each episode
    public override void OnEpisodeBegin()
    {
        // reset agent to center of floor if it falls
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        // position target randomly on floor
        TargetTransform.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
        TargetRigidBody.velocity = Vector3.zero;
        TargetRigidBody.angularVelocity = Vector3.zero;
    }

    // add inputs to the agent
    public override void CollectObservations(VectorSensor sensor)
    {
        // positions of Target and Agent
        sensor.AddObservation(TargetTransform.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // velocity of Agent
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    // receive input from the Brain and apply to the world, assign rewards
    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, TargetTransform.localPosition);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            SetReward(-0.1f);
            EndEpisode();
        }

        // Target fell off platform
        else if (TargetTransform.localPosition.y < 0)
        {
            EndEpisode();
        }

        // else, incur small penalty so it goes FASTER
        else
        {
            SetReward(-1e-4f);
        }
    }

    // add keyboard controls
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
