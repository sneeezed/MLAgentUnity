# Tag Game - Quick Start

Converting your reward-seeking agent into a competitive tag game!

## ✅ What I've Done

### 1. Created `TagAgent.cs`
A new agent script designed for competitive tag gameplay:
- Supports both Tagger and Runner roles
- Role assignment randomized each episode
- Observations track the other agent (not a static target)
- Role-specific rewards (catching vs evading)
- Visual feedback (red for tagger, blue for runner)
- Keeps all your obstacle and stuck detection features

### 2. Created `tag_game_config.yaml`
Training configuration optimized for tag games:
- **Self-play enabled** - agents learn by competing against themselves
- Larger batch sizes for competitive learning
- 2M max steps for complex strategy development

### 3. Created `TAG_GAME_SETUP.md`
Complete step-by-step guide with:
- Unity scene setup instructions
- Material creation
- Agent linking process
- Training commands
- Troubleshooting tips

---

## 🎯 Key Concept: Tag Game vs Reward Seeking

| Aspect | Old Setup | New Setup |
|--------|-----------|-----------|
| **Agents** | 1 agent | 2 agents (same script) |
| **Goal** | Reach static target | Catch opponent / Evade opponent |
| **Roles** | None | Tagger (Red) or Runner (Blue) |
| **Learning** | Standard RL | Self-play (competitive) |
| **Observations** | 19 (target position) | 23 (opponent position) |
| **Win Condition** | Reach target | Tag within distance / Survive time |

---

## 🚀 Quick Setup (5 Minutes)

### In Unity:

1. **Duplicate your agent** (Ctrl+D) → Now you have 2 agents

2. **On BOTH agents**:
   - Remove `BlockAgent` component
   - Add `TagAgent` component
   - Change `Behavior Name` to `TagAgent`

3. **Create Materials**:
   - Right-click → Create → Material → Name: `TaggerMaterial` → Color: Red
   - Right-click → Create → Material → Name: `RunnerMaterial` → Color: Blue

4. **Link everything on BOTH agents**:
   - **Other Agent**: Drag the OTHER agent here
   - **Obstacle Manager**: Drag your ObstacleManager
   - **Area Center**: Create empty GameObject at (0,0,0) and drag here
   - **Tagger Material**: Drag TaggerMaterial
   - **Runner Material**: Drag RunnerMaterial

5. **Test it**:
   - Set one agent to `Heuristic Only` in Behavior Parameters
   - Press Play
   - Use WASD - verify colors change and tagging works

6. **Start Training**:
   ```bash
   source activate_mlagents.sh
   mlagents-learn config/tag_game_config.yaml --run-id=TagGame_SelfPlay
   ```

---

## 🎮 How It Works

### Episode Flow:
1. **Episode starts** → Both agents spawn on opposite sides
2. **Roles assigned randomly** → One becomes red (tagger), one becomes blue (runner)
3. **Obstacle spawns at CENTER (0, 0, 0)** of the arena
4. **Tagger chases runner** / **Runner evades tagger**
5. **Episode ends when**:
   - Tagger catches runner (within 2 units) → Tagger wins
   - 500 timesteps pass → Runner wins
   - Agent goes out of bounds → Both lose

### Rewards:
- **Tagger**:
  - +1.0 for catching runner
  - +0.01 for getting closer
  - Small penalties for time/being stuck
  
- **Runner**:
  - +1.0 for surviving full episode
  - +0.01 for getting farther
  - Small penalties for time/being stuck

### Self-Play Learning:
- Both agents use the same neural network
- They learn by playing against past versions of themselves
- Creates an "arms race" of strategies
- Produces much more interesting behavior than standard training

---

## 📊 Expected Training Timeline

### 0-100k steps (~30 min):
- Random movement
- Accidental tags
- Lots of out-of-bounds

### 100k-500k steps (~2-3 hours):
- Basic chasing/evading
- Some obstacle awareness
- Longer episodes

### 500k-1M steps (~5-8 hours):
- Strategic movement
- Obstacle usage
- Prediction and counter-prediction
- Interesting emergent behavior!

### 1M+ steps (overnight):
- Advanced strategies
- Complex mind games
- Very efficient movement
- Beautiful to watch!

*Times assume single training environment. Use multiple environments for faster training.*

---

## 🎯 Next Steps

1. **Follow the detailed guide**: `TAG_GAME_SETUP.md`
2. **Test manually first** to verify everything works
3. **Start training** with self-play config
4. **Monitor in TensorBoard**: `tensorboard --logdir results`
5. **Be patient** - competitive games take longer to train but are worth it!

---

## 💡 Pro Tips

1. **Start with manual testing** - verify the tag game works before training
2. **Use multiple environments** - duplicate your entire setup 4-8 times for 4-8x faster training
3. **Increase time scale** - Edit → Project Settings → Time → Time Scale = 2 or 3
4. **Watch TensorBoard** - Episode length should increase as agents get better
5. **Self-play takes time** - But produces much more interesting results than standard training!

---

## 📁 Files Reference

- **`MLAgent/Assets/TagAgent.cs`** - Main agent script (replaces BlockAgent)
- **`config/tag_game_config.yaml`** - Training configuration with self-play
- **`TAG_GAME_SETUP.md`** - Detailed setup guide
- **`MLAgent/Assets/ObstacleManager.cs`** - Your existing obstacle system (unchanged)

---

## 🐛 Common Issues

**"Other Agent is null"**
→ Make sure both agents have TagAgent component and are linked to each other

**"Behavior not found"**
→ Change Behavior Name in Behavior Parameters to "TagAgent"

**Agents don't change colors**
→ Verify materials are created and assigned in TagAgent component

**Training is slow**
→ Create multiple training environments (duplicate entire setup)

---

Need more details? Check `TAG_GAME_SETUP.md` for the complete guide! 🎮
