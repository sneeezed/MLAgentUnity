# Obstacle Reset Fix Guide

How to make your existing obstacle reset properly each episode!

---

## 🎯 The Problem

You had an obstacle in your scene, but it wasn't resetting because the `ObstacleManager` was trying to create NEW obstacles instead of resetting YOUR existing one.

---

## ✅ The Solution

I've updated `ObstacleManager.cs` to support **TWO modes**:

### **Mode 1: Use Existing Obstacle** (Recommended - What You Want!)
- Use the obstacle already in your scene
- Just resets its position/rotation each episode
- No duplicates created

### **Mode 2: Dynamic Obstacle Creation**
- Creates new obstacles each episode
- Only if you don't have an existing one

---

## 🛠️ Unity Setup (2 Minutes)

### **Step 1: Make Sure Your Obstacle Has Physics**

1. Select your obstacle in the Hierarchy
2. In Inspector, check it has:
   - ✅ **Rigidbody** component
     - Mass: 1
     - Use Gravity: ❌ OFF
     - Is Kinematic: ❌ OFF
   - ✅ **Box Collider** component

If missing, add them:
- Add Component → **Rigidbody**
- Add Component → **Box Collider**

### **Step 2: Link Your Existing Obstacle**

1. Find your **ObstacleManager** GameObject in Hierarchy
2. Select it
3. In Inspector, find the **ObstacleManager** component
4. Look for the new field: **Existing Obstacle**
5. **Drag your obstacle** from Hierarchy into this field

### **Step 3: Test It!**

1. Press **Play** in Unity
2. Watch your obstacle - it should:
   - ✅ Reset to center (0, 0.2, 0) each episode
   - ✅ Get a random rotation
   - ✅ Have its physics reset (stop moving)
   - ✅ Stay as ONE obstacle (no duplicates!)

3. Check Console for: **"Existing obstacle reset to center (0, 0.2, 0)"**

---

## 🎮 What Changed

### **Before:**
```
ObstacleManager finds no prefab
  ↓
Creates default prefab
  ↓
Spawns NEW obstacle each episode
  ↓
Your existing obstacle just sits there, never resets
```

### **After:**
```
ObstacleManager checks for existing obstacle
  ↓
Found your obstacle!
  ↓
Resets its position to center
  ↓
Resets physics (velocity = 0)
  ↓
Random rotation
  ↓
Done! Same obstacle, fresh reset
```

---

## 📋 ObstacleManager Fields Explained

| Field | What It Does |
|-------|--------------|
| **Existing Obstacle** | ⭐ Drag your scene obstacle here |
| **Obstacle Prefab** | Only used if no existing obstacle |
| **Center Spawn Position** | Where obstacle spawns (0, 0.2, 0) |
| **Obstacle Mass** | How heavy it is (affects pushing) |
| **Target** | Auto-linked by TagAgent |
| **Agent** | Auto-linked by TagAgent |

---

## 🎯 Position Reference

Your obstacle will reset to: **(0, 0.2, 0)**

- **X = 0**: Center horizontally
- **Y = 0.2**: Just above ground (height of obstacle bottom)
- **Z = 0**: Center depth-wise

Perfect center between the two agents! 🎯

---

## 🔧 Adjusting Spawn Height

If your obstacle is floating or sinking:

1. Select **ObstacleManager**
2. In Inspector → **Center Spawn Position**
3. Adjust the **Y value**:
   - Too low? Increase Y (e.g., 0.3)
   - Too high? Decrease Y (e.g., 0.15)
   - Should be half the obstacle's height

---

## 🐛 Troubleshooting

### **"Obstacle still not resetting"**

**Check 1:** Is obstacle linked?
- Select ObstacleManager
- "Existing Obstacle" field should have your obstacle

**Check 2:** Is ObstacleManager linked to agents?
- Select both agents
- TagAgent component → "Obstacle Manager" should be filled

**Check 3:** Look at Console
- Should see: "Existing obstacle reset to center..."
- If you see: "New obstacle spawned..." → Existing Obstacle not linked

### **"I see two obstacles now"**

This means:
1. You have your original obstacle
2. AND the manager is creating a new one

**Fix:**
- Link your existing obstacle to ObstacleManager
- It will use that one instead of creating new ones

### **"Obstacle falls through floor"**

**Fix:**
- Make sure your ground has a Collider
- Increase obstacle's Y position (0.25 instead of 0.2)

### **"Obstacle doesn't have physics"**

**Fix:**
- Select obstacle
- Add Component → Rigidbody
- Add Component → Box Collider
- OR let ObstacleManager set it up (it calls SetupObstaclePhysics automatically)

### **"Obstacle is too easy/hard to push"**

**Adjust:**
- Select ObstacleManager
- Change "Obstacle Mass" (lower = easier to push)
- Or increase agent moveSpeed in TagAgent

---

## ✅ Quick Checklist

- [ ] Obstacle has Rigidbody (Use Gravity = OFF)
- [ ] Obstacle has Box Collider
- [ ] Obstacle is dragged to "Existing Obstacle" field in ObstacleManager
- [ ] ObstacleManager is linked in both TagAgent components
- [ ] Tested - obstacle resets to center each episode!
- [ ] Console shows "Existing obstacle reset to center"
- [ ] No duplicate obstacles appearing

---

## 💡 Pro Tips

1. **Position your obstacle at center before Play** - Makes it easier to see if reset works

2. **Make obstacle a distinct color** - Easier to track during training

3. **Name it clearly** - Like "TrainingObstacle" so you don't lose it

4. **Test in Heuristic mode** - Easier to see if reset works when you control agent

5. **Check debug logs** - Console will tell you exactly what's happening

---

## 📊 Visual Setup

```
Hierarchy:
├─ Ground
├─ TaggerRunner1 (TagAgent)
│  └─ Obstacle Manager: [ObstacleManager]
├─ TaggerRunner2 (TagAgent)
│  └─ Obstacle Manager: [ObstacleManager]
├─ ObstacleManager
│  ├─ Existing Obstacle: [Your Obstacle] ← DRAG HERE!
│  └─ Center Spawn Position: (0, 0.2, 0)
├─ YourObstacle ← This is what gets reset!
│  ├─ Rigidbody
│  └─ Box Collider
└─ AreaCenter (0, 0, 0)
```

---

That's it! Your existing obstacle should now reset properly every episode. No more duplicates, no more static obstacles! 🎯

The obstacle will:
- ✅ Reset to center (0, 0.2, 0)
- ✅ Get random rotation
- ✅ Reset velocity to zero
- ✅ Stay as ONE obstacle
- ✅ Be pushed by agents during play
