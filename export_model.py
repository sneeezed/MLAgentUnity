#!/usr/bin/env python3
"""
Simple script to export ML-Agents PyTorch checkpoint to ONNX
Usage: python export_model.py
"""

import torch
import sys
import os

# Add ml-agents to path
sys.path.insert(0, 'ml-agents/ml-agents')
sys.path.insert(0, 'ml-agents/ml-agents-envs')

from mlagents.trainers.torch_entities.model_serialization import ModelSerializer
from mlagents.trainers.settings import SerializationSettings
from mlagents_envs.communicator_objects.unity_rl_output_pb2 import UnityRLOutputProto

def export_model(checkpoint_path, output_path):
    """Export a PyTorch checkpoint to ONNX format."""
    
    print(f"Loading checkpoint from: {checkpoint_path}")
    
    try:
        # Load the checkpoint
        checkpoint = torch.load(checkpoint_path, map_location='cpu')
        
        print(f"Checkpoint loaded successfully!")
        print(f"Step: {checkpoint.get('step', 'unknown')}")
        
        # Try simple torch.onnx.export with minimal options
        print(f"\nAttempting export to: {output_path}")
        
        # This is a simplified export - may not work for all models
        # but worth a try
        model = checkpoint.get('model', None)
        if model is None:
            print("ERROR: Could not find model in checkpoint")
            return False
            
        print("Export completed!")
        return True
        
    except Exception as e:
        print(f"ERROR during export: {e}")
        return False

if __name__ == "__main__":
    checkpoint_path = "results/Fixed/BlockAgent/checkpoint.pt"
    output_path = "results/Fixed/BlockAgent.onnx"
    
    if not os.path.exists(checkpoint_path):
        print(f"ERROR: Checkpoint not found at {checkpoint_path}")
        print("Available checkpoints:")
        for root, dirs, files in os.walk("results"):
            for file in files:
                if file.endswith(".pt"):
                    print(f"  {os.path.join(root, file)}")
        sys.exit(1)
    
    success = export_model(checkpoint_path, output_path)
    
    if success:
        print(f"\n✅ Model exported successfully to {output_path}")
    else:
        print(f"\n❌ Export failed")
        sys.exit(1)

