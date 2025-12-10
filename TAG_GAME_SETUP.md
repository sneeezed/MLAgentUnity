# Tag Game Setup Guide

Complete guide to convert your reward-seeking agent into a competitive tag game with obstacles!

## 🎮 Game Overview

**Tag Game Mechanics:**
- **Two agents** learning through self-play
- **Randomly assigned roles** each episode:
  - **Tagger (Red)**: Must catch the runner within time limit
  - **Runner (Blue)**: Must evade tagger for full episode
- **Obstacles**: Physics-based rectangular obstacles for strategic play
- **Win Conditions**:
  - Tagger wins: Gets within 2 units of runner
  - Runner wins: Survives 500 timesteps

---

## 🛠️ Unity Setup Steps

### Step 1: Create Tag Game Scene

1. **Duplicate your existing scene**:
   - Right-click on your current scene → Duplicate
   - Rename to `TagGameScene`

2. **Delete the old target/reward object** (green sphere or whatever you had)

### Step 2: Setup Two Agents

1. **Duplicate your agent GameObject**:
   - Select your existing agent
   - Ctrl+D (Cmd+D on Mac) to duplicate
   - You should now have two agents in the scene

2. **Position them apart** (they'll be repositioned at runtime anyway):
   - Agent1: Position (-5, 0.5, 0)
   - Agent2: Position (5, 0.5, 0)

3. **Rename them**:
   - Agent1 → "TaggerRunner1"
   - Agent2 → "TaggerRunner2"

### Step 3: Replace BlockAgent with TagAgent

For **BOTH agents**:

1. **Remove BlockAgent component**:
   - Select agent → Inspector → Find `BlockAgent` script
   - Click the 3 dots (⋮) → Remove Component

2. **Add TagAgent component**:
   - Click "Add Component"
   - Search for "TagAgent"
   - Add it

3. **Update Behavior Parameters**:
   - Find `Behavior Parameters` component
   - Change `Behavior Name` to: **`TagAgent`**
   - Keep `Vector Observation` → `Space Size`: **23** (will update automatically)
   - Keep `Continuous Actions` → **2**

### Step 4: Create Materials for Visual Distinction

**Don't know how to create materials?** → See `UNITY_MATERIALS_GUIDE.md` for detailed step-by-step instructions!

1. **Create Tagger Material (Red)**:
   - Right-click in Assets → Create → Material
   - Name: `TaggerMaterial`
   - Click the white box next to "Albedo"
   - Set color to **Red** (R:255, G:0, B:0)

2. **Create Runner Material (Blue)**:
   - Right-click in Assets → Create → Material
   - Name: `RunnerMaterial`
   - Click the white box next to "Albedo"
   - Set color to **Blue** (R:0, G:0, B:255)

### Step 5: Link Everything Together

**First, create Area Center marker:**
1. Right-click in Hierarchy → **Create Empty**
2. Name it: `AreaCenter`
3. Set Position to: (0, 0, 0)

**For Agent 1 (TaggerRunner1):**

In Inspector, find `TagAgent` component and fill in:
   - **Other Agent**: Drag `TaggerRunner2` here
   - **Obstacle Manager**: Drag your ObstacleManager GameObject here
   - **Area Center**: Drag the `AreaCenter` GameObject you just created
   - **Tagger Material**: Drag `TaggerMaterial` from Project panel
   - **Runner Material**: Drag `RunnerMaterial` from Project panel

**For Agent 2 (TaggerRunner2):**

Same as Agent 1, but:
   - **Other Agent**: Drag `TaggerRunner1` here
   - (Everything else same as Agent 1)

### Step 6: Setup ObstacleManager (if not already done)

1. **Find or create ObstacleManager GameObject**:
   - If you don't have one: Create Empty GameObject → Add `ObstacleManager` component
   - Position at (0, 0, 0)

2. **Configure ObstacleManager**:
   - **Obstacle Mass**: 1 (light enough to push)
   - **Note**: Obstacle will always spawn at the **center (0, 0, 0)** between the two agents

3. **The TagAgent script will auto-link** the obstacle manager references

### Step 7: Setup Play Area

1. **Create or ensure you have**:
   - Ground plane (large enough, e.g., 20x20)
   - Optional: Walls around the perimeter
   - Area Center marker (empty GameObject at 0,0,0)

2. **Adjust `maxDistance` in TagAgent** if needed:
   - Select both agents
   - In Inspector → `TagAgent` → `Max Distance`: 15 (or adjust based on your arena size)

### Step 8: Configure Training

You have two options:

#### Option A: Self-Play (Recommended for Tag Games)

The config file `config/tag_game_config.yaml` is already set up with self-play!

**What is self-play?**
- Agents learn by playing against increasingly skilled versions of themselves
- Much better for competitive games like tag
- Creates more robust strategies

#### Option B: Standard Multi-Agent (Simpler but less effective)

Both agents learn simultaneously but not through self-play competition.

### Step 9: Test Manually First

Before training, test that everything works:

1. **Set one agent to Heuristic mode**:
   - Select TaggerRunner1
   - `Behavior Parameters` → `Behavior Type` → **Heuristic Only**

2. **Press Play** in Unity

3. **Use WASD** to control TaggerRunner1

4. **Verify**:
   - ✅ Agents spawn on opposite sides
   - ✅ One agent is red, one is blue
   - ✅ Obstacle spawns between them
   - ✅ You can push the obstacle
   - ✅ Getting close to other agent ends episode
   - ✅ Episode ends after ~20-30 seconds if no tag

5. **Set back to Default** when done testing

---

## 🚀 Training Commands

### Start Training (Self-Play)

```bash
# Activate environment
source activate_mlagents.sh

# Start training with self-play
mlagents-learn config/tag_game_config.yaml --run-id=TagGame_SelfPlay

# Press Play in Unity when prompted
```

### Resume Training

```bash
source activate_mlagents.sh
mlagents-learn config/tag_game_config.yaml --run-id=TagGame_SelfPlay --resume
```

### Monitor Training

```bash
# In a new terminal
source activate_mlagents.sh
tensorboard --logdir results
```

Then open: http://localhost:6006

---

## 📊 What to Expect During Training

### Early Training (0-100k steps):
- Agents move randomly
- Lots of episodes ending from going out of bounds
- Tagger rarely catches runner (or catches by accident)

### Mid Training (100k-500k steps):
- Tagger learns to chase runner
- Runner learns basic evasion
- Episodes last longer
- Some strategic use of obstacles

### Advanced Training (500k-1M+ steps):
- Complex strategies emerge
- Runner uses obstacles to hide/block
- Tagger learns to cut off runner
- Mind games and predictions
- Beautiful emergent behavior!

### Key Metrics to Watch:
- **Episode Length**: Should increase as runner gets better
- **Cumulative Reward**: Both agents should improve together
- **ELO Ratings** (in self-play): Shows relative skill progression

---

## ⚙️ Configuration Tuning

### If Tagger Wins Too Often:
- Increase `maxStepsPerEpisode` in TagAgent.cs (give runner more time)
- Increase `runnerSurviveReward` (make surviving more valuable)
- Decrease `tagDistance` (make tagging harder)

### If Runner Wins Too Often:
- Decrease `maxStepsPerEpisode`
- Increase `taggerCatchReward`
- Increase `tagDistance` (easier to tag)
- Increase agent `moveSpeed` (faster game)

### If Training is Too Slow:
- Increase `time_scale` in Unity (Edit → Project Settings → Time → Time Scale = 2 or 3)
- Create multiple training environments (duplicate the whole setup)

### If Agents Get Stuck Often:
- Decrease `stuckTimeout` (faster reset)
- Increase `stuckPenalty` (stronger discouragement)
- Check for invisible colliders in your scene

---

## 🎯 Advanced: Multiple Training Environments

For faster training, create multiple arenas:

1. **Create a parent GameObject**: "TrainingArea"
2. **Put everything inside it** (both agents, obstacle manager, ground, walls)
3. **Duplicate the entire TrainingArea** 4-8 times
4. **Space them apart** (e.g., 30 units apart)
5. **All agents will train in parallel!**

---

## 🐛 Troubleshooting

**Agents don't change colors:**
- Check that materials are assigned in TagAgent component
- Verify agents have a Renderer component

**"Other Agent" is null error:**
- Make sure each agent has reference to the other agent
- Check that both agents have TagAgent component

**Episodes end immediately:**
- Check `maxDistance` setting - might be too small
- Verify agents aren't spawning outside the play area

**Observation space mismatch:**
- Should be 23 observations
- Check in Behavior Parameters component
- ML-Agents should auto-detect, but you can manually set it

**Obstacle doesn't spawn:**
- Verify ObstacleManager is linked in TagAgent
- Check ObstacleManager settings (min distances, spawn area)

**Self-play not working:**
- Check config file has `self_play:` section
- Requires ML-Agents version that supports self-play
- Can fallback to standard training if needed

---

## 📝 Key Differences from Old Setup

| Feature | Old (Reward Seeking) | New (Tag Game) |
|---------|---------------------|----------------|
| **Agents** | 1 agent | 2 agents |
| **Target** | Static reward object | Other agent (dynamic) |
| **Observations** | 19 | 23 |
| **Script** | BlockAgent.cs | TagAgent.cs |
| **Training** | Standard RL | Self-play RL |
| **Win Condition** | Reach target | Catch/evade opponent |
| **Episode Length** | Variable | Max 500 steps |

---

## 🎓 Learning Objectives

The agents should learn to:

### Tagger Skills:
- ✅ Chase runner efficiently
- ✅ Predict runner's movement
- ✅ Cut off escape routes
- ✅ Use obstacles strategically to corner runner
- ✅ Optimize speed vs. precision

### Runner Skills:
- ✅ Evade tagger movement
- ✅ Maintain distance
- ✅ Use obstacles as shields
- ✅ Change direction unpredictably
- ✅ Maximize survival time

---

## 📈 Next Steps After Training

1. **Test the trained agents**:
   - Load checkpoint in Unity
   - Set to Inference mode
   - Watch them play!

2. **Play against the AI**:
   - Set your agent to Heuristic Only
   - Use WASD to control
   - Try to catch (or evade) the trained AI

3. **Iterate on design**:
   - Add more obstacles
   - Change arena shape
   - Adjust tag distance
   - Add power-ups
   - Create tournament brackets

---

## 🎮 Summary Checklist

- [ ] Created two agents in scene
- [ ] Added TagAgent component to both
- [ ] Linked agents to each other
- [ ] Created red and blue materials
- [ ] Assigned materials to TagAgent
- [ ] Setup ObstacleManager
- [ ] Linked ObstacleManager to agents
- [ ] Set Behavior Name to "TagAgent"
- [ ] Tested manually with Heuristic mode
- [ ] Started training with self-play config
- [ ] Monitoring progress in TensorBoard

---

Good luck with your tag game training! The agents should develop fascinating strategies over time. Self-play training typically takes longer but produces much more interesting results! 🎯🤖
