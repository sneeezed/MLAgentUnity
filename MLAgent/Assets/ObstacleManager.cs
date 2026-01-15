//alot of this code was taken from obsticall manager tutorial on yotube

using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public GameObject existingObstacle; // Drag your existing obstacle here!
    public GameObject obstaclePrefab; // Only used if no existing obstacle
    public Vector3 obstacleSize = new Vector3(2f, 0.5f, 1f); // Rectangular shape
    public float obstacleMass = 100f; // Mass increased for ramp stability

    [Header("Spawn Settings")]
    public Vector3 centerSpawnPosition = new Vector3(0f, 0.2f, 0f); // Center spawn position
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

        // Add mesh and scale it
        var meshFilter = obstaclePrefab.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateRampMesh(); // Use the new Ramp Mesh

        var meshRenderer = obstaclePrefab.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = new Color(0.8f, 0.3f, 0.1f); // color

        // Add physics components
        var rb = obstaclePrefab.AddComponent<Rigidbody>();
        rb.mass = obstacleMass;
        rb.linearDamping = 0.1f; // Slight drag might change
        rb.angularDamping = 0.1f;
        rb.isKinematic = false; 
        rb.useGravity = true; 
        
        // Use MeshCollider for complex shapes like ramps
        var meshCollider = obstaclePrefab.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.mesh;
        meshCollider.convex = true;

        // Scale to rectangular shape
        obstaclePrefab.transform.localScale = obstacleSize;

        // Don't activate the prefab
        obstaclePrefab.SetActive(false);
    }

    /// <summary>
    /// Creates a triangular prism mesh (Ramp)
    /// </summary>
    private Mesh CreateRampMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "RampMesh";

        // this is stuff for ramps idk how it works if sormthing breaks its here prolly
        // This creates a ramp where the slope goes from Z=0.5 to Z=-0.5
        Vector3[] vertices = new Vector3[]
        {
            // Bottom square
            new Vector3(-0.5f, -0.5f, -0.5f), // 0
            new Vector3(0.5f, -0.5f, -0.5f),  // 1
            new Vector3(0.5f, -0.5f, 0.5f),   // 2
            new Vector3(-0.5f, -0.5f, 0.5f),  // 3
            
            // Top edge (the "peak" of the ramp - aligned with the back)
            new Vector3(-0.5f, 0.5f, 0.5f),   // 4
            new Vector3(0.5f, 0.5f, 0.5f)     // 5
        };

        // def triangles
        int[] triangles = new int[]
        {
            // Bottom
            0, 1, 2, 0, 2, 3,
            // Back face (Square)
            2, 5, 4, 2, 4, 3,
            // Slope (The actual ramp)
            0, 4, 5, 0, 5, 1,
            // Left side (Triangle)
            0, 3, 4,
            // Right side (Triangle)
            1, 5, 2
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// Spawns or respawns the obstacle at the center of the arena
    /// </summary>
    public void SpawnObstacle()
    {
        Debug.Log("=== SpawnObstacle called ===");
        
        // Calculate spawn position relative to this transform (local to the arena)
        Vector3 spawnPosition = transform.TransformPoint(centerSpawnPosition);
        
        // Check if user assigned an existing obstacle
        if (existingObstacle != null)
        {
            Debug.Log($"✅ Using existing obstacle: {existingObstacle.name}");
            
            // Use the existing obstacle - just reset its position
            currentObstacle = existingObstacle;
            
            // Reset position to center of arena
            Debug.Log($"Moving obstacle from {currentObstacle.transform.position} to {spawnPosition}");
            currentObstacle.transform.position = spawnPosition;
            
            // Random rotation (only around Y axis to keep it upright)
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            currentObstacle.transform.rotation = randomRotation;
            
            // Reset physics
            Rigidbody rb = currentObstacle.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Debug.Log(" velocities reset to zero");
            }
            else
            {
                Debug.LogWarning(" Obstacle has no Rigidbody!");
            }
            
            // Make sure it's active
            currentObstacle.SetActive(true);
            
            // Ensure physics is set up properly
            SetupObstaclePhysics(currentObstacle);
            
            Debug.Log($"Existing obstacle '{existingObstacle.name}' reset to center {centerSpawnPosition}");
        }
        else
        {
            Debug.LogWarning(" No existing obstacle linked! Trying to create dynamic obstacle...");
            
            // No existing obstacle - create a new one dynamically
            
            // Remove old dynamic obstacle if any
            if (currentObstacle != null && currentObstacle != existingObstacle)
            {
                Destroy(currentObstacle);
            }

            // Check if we have a prefab
            if (obstaclePrefab == null)
            {
                Debug.LogError("No obstacle prefab and no existing obstacle! Can't spawn obstacle!");
                Debug.LogError("FIX: Drag your obstacle from the scene to the 'Existing Obstacle' field in ObstacleManager!");
                return;
            }

            // Always spawn at center of arena (already calculated relative to transform)
            
            // Random rotation (only around Y axis to keep it upright)
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            // Instantiate the obstacle
            currentObstacle = Instantiate(obstaclePrefab, spawnPosition, randomRotation, transform);
            currentObstacle.SetActive(true);
            currentObstacle.name = "Obstacle";

            // Ensure the obstacle has proper physics components
            SetupObstaclePhysics(currentObstacle);

            Debug.Log($"New obstacle spawned at relative center {spawnPosition}");
        }

        // Store start position and rotation for potential reset
        obstacleStartPosition = spawnPosition;
        obstacleStartRotation = currentObstacle.transform.rotation;
    }

    /// <summary>
    /// Resets the obstacle to its spawn position (optional, if you want to reset mid-episode)
    /// </summary>
    public void ResetObstacle()
    {
        if (currentObstacle != null)
        {
            currentObstacle.transform.position = transform.TransformPoint(centerSpawnPosition);
            currentObstacle.transform.rotation = obstacleStartRotation;

            // Reset physics velocities
            Rigidbody rb = currentObstacle.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

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
        rb.linearDamping = 0.1f; // Slight drag
        rb.angularDamping = 0.1f;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false; // Must be false for physics
        rb.useGravity = true; // Enable gravity so it falls naturally to ground
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // Keep upright, but allow Y movement for gravity

        // Use MeshCollider for the ramp
        var meshCollider = obstacle.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            // Remove BoxCollider if it exists
            var boxCollider = obstacle.GetComponent<BoxCollider>();
            if (boxCollider != null) DestroyImmediate(boxCollider);

            meshCollider = obstacle.AddComponent<MeshCollider>();
            var meshFilter = obstacle.GetComponent<MeshFilter>();
            if (meshFilter != null) meshCollider.sharedMesh = meshFilter.sharedMesh;
        }
        meshCollider.convex = true;

        // Debug log to confirm physics setup
        Debug.Log($"Obstacle physics setup: Mass={rb.mass}, Kinematic={rb.isKinematic}, Gravity={rb.useGravity}, Collider={meshCollider != null}");
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



