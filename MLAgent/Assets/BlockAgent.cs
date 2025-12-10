using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

public class BlockAgent : Agent
{
    [Header("References")]
    public Transform target;
    public ObstacleManager obstacleManager;

    [Header("Settings")]
    public float moveSpeed = 8f; // Increased for better obstacle pushing
    public float maxDistance = 10f;
    public float speedRewardMultiplier = 0.0005f; // Reduced to prevent spinning
    public float stuckPenalty = -0.005f; // Penalty for being stuck/not moving
    public float obstacleCollisionPenalty = -0.01f; // Penalty for hitting obstacles
    public float stuckTimeout = 5f; // Seconds before forcing episode end when stuck

    private Rigidbody rb;
    private Vector3 startPosition;
    private Vector3 previousPosition; // Track previous position to detect being stuck
    private int stuckCounter; // Count how many frames agent hasn't moved
    private float stuckStartTime; // Time when agent first got stuck

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.localPosition;

        // Ensure agent has proper physics for obstacle interaction
        if (rb != null)
        {
            rb.useGravity = false; // No gravity in plane environment
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ; // Keep on plane and facing forward
            rb.linearDamping = 0.1f; // Some drag for realistic movement
            rb.angularDamping = 0.1f;
            Debug.Log($"Agent physics setup: Mass={rb.mass}, Kinematic={rb.isKinematic}, Gravity={rb.useGravity}");
        }

        // Set up obstacle manager references
        if (obstacleManager != null)
        {
            obstacleManager.target = target;
            obstacleManager.agent = transform;
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reset block position and velocity
        transform.localPosition = startPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;

        // Initialize stuck detection
        previousPosition = startPosition;
        stuckCounter = 0;
        stuckStartTime = -1f; // Reset stuck timer

        // Randomize target position on the ground plane
        target.localPosition = new Vector3(
            Random.Range(-8f, 8f),
            0.5f,
            Random.Range(-8f, 8f)
        );

        // Spawn obstacle at random position (away from target and agent)
        if (obstacleManager != null)
        {
            obstacleManager.SpawnObstacle();
        }
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

        // Obstacle observations
        if (obstacleManager != null && obstacleManager.HasObstacle())
        {
            Vector3 obstaclePos = obstacleManager.GetObstaclePosition();
            Rigidbody obstacleRb = obstacleManager.GetObstacleRigidbody();

            // Obstacle position relative to agent (3 values)
            sensor.AddObservation(obstaclePos - transform.localPosition);

            // Obstacle velocity (3 values)
            if (obstacleRb != null)
            {
                sensor.AddObservation(obstacleRb.linearVelocity);
            }
            else
            {
                sensor.AddObservation(Vector3.zero);
            }
        }
        else
        {
            // If no obstacle, add zeros
            sensor.AddObservation(Vector3.zero); // position
            sensor.AddObservation(Vector3.zero); // velocity
        }

        // Total observations: 19 (13 + 6 obstacle observations)
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

        // Check if agent is stuck (not moving)
        float movementDistance = Vector3.Distance(transform.localPosition, previousPosition);
        if (movementDistance < 0.01f) // Very little movement
        {
            stuckCounter++;
            if (stuckCounter > 10) // Been stuck for multiple frames
            {
                AddReward(stuckPenalty); // Penalty for being stuck

                // Start stuck timer if not already started
                if (stuckStartTime < 0f)
                {
                    stuckStartTime = Time.time;
                }
                // Check if stuck for too long
                else if (Time.time - stuckStartTime > stuckTimeout)
                {
                    // Force episode end - agent is stuck on invisible walls
                    SetReward(-1.0f);
                    EndEpisode();
                    return; // Exit early to prevent further processing
                }
            }
        }
        else
        {
            stuckCounter = 0; // Reset counter if moving
            stuckStartTime = -1f; // Reset stuck timer
        }

        // Update previous position
        previousPosition = transform.localPosition;

        // Reward for moving fast (but not when stuck)
        float speed = rb.linearVelocity.magnitude;
        if (stuckCounter == 0) // Only reward speed when not stuck
        {
            AddReward(speed * speedRewardMultiplier);
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
        // Manual control for testing with keyboard (New Input System)
        var continuousActions = actionsOut.ContinuousActions;

        // Get keyboard input
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            // Horizontal movement (A/D or Left/Right arrows)
            float horizontal = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal = -1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal = 1f;

            // Vertical movement (W/S or Up/Down arrows)
            float vertical = 0f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical = -1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical = 1f;

            continuousActions[0] = horizontal;
            continuousActions[1] = vertical;
        }
        else
        {
            continuousActions[0] = 0f;
            continuousActions[1] = 0f;
        }
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

    // Detect collisions with obstacles
    private void OnCollisionEnter(Collision collision)
    {
        // Check if we collided with an obstacle
        if (collision.gameObject.name == "Obstacle")
        {
            // Small penalty for hitting obstacles
            AddReward(obstacleCollisionPenalty);
        }
    }
}
