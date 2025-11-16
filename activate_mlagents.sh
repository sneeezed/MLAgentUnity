#!/bin/bash
# ML-Agents Environment Activation Script

# Set up pyenv
export PYENV_ROOT="$HOME/.pyenv"
export PATH="$PYENV_ROOT/bin:$PATH"
eval "$(pyenv init -)"

# Navigate to project directory
cd "$(dirname "$0")"

# Activate virtual environment
source mlagents-env/bin/activate

echo "✅ ML-Agents environment activated!"
echo "📍 Project: $(pwd)"
echo "🐍 Python: $(python --version)"
echo ""
echo "Available commands:"
echo "  mlagents-learn config/block_training_config.yaml --run-id=YourRunName"
echo "  tensorboard --logdir results"
echo ""

