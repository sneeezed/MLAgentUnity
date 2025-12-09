# Obstacle Setup Guide

This guide explains how to add a physics-based obstacle that the agent must learn to navigate around or push to reach the reward.

## What Was Added

1. **ObstacleManager.cs** - Manages obstacle spawning and physics
2. **Updated BlockAgent.cs** - Integrated obstacle system with observations and speed rewards

## Key Features

- **Physics-based obstacle** that can be pushed by the agent
- **Smart spawning** - avoids spawning on the reward or inside the agent
- **Speed rewards** - encourages dynamic movement and obstacle pushing
- **19 observations** - includes obstacle position and velocity data

## Unity Setup Steps

### Step 1: Add ObstacleManager to Scene

1. In Unity, select your training area GameObject (the parent containing your agent and target)
2. Click "Add Component" in the Inspector
3. Search for and add `ObstacleManager`

### Step 2: Configure ObstacleManager

The ObstacleManager has several configurable parameters:

#### Obstacle Settings:
- **Obstacle Prefab**: Leave empty to auto-generate a rectangular obstacle
- **Obstacle Size**: Default (2, 0.5, 1) creates a wide, flat rectangle
  - Adjust X/Z for length/width
  - Adjust Y for height
- **Obstacle Mass**: Default 1kg (reduced for easier pushing)
  - Lower = easier to push
  - Higher = requires more speed/force

#### Spawn Settings:
- **Min Distance From Target**: Default 3 units - obstacle won't spawn too close to reward
- **Min Distance From Agent**: Default 2 units - obstacle won't spawn inside agent
- **Spawn Area Min/Max**: Defines where obstacles can spawn
  - Default: (-7, 0.25, -7) to (7, 0.25, 7)
  - Adjust to fit your arena size
  - Y value should be half the obstacle height above ground

### Step 3: Link ObstacleManager to Agent

1. Select your agent GameObject
2. In the `BlockAgent` component, find the new field: **Obstacle Manager**
3. Drag the GameObject with the ObstacleManager component into this field

### Step 4: Update ML-Agents Configuration

The observation space has increased from **13 to 19 values**. Update your training configuration:

**In your `config/block_training_config.yaml`**, update the vector_observation_space_size:

```yaml
behaviors:
  BlockAgent:
    trainer_type: ppo
    hyperparameters:
      # ... other settings ...
    network_settings:
      # ... other settings ...
      # UPDATE THIS:
      vector_observation_space_size: 19  # was 13, now 19
```

### Step 5: Adjust Speed Reward (Optional)

In the `BlockAgent` component, you can adjust:
- **Speed Reward Multiplier**: Default 0.0005 (reduced to prevent spinning)
  - Higher = more reward for moving fast
  - Lower = less emphasis on speed
- **Stuck Penalty**: Default -0.005 (penalty for not moving)
  - More negative = stronger penalty for being stuck
- **Obstacle Collision Penalty**: Default -0.01 (penalty for hitting obstacles)
  - More negative = stronger penalty for collisions
- **Stuck Timeout**: Default 5 seconds (force episode end when stuck)
  - Lower = episodes end faster when stuck
  - Higher = more patience for stuck agents

### Step 6: Customize Obstacle Appearance (Optional)

To create a custom obstacle prefab:

1. Create a cube: GameObject → 3D Object → Cube
2. Scale it to rectangular shape (e.g., 2, 0.5, 1)
3. Add/modify material for custom color
4. Add Rigidbody component:
   - Mass: 3 (adjust as needed)
   - Drag: 0.5
   - Angular Drag: 0.2
5. Ensure it has a Box Collider
6. Save as prefab
7. Assign to ObstacleManager's "Obstacle Prefab" field
8. Delete the original from the scene

## How It Works

### Physics Interaction
- **Obstacle has Rigidbody + BoxCollider** for realistic physics
- **Agent can push obstacle** when moving with sufficient speed/force
- **Obstacle maintains momentum** and can be pushed around the arena
- **Frozen Y-axis movement** keeps everything on the ground plane
- **Configurable mass** (default 1kg) affects push difficulty

### Learning Behavior
The agent now receives 6 additional observations:
- Obstacle position relative to agent (3 values)
- Obstacle velocity (3 values)

Plus speed rewards and stuck penalties to encourage efficient movement.

This allows the agent to learn to:
- Navigate around the obstacle
- Push the obstacle when needed to clear a path
- Use speed strategically to overcome obstacles
- Plan paths considering obstacle physics
- Avoid getting stuck on walls (episodes auto-restart after 5 seconds)

### Episode Flow
1. Episode begins
2. Agent resets to start position
3. Target spawns at random location
4. **Obstacle spawns at random location (away from target and agent)**
5. Agent tries to reach target (may need to push obstacle)
6. Episode ends when target is reached, agent falls/goes too far, or gets stuck for 5+ seconds

## Training Strategy

Since you added obstacles, the agent needs to learn new behaviors. Consider:

### Start Fresh Training Run
```bash
mlagents-learn config/block_training_config.yaml --run-id=BlockWithObstacle
```

### Resume Existing Training (Not Recommended)
If you resume existing training, the agent will need to adapt its learned behaviors to handle obstacles. This may take longer and could be confusing.

### Training Tips
- **Monitor rewards**: Look for increasing rewards as agent learns obstacle navigation
- **Watch behavior**: Agent should learn to either go around obstacles or push them
- **Adjust difficulty**: If obstacles are too hard/easy to push, adjust mass or agent speed
- **Test manually**: Use "Heuristic Only" mode to test obstacle physics yourself

## Testing

### Manual Control
Test the obstacle physics manually:
1. Set the agent's Behavior Type to "Heuristic Only" in BehaviorParameters
2. Use WASD/Arrow keys to control the agent
3. Try pushing the obstacle at different speeds
4. Test navigation around obstacles

### Training
- The agent will need to learn new behaviors with obstacles
- Training may take longer due to increased complexity
- Monitor reward curves for convergence

## Troubleshooting

**Obstacle spawns on target or agent:**
- Increase `Min Distance From Target` and `Min Distance From Agent`
- Check that ObstacleManager has references to target and agent

**Obstacle too easy/hard to push:**
- Adjust obstacle mass (lower = easier to push)
- Adjust agent `moveSpeed` (higher = stronger pushing)
- Check Unity console for physics setup debug logs
- Ensure both objects have Rigidbodies and BoxColliders
- Try increasing agent mass if it's too light

**Agent gets stuck on walls/obstacles:**
- The stuck detection penalizes this and will force episode restart after 5 seconds
- If episodes end too quickly, increase `stuckTimeout` value
- If episodes don't end when stuck, decrease `stuckTimeout` value
- Make sure obstacle collision penalty is working

**Agent ignores obstacle:**
- Verify ObstacleManager is linked in BlockAgent
- Check that observations are being collected (debug in Heuristic mode)

**Training doesn't start:**
- Update `vector_observation_space_size` to 19 in your config
- Ensure you're using a new run ID

**Agent just spins in circles:**
- Reduce `speedRewardMultiplier` if speed rewards are too dominant
- The agent might be learning that spinning gives speed rewards

## Next Steps

Consider adding:
- Multiple obstacles
- Moving obstacles
- Different obstacle shapes/sizes
- Collision penalties for hitting obstacles
- Time pressure (shorter episodes)
- Obstacle that blocks the target specifically



