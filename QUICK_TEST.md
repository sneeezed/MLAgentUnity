# Quick Test Setup - 2 Minutes

Want to test your AI right now? Here's the fastest way:

## 1. Open Unity
Open your TagAgent scene in the Unity Editor

## 2. Set One Agent to Manual Control
1. Click on **Agent1** in the Hierarchy
2. Find **Behavior Parameters** in the Inspector
3. Change **Behavior Type** from "Default" to **"Heuristic Only"**

That's it! Agent1 is now controlled by you.

## 3. Press Play
- Use **WASD** or **arrow keys** to move
- Press **Space** to jump
- Agent2 will use its trained AI behavior

## Want to Train While Testing?
```bash
# Terminal 1: Activate and start training
source activate_mlagents.sh
mlagents-learn config/tag_training_config.yaml --run-id=HumanTest --resume

# Then press Play in Unity
```

The AI agent continues learning while you test against it!

## Optional: Add Control UI
1. In Hierarchy: Right-click → Create Empty
2. Name it "PlayerControlUI"
3. Add Component → **PlayerControlUI**
4. Drag Agent1 and Agent2's Behavior Parameters to the script fields
5. Press Play - you'll see controls on screen (press H to toggle)

---
For detailed instructions, see [TESTING_GUIDE.md](TESTING_GUIDE.md)

