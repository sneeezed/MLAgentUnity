# Testing Guide: Human vs AI in Tag Game

This guide explains how to play against your AI agent while it's training or after training is complete.

## Quick Setup

### Option 1: Test During Training (Recommended)

Test your AI in real-time while it continues to learn:

1. **Start Unity and open your TagAgent scene**
   - Open Unity Editor
   - Load the scene with your TagAgent setup

2. **Configure one agent for manual control**
   - In the Hierarchy, select one of your TagAgent GameObjects (e.g., "Agent1")
   - In the Inspector, find the **Behavior Parameters** component
   - Change **Behavior Type** dropdown from "Default" to **"Heuristic Only"**
   - Leave the other agent at "Default" (for AI training)

3. **Add the UI helper (optional but recommended)**
   - In the Hierarchy, right-click and create an empty GameObject
   - Name it "PlayerControlUI"
   - Add Component → Search for "PlayerControlUI" and add it
   - In the Inspector, drag your agents' Behavior Parameters to the UI script:
     - Agent 1 Behavior Params → Drag from Agent1's Behavior Parameters
     - Agent 2 Behavior Params → Drag from Agent2's Behavior Parameters
     - Agent 1 Object → Drag Agent1 GameObject
     - Agent 2 Object → Drag Agent2 GameObject

4. **Start training with manual control**
   ```bash
   # Activate your environment
   source activate_mlagents.sh
   
   # Start training (one agent will be you, one will be AI)
   mlagents-learn config/tag_training_config.yaml --run-id=HumanTest --resume
   ```

5. **Press Play in Unity Editor**
   - The AI agent will train while you control the other agent
   - You'll randomly be assigned tagger or runner each episode
   - Test both roles to see if the environment favors one side

### Option 2: Test After Training (No Training)

Test your trained AI without continuing training:

1. **Set up both agents:**
   - Agent 1: Behavior Type = **"Heuristic Only"** (you control this)
   - Agent 2: Behavior Type = **"Default"** (AI, but won't train)

2. **Just Press Play in Unity** - No Python training needed!
   - The AI will use its learned behavior from the checkpoint
   - You control the other agent
   - No new training happens

## Controls

- **W** or **↑** - Move forward
- **S** or **↓** - Move backward  
- **A** or **←** - Turn left
- **D** or **→** - Turn right
- **Space** - Jump
- **H** - Toggle help UI (if you added PlayerControlUI)

## Understanding the Game

### Roles (Randomly Assigned Each Episode)
- **Tagger (Red)**: Your goal is to tag the runner within the time limit
- **Runner (Blue)**: Your goal is to survive without being tagged

### Win Conditions
- **Tagger wins**: Gets within tag distance (3 units) of runner
  - Agents flash **orange** when tagger wins
- **Runner wins**: Survives for 500 timesteps without being tagged
  - Agents flash **purple** when runner wins
- Falling off the map is an automatic loss

## Testing Scenarios

### Scenario A: You as Tagger vs AI Runner
- Set yourself (e.g., Agent1) to "Heuristic Only"
- When you spawn red, try to catch the blue AI
- Tests: Is the runner AI too good at evading?

### Scenario B: You as Runner vs AI Tagger
- Set yourself to "Heuristic Only"
- When you spawn blue, try to escape the red AI
- Tests: Is the tagger AI too aggressive? Does the environment favor the tagger?

### Tips for Testing
1. **Play multiple episodes** - Roles swap randomly, so you'll get both perspectives
2. **Watch AI behavior** - Notice patterns in how the AI moves
3. **Test environment fairness** - If tagger always wins, the arena might be too small or open
4. **Try different strategies** - Test if the AI adapts to your tactics

## Troubleshooting

### Agent not responding to keyboard
- Check that the agent's Behavior Type is set to "Heuristic Only"
- Make sure the Unity Game window has focus (click on it)

### Both agents are AI
- At least one agent must be set to "Heuristic Only" for manual control
- Check the Behavior Parameters component on each agent

### AI seems random/untrained
- If not training: The AI uses the last checkpoint from your training session
- If the checkpoint is very early (few steps), the AI won't be well-trained yet
- Use a later checkpoint (like TagAgent-2612948.pt from TagTraining7)

### Training not starting
- Make sure your training command points to the right config
- Ensure you're in the correct directory
- Check that `activate_mlagents.sh` ran successfully

## Advanced: Resume Training from Best Checkpoint

If you want to resume training from a specific checkpoint (not just the latest):

```bash
# Copy the desired checkpoint to be the active one
cp results/TagTraining7_FixedRewards/TagAgent/TagAgent-2612948.pt results/TagTraining7_FixedRewards/TagAgent/checkpoint.pt

# Resume training from that point
mlagents-learn config/tag_training_config.yaml --run-id=TagTraining7_FixedRewards --resume
```

## Evaluating Your Test Results

After testing, consider:
- **Tagger win rate**: If tagger wins >70% of the time, environment might favor offense
- **Your experience**: Can you beat the AI consistently in either role?
- **AI behavior quality**: Does the AI make smart decisions or just random movements?
- **Environment design**: Are there enough obstacles for the runner to use tactically?

Based on your testing, you might want to:
- Adjust the arena size or add more cover for runners
- Modify the tag distance in TagAgent.cs
- Adjust the max episode steps (currently 500)
- Change the reward structure in the config or agent code

