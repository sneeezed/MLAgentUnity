using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TagAgent : Agent
{
    [Header("Agent References")]
    public Transform otherAgent;
    
    [Header("Settings")]
    public float moveSpeed = 10;
    public float moveAcceleration = 20;
    public float rotationSpeed = 200f;
    public float jumpForce = 80f;
    public float maxDistance = 15f;
    public float tagDistance = 3.0f;
    public int maxStepsPerEpisode = 500;
    
    [Header("Visual Settings")]
    public Color taggerColor = Color.red;
    public Color runnerColor = Color.blue;
    public Color taggerWinColor = new Color(1f, 0.5f, 0f); // Orange
    public Color runnerWinColor = new Color(0.5f, 0f, 1f); // Purple
    
    private Rigidbody rb;
    public Renderer agentRenderer; // Public so other agent can change color
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool isTagger;
    private int episodeSteps;
    private bool isGrounded;
    
    // Reference to the other agent's script to coordinate episodes
    private TagAgent otherAgentScript;
    
    // Track rewards for logging (public so other agent can access)
    public float cumulativeTaggerReward = 0f;
    public float cumulativeRunnerReward = 0f;
    public int taggerEpisodes = 0;
    public int runnerEpisodes = 0;
    private int taggerWins = 0;
    private int runnerWins = 0;
    
    // Track previous distance for movement reward
    private float previousDistance = 0f;
    
    // Win celebration tracking
    private float winCelebrationTimer = 0f;
    private bool isShowingWin = false;


    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        agentRenderer = GetComponent<Renderer>();
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
        
        if (otherAgent != null)
        {
            otherAgentScript = otherAgent.GetComponent<TagAgent>();
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reset position and physics
        transform.localPosition = startPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localRotation = startRotation;
        
        episodeSteps = 0;
        
        // Track which role this episode
        if (isTagger)
        {
            taggerEpisodes++;
        }
        else
        {
            runnerEpisodes++;
        }
        
        // Initialize previous distance
        if (otherAgent != null)
        {
            previousDistance = Vector3.Distance(transform.localPosition, otherAgent.localPosition);
        }
        
        // Randomly assign roles (only one agent decides for both)
        // Use a deterministic check so only one agent assigns roles
        if (transform.GetInstanceID() < otherAgent.GetInstanceID())
        {
            // This agent decides roles for both
            isTagger = Random.value > 0.5f;
            otherAgentScript.SetRole(!isTagger);
        }
        
        // Update visual representation
        UpdateVisuals();
    }
    
    public void SetRole(bool isTaggerRole)
    {
        isTagger = isTaggerRole;
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (agentRenderer != null && !isShowingWin)
        {
            agentRenderer.material.color = isTagger ? taggerColor : runnerColor;
        }
    }
    
    public void ShowWinCelebration(bool taggerWon, float duration = 0.5f)
    {
        if (agentRenderer != null)
        {
            agentRenderer.material.color = taggerWon ? taggerWinColor : runnerWinColor;
            isShowingWin = true;
            winCelebrationTimer = duration;
        }
    }

    private void FixedUpdate()
    {
        episodeSteps++;
        
        // Handle win celebration timer
        if (isShowingWin)
        {
            winCelebrationTimer -= Time.fixedDeltaTime;
            if (winCelebrationTimer <= 0f)
            {
                isShowingWin = false;
                UpdateVisuals(); // Reset to normal role colors
            }
        }
        
        // Check if episode should end due to time limit
        if (episodeSteps >= maxStepsPerEpisode)
        {
            if (isTagger)
            {
                // Tagger failed to catch runner
                SetReward(-0.5f);
            }
            else
            {
                // Runner survived the whole episode!
                AddReward(0.5f);
                runnerWins++;
                
                // Show runner winning in purple
                ShowWinCelebration(false, 0.5f);
            }
            LogEpisodeStats();
            EndEpisode();
            otherAgentScript.EndEpisode();
        }
        
        RequestDecision();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (otherAgent == null) return;
        
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        
        Rigidbody otherRb = otherAgent.GetComponent<Rigidbody>();
        if (otherRb == null) return;
        
        // Role information (1 value)
        sensor.AddObservation(isTagger ? 1f : 0f);
        
        // Grounded state (1 value)
        sensor.AddObservation(isGrounded ? 1f : 0f);
        
        // My position relative to center (3 values)
        sensor.AddObservation(transform.localPosition);
        
        // My velocity (3 values)
        sensor.AddObservation(rb.linearVelocity);
        
        // My forward direction (3 values)
        sensor.AddObservation(transform.forward);
        
        // Other agent's position relative to center (3 values)
        sensor.AddObservation(otherAgent.localPosition);
        
        // Other agent's velocity (3 values)
        sensor.AddObservation(otherRb.linearVelocity);
        
        // Direction to other agent (normalized) (3 values)
        Vector3 directionToOther = (otherAgent.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(directionToOther);
        
        // Distance to other agent (1 value)
        float distanceToOther = Vector3.Distance(transform.localPosition, otherAgent.localPosition);
        sensor.AddObservation(distanceToOther / maxDistance); // Normalize
        
        // Time remaining in episode (normalized) (1 value)
        sensor.AddObservation((float)episodeSteps / maxStepsPerEpisode);
        
        // Total observations: 22
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (otherAgent == null) return;
        
        // Get movement actions
        float moveForward = actions.ContinuousActions[0]; // -1 to 1 (forward/backward)
        float rotate = actions.ContinuousActions[1]; // -1 to 1 (left/right)
        int jumpAction = actions.DiscreteActions[0]; // 0 or 1
        
        // Movement for both agents
        Vector3 moveDirection = transform.forward * moveForward;
        Vector3 newPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
        
        // Apply rotation
        transform.Rotate(0, rotate * rotationSpeed * Time.fixedDeltaTime, 0);
        
        // Apply jump (with penalty to discourage spam)
        if (jumpAction == 1 && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            AddReward(-0.05f); // Penalty for jumping
        }
        
        // Calculate distance to other agent
        float distanceToOther = Vector3.Distance(transform.localPosition, otherAgent.localPosition);
        
        // Reward tagger for getting CLOSER (based on change in distance)
        if (isTagger)
        {
            float distanceChange = previousDistance - distanceToOther;
            if (distanceChange > 0)
            {
                // Moving closer - BIG reward
                AddReward(distanceChange * 0.5f);
            }
            else
            {
                // Moving away - penalty
                AddReward(distanceChange * 0.3f);
            }
            previousDistance = distanceToOther;
        }
        
        // Check for tag
        if (distanceToOther < tagDistance)
        {
            if (isTagger)
            {
                // Tagger caught the runner!
                SetReward(1.0f);
                otherAgentScript.SetReward(-1.0f);
                taggerWins++;
                
                // Show tagger winning in orange
                ShowWinCelebration(true, 0.5f);
                
                LogEpisodeStats();
                EndEpisode();
                otherAgentScript.EndEpisode();
                return;
            }
        }
        
        // Role-specific rewards
        if (isTagger)
        {
            // Small time penalty to encourage speed
            AddReward(-0.001f);
        }
        else
        {
            // Runner: reward for surviving and staying away
            AddReward(0.01f); // Reward per step survived
            
            // Reward runner for staying far from tagger
            if (distanceToOther > tagDistance * 2)
            {
                AddReward(0.01f); // Bonus for maintaining good distance
            }
        }
        
        // Severe penalty for falling off the map (below ground)
        if (transform.localPosition.y < -2f)
        {
            SetReward(-5.0f); // Much harsher penalty for falling
            
            if (isTagger)
            {
                // Tagger fell - runner wins big
                otherAgentScript.AddReward(2.0f);
                runnerWins++;
                
                // Show the winning runner in purple
                otherAgentScript.ShowWinCelebration(false, 0.5f);
            }
            else
            {
                // Runner fell - tagger wins but gets less reward
                otherAgentScript.AddReward(0.5f);
                taggerWins++;
                
                // Show the winning tagger in orange
                otherAgentScript.ShowWinCelebration(true, 0.5f);
            }
            
            LogEpisodeStats();
            EndEpisode();
            otherAgentScript.EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Manual control for testing with keyboard
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;
        
        // W/S or Up/Down arrows for forward/backward
        continuousActions[0] = Input.GetAxis("Vertical");
        
        // A/D or Left/Right arrows for rotation
        continuousActions[1] = Input.GetAxis("Horizontal");
        
        // Space for jump
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
    
    // Check if agent is on the ground
    private void OnCollisionStay(Collision collision)
    {
        // Check if we're touching something below us (ground)
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.7f) // Surface is relatively flat and pointing up
            {
                isGrounded = true;
                return;
            }
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
    
    private void LogEpisodeStats()
    {
        // Track cumulative reward for this episode
        if (isTagger)
        {
            cumulativeTaggerReward += GetCumulativeReward();
        }
        else
        {
            cumulativeRunnerReward += GetCumulativeReward();
        }
        
        // Only log from one agent to avoid duplicate logs
        if (transform.GetInstanceID() < otherAgent.GetInstanceID())
        {
            // Combine stats from both agents
            float totalTaggerReward = cumulativeTaggerReward + otherAgentScript.cumulativeTaggerReward;
            float totalRunnerReward = cumulativeRunnerReward + otherAgentScript.cumulativeRunnerReward;
            int totalTaggerEps = taggerEpisodes + otherAgentScript.taggerEpisodes;
            int totalRunnerEps = runnerEpisodes + otherAgentScript.runnerEpisodes;
            
            float totalEpisodes = taggerWins + runnerWins;
            
            float taggerWinRate = totalEpisodes > 0 ? (taggerWins / totalEpisodes) * 100f : 0f;
            float runnerWinRate = totalEpisodes > 0 ? (runnerWins / totalEpisodes) * 100f : 0f;
            
            float meanTaggerReward = totalTaggerEps > 0 ? totalTaggerReward / totalTaggerEps : 0f;
            float meanRunnerReward = totalRunnerEps > 0 ? totalRunnerReward / totalRunnerEps : 0f;
            
            // Log every 50 episodes to reduce spam
            if (totalEpisodes > 0 && totalEpisodes % 50 == 0)
            {
                print($"[TAG STATS] Ep:{totalEpisodes} | Tagger:{taggerWins}W({taggerWinRate:F0}%)R={meanTaggerReward:F2} | Runner:{runnerWins}W({runnerWinRate:F0}%)R={meanRunnerReward:F2}");
            }
        }
    }

    // Visualize the relationship between agents
    private void OnDrawGizmos()
    {
        if (otherAgent != null)
        {
            // Draw line between agents
            Gizmos.color = isTagger ? Color.red : Color.blue;
            Gizmos.DrawLine(transform.position, otherAgent.position);
            
            // Draw tag range for tagger
            if (isTagger)
            {
                Gizmos.color = new Color(1, 0, 0, 0.3f);
                Gizmos.DrawWireSphere(transform.position, tagDistance);
            }
        }
    }
}

