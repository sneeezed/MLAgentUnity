using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

public class TagAgent : Agent
{
    [Header("References")]
    public TagAgent otherAgent; // Reference to the other agent
    public ObstacleManager obstacleManager;
    public Transform areaCenter; // Center point of play area

    [Header("Game Settings")]
    public float moveSpeed = 5f; 
    public float turnSpeed = 10f; //watch out if you change friction also change this
    public float jumpForce = 5f; 
    public float maxDistance = 15f; 
    public float tagDistance = 2f; 
    public int maxStepsPerEpisode = 500;
    public float uprightForce = 5f; 

    [Header("Reward Settings")]
    public float taggerCatchReward = 1.0f;
    public float runnerSurviveReward = 1.0f;
    public float approachReward = 2.0f; // 
    public float evadeReward = 2.0f; // 
    public float facingReward = 0.0001f; // prolly just delete this idk
    public float stuckPenalty = -0.005f;
    public float stuckTimeout = 5f; // maybe lower this cuz it makes the runner sad
    [Header("Visual")]
    public Material taggerMaterial; // Red material for tagger
    public Material runnerMaterial; // Blue material for runner

    [Header("Spawner Control")]
    public bool isSpawner = true; // Only one agent should spawn/reset environment

    // State
    private Rigidbody rb;
    private Vector3 startPosition;
    private Renderer agentRenderer;
    private bool isTagger;
    private int stepCount;
    private float previousDistanceToOther;
    private bool isGrounded; // Tracks if agent is on the floor
    
    // Stuck detection
    private Vector3 previousPosition;
    private int stuckCounter;
    private float stuckStartTime;

    public bool IsTagger => isTagger;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        agentRenderer = GetComponent<Renderer>();
        startPosition = transform.localPosition;

