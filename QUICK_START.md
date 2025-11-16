# 🚀 Quick Start Guide - ML-Agents Training

## ✅ Installation Complete!

Everything is set up and ready to go! Here's what's been installed:

- ✅ Python 3.10.12 (via pyenv)
- ✅ ML-Agents Release 23 (latest version)
- ✅ Virtual environment with all dependencies
- ✅ Unity ML-Agents package (will load when you open Unity)
- ✅ BlockAgent.cs script
- ✅ Training configuration file

---

## 🎮 Step 1: Set Up Unity Scene (5 minutes)

1. **Open your Unity project** (MLAgent folder)

2. **Wait for ML-Agents package to install** (Unity will do this automatically)

3. **Create the training environment:**
   - Create a **Plane** (Ground): Scale (2, 1, 1), Position (0, 0, 0)
   - Create a **Cube** (Agent): Position (0, 0.5, 0)
     - Add **Rigidbody** component
     - Add **BlockAgent** script (it's in Assets/)
     - Add **Behavior Parameters** component:
       - Behavior Name: `BlockAgent`
       - Vector Observation Space Size: `13`
       - Continuous Actions: `2`
   - Create a **Sphere** (Target): Position (5, 0.5, 5), Scale (0.5, 0.5, 0.5)
     - Make it green (create a material)
   - **Drag the Sphere** into the BlockAgent's "Target" field

4. **Test manual control** (optional):
   - Set Behavior Type to "Heuristic Only"
   - Press Play
   - Use arrow keys to move the cube

---

## 🤖 Step 2: Train Your Agent

### Open Terminal and run:

```bash
cd /Users/matiassevak/Desktop/MLAgentProject1
source activate_mlagents.sh
```

This activates your ML-Agents environment.

### Start training:

```bash
mlagents-learn config/block_training_config.yaml --run-id=BlockAgent_Run1
```

### In Unity:

1. Make sure Behavior Type is set to **"Default"** (not Heuristic Only)
2. When you see "**Start training by pressing the Play button**" in terminal
3. **Press Play in Unity**

### Watch it learn! 🎉

The terminal will show:
- **Step**: Current training step
- **Mean Reward**: Should increase over time
- **Time Elapsed**: How long it's been training

Training takes about **15-30 minutes** on your MacBook Air.

---

## 📊 Step 3: Monitor Training (Optional)

Open a **new terminal** and run:

```bash
cd /Users/matiassevak/Desktop/MLAgentProject1
source activate_mlagents.sh
tensorboard --logdir results
```

Open browser to: http://localhost:6006

You'll see graphs of:
- Reward over time
- Episode length
- Learning rate
- And more!

---

## 🎯 What to Expect

### Early Training (Steps 0-50,000):
- Agent moves randomly
- Occasionally reaches target by accident
- Mean Reward: -0.5 to 0.0

### Mid Training (Steps 50,000-200,000):
- Agent starts moving toward target
- Still falls off sometimes
- Mean Reward: 0.0 to 0.5

### Late Training (Steps 200,000-500,000):
- Agent reliably reaches target
- Efficient movement
- Mean Reward: 0.7 to 1.0

---

## 🔧 Common Issues

### "mlagents-learn: command not found"
```bash
source activate_mlagents.sh
```

### Agent doesn't move during training
- Check Behavior Type is "Default" (not Heuristic Only)
- Make sure you pressed Play in Unity after starting mlagents-learn

### Training is slow
- Close other apps
- In Unity: Edit → Project Settings → Time → Time Scale = 10-20

### Want to stop training
- Press `Ctrl+C` in terminal
- Model is auto-saved every 50,000 steps in `results/BlockAgent_Run1/`

---

## 🎓 Next Steps

### Try Different Rewards
Edit `MLAgent/Assets/BlockAgent.cs`:
- Change reward values
- Add penalties for slow movement
- Reward staying close to target

### Modify Training
Edit `config/block_training_config.yaml`:
- `learning_rate`: How fast it learns
- `hidden_units`: Neural network size
- `max_steps`: Total training steps

### Add Challenges
- Multiple targets
- Moving targets
- Obstacles
- Multiple agents competing

---

## 📁 Project Structure

```
MLAgentProject1/
├── activate_mlagents.sh          # Quick activation script
├── config/
│   └── block_training_config.yaml # Training settings
├── ml-agents/                     # ML-Agents source code
├── mlagents-env/                  # Python virtual environment
├── MLAgent/                       # Unity project
│   └── Assets/
│       └── BlockAgent.cs         # Your agent script
└── results/                       # Training results (created when you train)
    └── BlockAgent_Run1/
        ├── *.onnx                # Trained model
        └── events.out.*          # TensorBoard logs
```

---

## 🆘 Need Help?

- **Full Guide**: See `SETUP_GUIDE.md`
- **ML-Agents Docs**: https://unity-technologies.github.io/ml-agents/
- **Configuration Reference**: https://unity-technologies.github.io/ml-agents/Training-Configuration-File/

---

**You're all set! Open Unity, set up your scene, and start training!** 🚀

