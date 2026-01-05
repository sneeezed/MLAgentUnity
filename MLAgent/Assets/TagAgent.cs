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
    public float moveSpeed = 5f; // Moderate speed when using VelocityChange
    public float maxDistance = 15f; // Distance from center before penalty
    public float tagDistance = 2f; // How close to be tagged
    public int maxStepsPerEpisode = 500;

    [Header("Reward Settings")]
    public float taggerCatchReward = 1.0f;
    public float runnerSurviveReward = 1.0f;
    public float approachReward = 0.01f; // Tagger reward for getting closer
    public float evadeReward = 0.01f; // Runner reward for getting farther
    public float speedRewardMultiplier = 0.0003f;
    public float stuckPenalty = -0.005f;
    public float stuckTimeout = 5f;

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

        // Setup physics
        if (rb != null)
        {
            rb.useGravity = true; // Enable gravity
            rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                            RigidbodyConstraints.FreezeRotationY | 
                            RigidbodyConstraints.FreezeRotationZ; // Freeze rotations but allow Y movement for gravity
            rb.linearDamping = 0.1f;   // Slightly more drag to prevent sliding
            rb.angularDamping = 0.1f;
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

            // Spawn agents on opposite sides
            float spawnDistance = 7f;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            
            transform.localPosition = new Vector3(
                Mathf.Cos(angle) * spawnDistance,
                0.5f,
                Mathf.Sin(angle) * spawnDistance
            );
            
            otherAgent.transform.localPosition = new Vector3(
                Mathf.Cos(angle + Mathf.PI) * spawnDistance,
                0.5f,
                Mathf.Sin(angle + Mathf.PI) * spawnDistance
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
        RequestDecision();
        stepCount++;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Self observations
        sensor.AddObservation(transform.localPosition); // 3
        sensor.AddObservation(rb.linearVelocity); // 3
        sensor.AddObservation(isTagger ? 1f : 0f); // 1 - role

        // Other agent observations
        if (otherAgent != null)
        {
            sensor.AddObservation(otherAgent.transform.localPosition); // 3
            sensor.AddObservation(otherAgent.rb.linearVelocity); // 3
            
            Vector3 directionToOther = (otherAgent.transform.localPosition - transform.localPosition).normalized;
            sensor.AddObservation(directionToOther); // 3
            
            float distanceToOther = Vector3.Distance(transform.localPosition, otherAgent.transform.localPosition);
            sensor.AddObservation(distanceToOther); // 1
        }
        else
        {
            sensor.AddObservation(Vector3.zero); // 3
            sensor.AddObservation(Vector3.zero); // 3
            sensor.AddObservation(Vector3.zero); // 3
            sensor.AddObservation(0f); // 1
        }

        // Obstacle observations
        if (obstacleManager != null && obstacleManager.HasObstacle())
        {
            Vector3 obstaclePos = obstacleManager.GetObstaclePosition();
            Rigidbody obstacleRb = obstacleManager.GetObstacleRigidbody();
            
            sensor.AddObservation(obstaclePos - transform.localPosition); // 3
            sensor.AddObservation(obstacleRb != null ? obstacleRb.linearVelocity : Vector3.zero); // 3
        }
        else
        {
            sensor.AddObservation(Vector3.zero); // 3
            sensor.AddObservation(Vector3.zero); // 3
        }

        // Total observations: 23
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Movement
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        
        Vector3 movement = new Vector3(moveX, 0, moveZ) * moveSpeed;
        // Use VelocityChange so movement is responsive but clamp top speed
        rb.AddForce(movement, ForceMode.VelocityChange);

        // Clamp max speed for stability
        float maxSpeed = 6f;
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
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
                // Reward tagger for getting closer
                if (distanceToOther < previousDistanceToOther)
                {
                    AddReward(approachReward);
                }
            }
            else
            {
                // Reward runner for getting farther
                if (distanceToOther > previousDistanceToOther)
                {
                    AddReward(evadeReward);
                }
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

        // Small speed reward when not stuck
        if (stuckCounter == 0)
        {
            float speed = rb.linearVelocity.magnitude;
            AddReward(speed * speedRewardMultiplier);
        }

        // Small time penalty for efficiency
        AddReward(-0.0005f);

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
            float horizontal = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal = -1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal = 1f;
            
            float vertical = 0f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical = -1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical = 1f;
            
            continuousActions[0] = horizontal;
            continuousActions[1] = vertical;
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
