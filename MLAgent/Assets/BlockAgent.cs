using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BlockAgent : Agent
{
    [Header("References")]
    public Transform target;
    
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float maxDistance = 10f;
    
    private Rigidbody rb;
    private Vector3 startPosition;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        // Reset block position and velocity
        transform.localPosition = startPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        // Randomize target position on the ground plane
        target.localPosition = new Vector3(
            Random.Range(-8f, 8f),
            0.5f,
            Random.Range(-8f, 8f)
        );
    }

    private void FixedUpdate()
    {
        // Request a decision every fixed update
        RequestDecision();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's position relative to starting point (3 values)
        sensor.AddObservation(transform.localPosition - startPosition);
        
        // Target's position relative to starting point (3 values)
        sensor.AddObservation(target.localPosition - startPosition);
        
        // Direction to target (normalized) (3 values)
        Vector3 directionToTarget = (target.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(directionToTarget);
        
        // Distance to target (1 value)
        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);
        sensor.AddObservation(distanceToTarget);
        
        // Agent's velocity (3 values)
        sensor.AddObservation(rb.linearVelocity);
        
        // Total observations: 13
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get movement actions (2 continuous values for X and Z movement)
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        
        // Apply movement force
        Vector3 movement = new Vector3(moveX, 0, moveZ) * moveSpeed;
        rb.AddForce(movement);
        
        // Calculate distance to target
        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);
        
        // Reward for reaching the target
        if (distanceToTarget < 1.5f)
        {
            SetReward(1.0f);
            EndEpisode();
        }
        
        // Small penalty for each step to encourage efficiency
        AddReward(-0.001f);
        
        // Reward for getting closer to target (shaped reward)
        float previousDistance = Vector3.Distance(transform.localPosition - rb.linearVelocity * Time.fixedDeltaTime, target.localPosition);
        if (distanceToTarget < previousDistance)
        {
            AddReward(0.01f);
        }
        
        // Penalty for falling off or going too far
        if (transform.localPosition.y < -1f || distanceToTarget > maxDistance)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Manual control for testing with keyboard
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    // Visualize the agent's path to target in Scene view
    private void OnDrawGizmos()
    {
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target.position, 1.5f);
        }
    }
}

