using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CollectorAgent : Agent
{
  [Tooltip("How fast the agent can move")]
  public float moveSpeed = 5f;

  [Tooltip("How fast the agent can rotate")]
  public float turnSpeed = 180f;

  private GemArena arena;
  new private Rigidbody rigidbody;

  // Start is called before the first frame update
  void Start()
  {
    rigidbody = GetComponent<Rigidbody>();
    arena = GetComponentInParent<GemArena>();
  }

  public override void OnEpisodeBegin()
  {
    arena.ResetArea();
  }

  public override void CollectObservations(VectorSensor sensor)
  {
    // position in the map
    sensor.AddObservation(transform.position);

    // direction facing
    sensor.AddObservation(transform.forward);
  }

  private void OnCollisionEnter(Collision collision)
  {
    if (collision.transform.CompareTag("gem"))
    {
      arena.RemoveGem(collision.gameObject);
      AddReward(1f);
    }
  }

  public override void OnActionReceived(ActionBuffers actions)
  {
    // Convert the first action to forward movement
    float forwardAmount = actions.DiscreteActions[0];

    // Convert the second action to turning left or right
    float turnAmount = 0f;
    if (actions.DiscreteActions[1] == 1f)
    {
      turnAmount = -1f;
    }
    else if (actions.DiscreteActions[1] == 2f)
    {
      turnAmount = 1f;
    }

    // Apply movement
    rigidbody.MovePosition(transform.position + transform.forward * forwardAmount * moveSpeed * Time.fixedDeltaTime);
    transform.Rotate(transform.up * turnAmount * turnSpeed * Time.fixedDeltaTime);

    // Apply a tiny negative reward every step to encourage action
    if (MaxStep > 0) AddReward(-1f / MaxStep);

    // Fell off platform
    if (this.transform.localPosition.y < 0)
    {
      SetReward(-0.1f);
      EndEpisode();
    }
  }

  public override void Heuristic(in ActionBuffers actionsOut)
  {
    int forwardAction = 0;
    int turnAction = 0;
    if (Input.GetKey(KeyCode.UpArrow))
    {
      // move forward
      forwardAction = 1;
    }
    if (Input.GetKey(KeyCode.LeftArrow))
    {
      // turn left
      turnAction = 1;
    }
    else if (Input.GetKey(KeyCode.RightArrow))
    {
      // turn right
      turnAction = 2;
    }

    // Put the actions into the array
    actionsOut.DiscreteActions.Array[0] = forwardAction;
    actionsOut.DiscreteActions.Array[1] = turnAction;
  }
}
