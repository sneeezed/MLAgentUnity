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
    public float tagDistance = 1.5f;
    public int maxStepsPerEpisode = 500;
    
    [Header("Visual Settings")]
    public Color taggerColor = Color.red;
    public Color runnerColor = Color.blue;
    
    private Rigidbody rb;
    private Renderer agentRenderer;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool isTagger;
    private int episodeSteps;
    private bool isGrounded;
    
    // Reference to the other agent's script to coordinate episodes
    private TagAgent otherAgentScript;


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
        if (agentRenderer != null)
        {
            agentRenderer.material.color = isTagger ? taggerColor : runnerColor;
        }
    }

    private void FixedUpdate()
    {
        episodeSteps++;
        
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
            }
            EndEpisode();
            otherAgentScript.EndEpisode();
        }
        
        RequestDecision();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (otherAgent == null) return;
        
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
        sensor.AddObservation(otherAgent.GetComponent<Rigidbody>().linearVelocity);
        
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
        
        // Move using Rigidbody MovePosition (works with physics)
        Vector3 moveDirection = transform.forward * moveForward;
        Vector3 newPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
        
        // Apply rotation
        transform.Rotate(0, rotate * rotationSpeed * Time.fixedDeltaTime, 0);
        
        // Apply jump
        if (jumpAction == 1 && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
        
        // Calculate distance to other agent
        float distanceToOther = Vector3.Distance(transform.localPosition, otherAgent.localPosition);
        
        // Check for tag
        if (distanceToOther < tagDistance)
        {
            if (isTagger)
            {
                // Tagger caught the runner!
                SetReward(1.0f);
                otherAgentScript.SetReward(-1.0f);
                EndEpisode();
                otherAgentScript.EndEpisode();
                return;
            }
        }
        
        // Role-specific rewards
        if (isTagger)
        {
            // Reward for getting closer to runner
            AddReward(-0.001f); // Small penalty per step to encourage quick catches
            
            // Small reward for being close to runner
            float proximityReward = (1.0f - (distanceToOther / maxDistance)) * 0.001f;
            AddReward(proximityReward);
        }
        else
        {
            // Runner: reward for surviving and staying away
            AddReward(0.002f); // Reward per step survived
            
            // Small bonus for maintaining distance
            float distanceReward = (distanceToOther / maxDistance) * 0.001f;
            AddReward(distanceReward);
        }
        
        // Penalty for falling off the map (below ground)
        if (transform.localPosition.y < -2f)
        {
            SetReward(-1.0f);
            otherAgentScript.AddReward(isTagger ? -0.5f : 0.5f); // Other agent gets partial reward/penalty
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