        // Setup physics for "Chaos" (Tumbling/Flipping)
        if (rb != null)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None; 
            rb.linearDamping = 0.5f; 
            rb.angularDamping = 1.0f; // Lowered from 2.0 to allow easier rotation
            rb.centerOfMass = new Vector3(0, -0.4f, 0); 
        }

        // Setup obstacle manager (only once, from one agent)
        if (obstacleManager != null && otherAgent != null)
        {
            obstacleManager.target = transform; // Use one agent as reference
            obstacleManager.agent = otherAgent.transform;
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reset agent
        transform.localPosition = startPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        // Reset stuck detection
        previousPosition = startPosition;
        stuckCounter = 0;
        stuckStartTime = -1f;
        stepCount = 0;

        // Only one agent controls the episode setup
        if (otherAgent != null && isSpawner)
        {
            // Randomly assign roles
            bool thisIsTagger = Random.value > 0.5f;
            AssignRole(thisIsTagger);
            otherAgent.AssignRole(!thisIsTagger);

            // Spawn agents on opposite sides (Smaller radius for smaller arena)
            float spawnDistance = 5f; // Reduced from 7f to avoid walls
            float angle = Random.value > 0.5f ? 0f : 180f; // Always spawn on the same line
            float angleRad = angle * Mathf.Deg2Rad;
            
            transform.localPosition = new Vector3(
                Mathf.Cos(angleRad) * spawnDistance,
                0.5f,
                Mathf.Sin(angleRad) * spawnDistance
            );
            
            otherAgent.transform.localPosition = new Vector3(
                Mathf.Cos(angleRad + Mathf.PI) * spawnDistance,
                0.5f,
                Mathf.Sin(angleRad + Mathf.PI) * spawnDistance
            );

            // Spawn obstacle
            if (obstacleManager != null)
            {
                obstacleManager.SpawnObstacle();
                Debug.Log($"[TagAgent] OnEpisodeBegin -> obstacle spawned (agent: {gameObject.name})");
            }
            else
            {
                Debug.LogWarning($"[TagAgent] ObstacleManager not set on {gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"[TagAgent] OnEpisodeBegin skipped spawn (agent: {gameObject.name}, isSpawner={isSpawner}, siblingIndex={transform.GetSiblingIndex()})");
        }

        // Calculate initial distance
        if (otherAgent != null)
        {
            previousDistanceToOther = Vector3.Distance(transform.localPosition, otherAgent.transform.localPosition);
        }
    }

    public void AssignRole(bool tagger)
    {
        isTagger = tagger;
        
        // Set visual appearance
        if (agentRenderer != null)
        {
            if (isTagger && taggerMaterial != null)
            {
                agentRenderer.material = taggerMaterial;
            }
            else if (!isTagger && runnerMaterial != null)
            {
                agentRenderer.material = runnerMaterial;
            }
        }
    }

    private void FixedUpdate()
    {
        // Grounded check using a short raycast down
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.6f);
        
        RequestDecision();
        stepCount++;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Role (1)
        sensor.AddObservation(isTagger ? 1f : 0f);

        // 2. Self observations (Relative to Area Center) (6)
        Vector3 relativePosToCenter = transform.localPosition - areaCenter.localPosition;
        sensor.AddObservation(relativePosToCenter / maxDistance); // Normalized position
        sensor.AddObservation(rb.linearVelocity / moveSpeed); // Normalized velocity

        // 3. Orientation & Spin (7)
        sensor.AddObservation(transform.up); // 3
        sensor.AddObservation(transform.forward); // 3
        sensor.AddObservation(rb.angularVelocity.y / 10f); // 1 - Spin magnitude around UP

        // 4. Grounded state (1)
        sensor.AddObservation(isGrounded ? 1f : 0f);

        // 5. Other agent observations (Relative) (7)
        if (otherAgent != null)
        {
            Vector3 relativePosToOther = otherAgent.transform.localPosition - transform.localPosition;
            sensor.AddObservation(relativePosToOther / (maxDistance * 2f)); // Normalized relative pos
            
            Vector3 relativeVelToOther = otherAgent.rb.linearVelocity - rb.linearVelocity;
            sensor.AddObservation(relativeVelToOther / moveSpeed); // Normalized relative vel
            
            float distanceToOther = Vector3.Distance(transform.localPosition, otherAgent.transform.localPosition);
            sensor.AddObservation(distanceToOther / (maxDistance * 2f)); // Normalized distance
        }
        else
        {
            sensor.AddObservation(Vector3.zero); // 3
            sensor.AddObservation(Vector3.zero); // 3
            sensor.AddObservation(0f); // 1
        }

        // 6. Obstacle observations (Relative) (6)
        if (obstacleManager != null && obstacleManager.HasObstacle())
        {
            Vector3 obstaclePos = obstacleManager.GetObstaclePosition();
            Vector3 relativePosToObstacle = obstaclePos - transform.localPosition;
            sensor.AddObservation(relativePosToObstacle / maxDistance); // 3
            
            Rigidbody obstacleRb = obstacleManager.GetObstacleRigidbody();
            Vector3 relativeVelToObstacle = (obstacleRb != null ? obstacleRb.linearVelocity : Vector3.zero) - rb.linearVelocity;
            sensor.AddObservation(relativeVelToObstacle / moveSpeed); // 3
        }
        else
        {
            sensor.AddObservation(Vector3.zero); // 3
            sensor.AddObservation(Vector3.zero); // 3
        }

        // Total observations: 28 (cleaned up and normalized)
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // 1. ROTATION (Action 1)
        float rotateInput = actions.ContinuousActions[1];
        // Use VelocityChange for rotation so it's snappy regardless of friction
        rb.AddTorque(Vector3.up * rotateInput * turnSpeed, ForceMode.VelocityChange);

        // 2. FORWARD MOVEMENT (Action 0)
        float moveInput = actions.ContinuousActions[0];
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 movement = flatForward * moveInput * moveSpeed * 10f; 
        rb.AddForce(movement, ForceMode.Force);

        // 3. SOFT KEEP UPRIGHT (Uses torque instead of hard MoveRotation)
        if (isGrounded)
        {
            // Calculate how much we are tilted
            Vector3 predictedUp = Quaternion.AngleAxis(
                rb.angularVelocity.magnitude * Mathf.Rad2Deg * 0.1f, 
                rb.angularVelocity
            ) * transform.up;

            Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
            rb.AddTorque(torqueVector * uprightForce * 10f, ForceMode.Acceleration);
        }

        // 4. JUMP (Action 2)
        if (actions.ContinuousActions[2] > 0.5f && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }

        // Clamp only HORIZONTAL speed (X and Z) so jumping (Y) isn't affected
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float maxSpeed = 6f;
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            Vector3 clampedVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(clampedVelocity.x, rb.linearVelocity.y, clampedVelocity.z);
        }

        // Check if out of bounds
        float distanceFromCenter = Vector3.Distance(transform.localPosition, areaCenter.localPosition);
        if (transform.localPosition.y < -1f || distanceFromCenter > maxDistance)
        {
            SetReward(-1.0f);
            EndEpisode();
            if (otherAgent != null) otherAgent.EndEpisode();
            return;
        }

        // Check for tag
        if (otherAgent != null)
        {
            float distanceToOther = Vector3.Distance(transform.localPosition, otherAgent.transform.localPosition);
            
            if (distanceToOther < tagDistance)
            {
                // Tag happened!
                if (isTagger)
                {
                    SetReward(taggerCatchReward);
                    otherAgent.SetReward(-runnerSurviveReward);
                    ScoreDisplay.TaggerWon(); // Update score display
                }
                else
                {
                    SetReward(-runnerSurviveReward);
                    otherAgent.SetReward(taggerCatchReward);
                    ScoreDisplay.TaggerWon(); // Update score display
                }
                EndEpisode();
                otherAgent.EndEpisode();
                return;
            }

            // Shaped rewards based on role
            if (isTagger)
            {
                // Potential-based reward: reward for progress towards/away
                float distanceChange = previousDistanceToOther - distanceToOther;
                AddReward(distanceChange * approachReward);

                // Reward for facing the runner
                Vector3 dirToOther = (otherAgent.transform.localPosition - transform.localPosition).normalized;
                float alignment = Vector3.Dot(transform.forward, dirToOther);
                AddReward(alignment * facingReward);
            }
            else
            {
                // Potential-based reward: reward for progress towards/away
                float distanceChange = distanceToOther - previousDistanceToOther;
                AddReward(distanceChange * evadeReward);

                // Reward for facing AWAY from the tagger
                Vector3 dirAwayFromOther = (transform.localPosition - otherAgent.transform.localPosition).normalized;
                float alignment = Vector3.Dot(transform.forward, dirAwayFromOther);
                AddReward(alignment * facingReward);
            }

            previousDistanceToOther = distanceToOther;
        }

        // Check if max steps reached (runner wins)
        if (stepCount >= maxStepsPerEpisode)
        {
            if (isTagger)
            {
                SetReward(-taggerCatchReward); // Tagger failed
            }
            else
            {
                SetReward(runnerSurviveReward); // Runner survived!
            }
            ScoreDisplay.RunnerWon(); // Update score display
            EndEpisode();
            if (otherAgent != null) otherAgent.EndEpisode();
            return;
        }

        // Stuck detection
        float movementDistance = Vector3.Distance(transform.localPosition, previousPosition);
        if (movementDistance < 0.01f)
        {
            stuckCounter++;
            if (stuckCounter > 10)
            {
                AddReward(stuckPenalty);
                
                if (stuckStartTime < 0f)
                {
                    stuckStartTime = Time.time;
                }
                else if (Time.time - stuckStartTime > stuckTimeout)
                {
                    SetReward(-1.0f);
                    EndEpisode();
                    if (otherAgent != null) otherAgent.EndEpisode();
                    return;
                }
            }
        }
        else
        {
            stuckCounter = 0;
            stuckStartTime = -1f;
        }

        previousPosition = transform.localPosition;

        // Dynamic Time Penalty: Only penalize if they aren't making significant distance progress
        // This stops the constant "drain" when they are actually doing their job
        float speed = rb.linearVelocity.magnitude;
        if (speed < 1.0f) 
        {
            AddReward(-0.0005f);
        }

        // Update score display with current rewards (only the spawner does this)
        if (isSpawner && otherAgent != null)
        {
            float taggerRew = isTagger ? GetCumulativeReward() : otherAgent.GetCumulativeReward();
            float runnerRew = isTagger ? otherAgent.GetCumulativeReward() : GetCumulativeReward();
            ScoreDisplay.UpdateRewards(taggerRew, runnerRew);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            // Action 0: W/S for Forward/Backward
            float forward = 0f;
            if (keyboard.wKey.isPressed) forward = 1f;
            if (keyboard.sKey.isPressed) forward = -1f;
            
            // Action 1: A/D for Left/Right Rotation
            float rotate = 0f;
            if (keyboard.aKey.isPressed) rotate = -1f;
            if (keyboard.dKey.isPressed) rotate = 1f;
            
            continuousActions[0] = forward;
            continuousActions[1] = rotate;

            // Jump with Space
            continuousActions[2] = keyboard.spaceKey.isPressed ? 1f : 0f;
        }
    }

    private void OnDrawGizmos()
    {
        if (otherAgent != null)
        {
            // Draw line to other agent
            Gizmos.color = isTagger ? Color.red : Color.blue;
            Gizmos.DrawLine(transform.position, otherAgent.transform.position);
            
            // Draw tag radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, tagDistance);
        }
    }
}
