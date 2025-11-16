# ML-Agents Block Character Training

A Unity ML-Agents project where AI agents learn to navigate to a target using reinforcement learning.

![Training Demo](https://img.shields.io/badge/Unity-6-blue) ![ML--Agents](https://img.shields.io/badge/ML--Agents-Release%2023-green) ![Python](https://img.shields.io/badge/Python-3.10.12-yellow)

## 🎯 Project Overview

This project demonstrates reinforcement learning using Unity ML-Agents. Block characters learn to:
- Navigate to a randomly placed target
- Avoid falling off the platform
- Optimize movement efficiency

**Training Results:**
- Training time: ~4 minutes (470K steps, 6 parallel agents)
- Final mean reward: 2.18
- Success rate: >95%

## 🚀 Quick Start

### Prerequisites
- Unity 6 (6000.0 or later)
- Python 3.10.1 - 3.10.12
- macOS, Windows, or Linux

### Installation

1. **Clone this repository**
   ```bash
   git clone <your-repo-url>
   cd MLAgentProject1
   ```

2. **Install Python 3.10.12 (using pyenv)**
   ```bash
   brew install pyenv
   pyenv install 3.10.12
   pyenv local 3.10.12
   ```

3. **Clone ML-Agents**
   ```bash
   git clone --branch release_23 --depth 1 https://github.com/Unity-Technologies/ml-agents.git
   ```

4. **Set up Python environment**
   ```bash
   python -m venv mlagents-env
   source mlagents-env/bin/activate  # On Windows: mlagents-env\Scripts\activate
   cd ml-agents
   pip install -e ./ml-agents-envs
   pip install -e ./ml-agents
   ```

5. **Open Unity project**
   - Open Unity Hub
   - Add project from `MLAgent/` folder
   - Let Unity import packages (may take a few minutes)

### Training Your Agent

1. **Set up the scene** (see [QUICK_START.md](QUICK_START.md) for details)
   - Create training areas with cube agents and target spheres
   - Configure Behavior Parameters

2. **Start training**
   ```bash
   source activate_mlagents.sh
   mlagents-learn config/block_training_config.yaml --run-id=MyTraining
   ```

3. **Press Play in Unity**

4. **Watch your agents learn!**

## 📁 Project Structure

```
MLAgentProject1/
├── config/
│   └── block_training_config.yaml    # Training hyperparameters
├── MLAgent/                           # Unity project
│   ├── Assets/
│   │   └── BlockAgent.cs             # Agent behavior script
│   ├── Packages/
│   │   └── manifest.json             # Unity package dependencies
│   └── ProjectSettings/              # Unity project settings
├── activate_mlagents.sh              # Quick environment activation
├── QUICK_START.md                    # Detailed setup guide
└── SETUP_GUIDE.md                    # Technical documentation
```

## 🎓 How It Works

### Agent Observations (13 values)
- Agent position (3)
- Target position (3)
- Direction to target (3)
- Distance to target (1)
- Agent velocity (3)

### Actions (2 continuous)
- Move X (left/right)
- Move Z (forward/back)

### Rewards
- **+1.0**: Reaches target
- **-1.0**: Falls off platform
- **-0.001**: Small penalty per step (encourages efficiency)
- **+0.01**: Gets closer to target

### Training Algorithm
- **PPO** (Proximal Policy Optimization)
- 2-layer neural network with 256 hidden units
- Batch size: 128
- Learning rate: 0.0003

## 🎮 Features

- ✅ Parallel training (multiple agents simultaneously)
- ✅ Configurable hyperparameters
- ✅ TensorBoard integration for monitoring
- ✅ Checkpoint saving/resuming
- ✅ Manual control mode for testing

## 📊 Training Tips

**For faster training:**
- Use 8-16 parallel agents
- Set Time Scale to 10-20 (Edit → Project Settings → Time)
- Enable "Run In Background" in Player Settings

**Typical training time:**
- Single agent: ~30 minutes
- 8 parallel agents: ~5 minutes
- 16 parallel agents: ~3 minutes

## 🔧 Configuration

Edit `config/block_training_config.yaml` to adjust:
- Learning rate
- Neural network size
- Training duration
- Reward parameters

## 📈 Monitoring Training

View training progress with TensorBoard:
```bash
tensorboard --logdir results
```

Open http://localhost:6006 in your browser.

## 🎬 Recording

Use Unity Recorder to capture training:
1. Window → Package Manager → Install "Unity Recorder"
2. Window → General → Recorder → Recorder Window
3. Add Movie recorder
4. Start recording and press Play

## 🤝 Contributing

Feel free to:
- Add new reward functions
- Experiment with different neural network architectures
- Create more complex environments
- Share your training results!

## 📚 Resources

- [ML-Agents Documentation](https://unity-technologies.github.io/ml-agents/)
- [ML-Agents GitHub](https://github.com/Unity-Technologies/ml-agents)
- [Training Configuration Reference](https://unity-technologies.github.io/ml-agents/Training-Configuration-File/)

## 📝 License

This project is open source and available under the MIT License.

## 🙏 Acknowledgments

- Unity ML-Agents Team
- Unity Technologies

---

**Made with Unity ML-Agents** 🤖
