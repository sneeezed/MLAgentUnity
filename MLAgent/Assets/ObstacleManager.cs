using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public GameObject obstaclePrefab;
    public Vector3 obstacleSize = new Vector3(2f, 0.5f, 1f); // Rectangular shape
    public float obstacleMass = 1f; // Mass affects how easy it is to push

    [Header("Spawn Settings")]
    public float minDistanceFromTarget = 3f; // Minimum distance from reward
    public float minDistanceFromAgent = 2f; // Minimum distance from agent
    public Vector3 spawnAreaMin = new Vector3(-7f, 0.25f, -7f);
    public Vector3 spawnAreaMax = new Vector3(7f, 0.25f, 7f);
    public int maxSpawnAttempts = 10; // Max attempts to find valid spawn position

    private GameObject currentObstacle;
    private Vector3 obstacleStartPosition;
    private Quaternion obstacleStartRotation;

    public Transform target; // Reference to the reward target
    public Transform agent; // Reference to the agent

    private void Start()
    {
        // If no prefab is assigned, create a default rectangular obstacle
        if (obstaclePrefab == null)
        {
            CreateDefaultObstaclePrefab();
        }
    }

    /// <summary>
    /// Creates a default rectangular obstacle prefab at runtime
    /// </summary>
    private void CreateDefaultObstaclePrefab()
    {
        obstaclePrefab = new GameObject("ObstaclePrefab");

        // Add cube mesh and scale it to be rectangular
        var meshFilter = obstaclePrefab.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateCubeMesh();

        var meshRenderer = obstaclePrefab.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = new Color(0.8f, 0.3f, 0.1f); // Orange color

        // Add physics components
        var rb = obstaclePrefab.AddComponent<Rigidbody>();
        rb.mass = obstacleMass;
        rb.linearDamping = 0.5f; // Some drag to prevent sliding forever
        rb.angularDamping = 0.2f;
        rb.isKinematic = false; // Make sure it's not kinematic
        rb.useGravity = false; // No gravity since we're on a plane
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // Keep upright and on plane

        var boxCollider = obstaclePrefab.AddComponent<BoxCollider>();

        // Scale to rectangular shape
        obstaclePrefab.transform.localScale = obstacleSize;

        // Don't activate the prefab
        obstaclePrefab.SetActive(false);
    }

    /// <summary>
    /// Spawns or respawns the obstacle at a random valid position
    /// </summary>
    public void SpawnObstacle()
    {
        // Remove existing obstacle if any
        if (currentObstacle != null)
        {
            Destroy(currentObstacle);
        }

        // Try to find a valid spawn position
        Vector3 spawnPosition = Vector3.zero;
        bool validPosition = false;
        int attempts = 0;

        while (!validPosition && attempts < maxSpawnAttempts)
        {
            // Generate random position
            spawnPosition = new Vector3(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                Random.Range(spawnAreaMin.z, spawnAreaMax.z)
            );

            // Check if position is valid (far enough from target and agent)
            float distanceToTarget = target != null ? Vector3.Distance(spawnPosition, target.position) : float.MaxValue;
            float distanceToAgent = agent != null ? Vector3.Distance(spawnPosition, agent.position) : float.MaxValue;

            if (distanceToTarget >= minDistanceFromTarget && distanceToAgent >= minDistanceFromAgent)
            {
                validPosition = true;
            }

            attempts++;
        }

        // If we couldn't find a valid position, spawn at a fallback location
        if (!validPosition)
        {
            spawnPosition = new Vector3(5f, 0.25f, 5f); // Fallback position
        }

        // Random rotation (only around Y axis to keep it upright)
        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // Instantiate the obstacle
        currentObstacle = Instantiate(obstaclePrefab, spawnPosition, randomRotation, transform);
        currentObstacle.SetActive(true);
        currentObstacle.name = "Obstacle";

        // Ensure the obstacle has proper physics components
        SetupObstaclePhysics(currentObstacle);

        // Store start position and rotation for potential reset
        obstacleStartPosition = spawnPosition;
        obstacleStartRotation = randomRotation;
    }

    /// <summary>
    /// Resets the obstacle to its spawn position (optional, if you want to reset mid-episode)
    /// </summary>
    public void ResetObstacle()
    {
        if (currentObstacle != null)
        {
            currentObstacle.transform.position = obstacleStartPosition;
            currentObstacle.transform.rotation = obstacleStartRotation;

            // Re-setup physics to ensure it's properly configured
            SetupObstaclePhysics(currentObstacle);
        }
    }

    /// <summary>
    /// Gets the current obstacle's rigidbody (useful for agent observations)
    /// </summary>
    public Rigidbody GetObstacleRigidbody()
    {
        return currentObstacle != null ? currentObstacle.GetComponent<Rigidbody>() : null;
    }

    /// <summary>
    /// Gets the current obstacle's position
    /// </summary>
    public Vector3 GetObstaclePosition()
    {
        return currentObstacle != null ? currentObstacle.transform.position : Vector3.zero;
    }

    /// <summary>
    /// Checks if the obstacle exists
    /// </summary>
    public bool HasObstacle()
    {
        return currentObstacle != null && currentObstacle.activeInHierarchy;
    }

    // Visualize spawn area in Scene view
    private void OnDrawGizmosSelected()
    {
        // Draw spawn area
        Gizmos.color = Color.red;
        Vector3 center = (spawnAreaMin + spawnAreaMax) / 2f;
        Vector3 size = spawnAreaMax - spawnAreaMin;
        Gizmos.DrawWireCube(center, size);

        // Draw minimum distance circles around target and agent
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, minDistanceFromTarget);
        }

        if (agent != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(agent.position, minDistanceFromAgent);
        }
    }

    /// <summary>
    /// Ensures the obstacle has proper physics components and settings
    /// </summary>
    private void SetupObstaclePhysics(GameObject obstacle)
    {
        // Add Rigidbody if not present
        var rb = obstacle.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obstacle.AddComponent<Rigidbody>();
        }

        // Configure Rigidbody for physics interaction
        rb.mass = obstacleMass;
        rb.linearDamping = 0.5f; // Some drag to prevent sliding forever
        rb.angularDamping = 0.2f;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false; // Must be false for physics
        rb.useGravity = false; // No gravity in our 2D plane environment
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // Keep upright and on plane

        // Add BoxCollider if not present
        var collider = obstacle.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = obstacle.AddComponent<BoxCollider>();
        }

        // Ensure collider matches the object size
        collider.size = obstacleSize;
        collider.center = Vector3.zero;

        // Debug log to confirm physics setup
        Debug.Log($"Obstacle physics setup: Mass={rb.mass}, Kinematic={rb.isKinematic}, Gravity={rb.useGravity}, Collider={collider != null}");
    }

    /// <summary>
    /// Helper method to create a basic cube mesh
    /// </summary>
    private Mesh CreateCubeMesh()
    {
        // Use Unity's built-in cube primitive
        GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh cubeMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(tempCube);
        return cubeMesh;
    }
}



