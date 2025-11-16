# ML-Agents Setup Guide for Unity 6

## What I've Set Up For You

✅ Created `BlockAgent.cs` - Your C# agent script (in `MLAgent/Assets/`)
✅ Created `block_training_config.yaml` - Training configuration (in `config/`)
✅ Downloaded ML-Agents Release 23 repository (in `ml-agents/`)

## Important: Python Version Issue

ML-Agents Release 23 requires **Python 3.10.1 to 3.10.12** exactly. Your Mac has Python 3.10.16 which is too new.

### Option 1: Use pyenv to install Python 3.10.12 (Recommended)

```bash
# Install pyenv if you don't have it
brew install pyenv

# Install Python 3.10.12
pyenv install 3.10.12

# Create virtual environment with the correct Python version
cd /Users/matiassevak/Desktop/MLAgentProject1
pyenv local 3.10.12
python -m venv mlagents-env
source mlagents-env/bin/activate

# Install ML-Agents
cd ml-agents
pip install -e ./ml-agents-envs
pip install -e ./ml-agents

# Verify installation
mlagents-learn --help
```

### Option 2: Use the Unity Package Manager method (Simpler)

If Python setup is too complex, you can use ML-Agents without Python training initially:

1. **In Unity**, open your project
2. Go to **Window → Package Manager**
3. Click **+** → **Add package from git URL**
4. Enter: `https://github.com/Unity-Technologies/ml-agents.git?path=com.unity.ml-agents#release_23`
5. Click **Add**

This installs ML-Agents in Unity. You can set up Python training later.

---

## Unity Scene Setup

### 1. Create the Training Environment

1. **Create a new scene** or open `SampleScene`
2. **Add a Plane** (Ground):
   - Right-click in Hierarchy → 3D Object → Plane
   - Scale: (2, 1, 2)
   - Position: (0, 0, 0)

3. **Add a Cube** (Your Block Agent):
   - Right-click in Hierarchy → 3D Object → Cube
   - Name it "BlockAgent"
   - Position: (0, 0.5, 0)
   - Scale: (1, 1, 1)

4. **Add Rigidbody to the Cube**:
   - Select BlockAgent
   - Add Component → Rigidbody
   - Uncheck "Use Gravity" (optional, depending on your preference)

5. **Add a Sphere** (Reward/Target):
   - Right-click in Hierarchy → 3D Object → Sphere
   - Name it "Target"
   - Position: (5, 0.5, 5)
   - Scale: (0.5, 0.5, 0.5)
   - Change color: Add a material and make it green

### 2. Configure the Block Agent

1. **Select the BlockAgent cube**
2. **Add the BlockAgent script**:
   - Click "Add Component"
   - Search for "BlockAgent"
   - Add it

3. **Add Behavior Parameters component**:
   - Click "Add Component"
   - Search for "Behavior Parameters"
   - Add it
   - Set these values:
     - **Behavior Name**: `BlockAgent`
     - **Vector Observation → Space Size**: `13`
     - **Actions → Continuous Actions**: `2`
     - **Behavior Type**: `Default` (for training) or `Heuristic Only` (for manual testing)

4. **Link the Target**:
   - In the BlockAgent script component
   - Drag the Target sphere into the "Target" field

### 3. Test Manual Control (Optional)

1. Set **Behavior Type** to **Heuristic Only**
2. Press **Play**
3. Use **WASD** or **Arrow Keys** to move the block
4. Try to reach the green target

---

## Training Your Agent (Once Python is Set Up)

### 1. Start Training

```bash
# Navigate to your project
cd /Users/matiassevak/Desktop/MLAgentProject1

# Activate virtual environment
source mlagents-env/bin/activate

# Start training
mlagents-learn config/block_training_config.yaml --run-id=BlockAgent_Run1
```

### 2. In Unity

1. When you see "**Start training by pressing the Play button in the Unity Editor**"
2. Click the **Play** button in Unity
3. Watch your agent learn!

### 3. Monitor Training

The terminal will show:
- **Step**: Current training step
- **Mean Reward**: Average reward (should increase over time)
- **ELO**: Skill level

Training typically takes **10-30 minutes** depending on your Mac.

### 4. View Training Progress (Optional)

```bash
# In a new terminal
cd /Users/matiassevak/Desktop/MLAgentProject1
source mlagents-env/bin/activate
tensorboard --logdir results
```

Open browser to `http://localhost:6006`

---

## Understanding the Code (You Don't Need to Modify This!)

### BlockAgent.cs - What It Does

The C# script is just a "bridge" between Unity and Python:

1. **CollectObservations**: Tells Python what the agent can "see"
   - Agent position
   - Target position
   - Direction to target
   - Agent velocity

2. **OnActionReceived**: Gets movement commands from Python
   - Python sends: "move X amount left/right, Z amount forward/back"
   - Unity applies the force

3. **Rewards**:
   - **+1.0**: Reaches the target ✅
   - **-1.0**: Falls off the platform ❌
   - **-0.001**: Small penalty per step (encourages efficiency)
   - **+0.01**: Small reward for moving closer to target

### block_training_config.yaml - Training Settings

This is where **all the ML happens** (in Python):
- **Neural network**: 2 layers, 256 neurons each
- **Learning rate**: How fast it learns
- **Batch size**: How many experiences to learn from at once
- **Max steps**: 500,000 training steps

**You can modify these values** to experiment with different training speeds!

---

## Troubleshooting

### "mlagents-learn: command not found"
- Make sure virtual environment is activated: `source mlagents-env/bin/activate`
- Check installation: `pip list | grep mlagents`

### Agent doesn't move during training
- Check that Behavior Type is set to **Default** (not Heuristic Only)
- Make sure you pressed Play in Unity after starting `mlagents-learn`

### Training is very slow
- Close other applications
- Reduce `max_steps` in the config file
- Use Time Scale in Unity (Edit → Project Settings → Time → Time Scale = 20)

### Python version issues
- Use pyenv to install exactly Python 3.10.12
- Or wait for Unity to release ML-Agents compatible with newer Python

---

## Next Steps

1. **Get Python 3.10.12 installed** using pyenv
2. **Set up the Unity scene** following the steps above
3. **Train your first agent!**
4. **Experiment**: Try changing rewards, adding obstacles, or making the target move!

## Resources

- [ML-Agents Documentation](https://unity-technologies.github.io/ml-agents/)
- [ML-Agents GitHub](https://github.com/Unity-Technologies/ml-agents)
- [Training Configuration Reference](https://unity-technologies.github.io/ml-agents/Training-Configuration-File/)

---

**You're all set!** The hard part (installation and code) is done. Now you just need to get the right Python version and start training! 🚀

