# Obstacle Not Resetting? Debug Guide

Quick checklist to diagnose and fix obstacle reset issues!

---

## ✅ Fixed Issues

1. **✅ Gravity is now ENABLED** - Obstacle will fall naturally to ground
2. **✅ Better debug logging** - Console will tell you exactly what's happening

---

## 🔍 Step-by-Step Diagnosis

### **Step 1: Check Console Logs**

Press Play in Unity and look at the **Console** (bottom of Unity). You should see:

**✅ GOOD - Obstacle resetting:**
```
=== SpawnObstacle called ===
✅ Using existing obstacle: YourObstacleName
Moving obstacle from (X, Y, Z) to (0, 0.2, 0)
✅ Physics velocities reset to zero
✅ Existing obstacle 'YourObstacleName' reset to center (0, 0.2, 0)
```

**❌ BAD - No existing obstacle linked:**
```
=== SpawnObstacle called ===
⚠️ No existing obstacle linked! Trying to create dynamic obstacle...
❌ No obstacle prefab and no existing obstacle! Can't spawn obstacle!
💡 FIX: Drag your obstacle from the scene to the 'Existing Obstacle' field in ObstacleManager!
```

**❌ BAD - SpawnObstacle never called:**
```
(No logs at all)
```

---

## 🛠️ Where Should ObstacleManager Be?

### **✅ CORRECT Setup:**

```
Hierarchy:
└─ TrainingArea (Your play area)
   ├─ Ground
   ├─ TaggerRunner1 (agent)
   ├─ TaggerRunner2 (agent)
   ├─ ObstacleManager ← Add it here!
   │  └─ ObstacleManager component
   │     ├─ Existing Obstacle: [Drag your obstacle here]
   │     └─ Center Spawn Position: (0, 0.2, 0)
   ├─ YourObstacle ← This is what gets reset
   │  ├─ Rigidbody (Use Gravity = ON)
   │  └─ Box Collider
   └─ AreaCenter (0, 0, 0)
```

**Yes, having ObstacleManager on the TrainingArea is CORRECT!** ✅

---

## 🔧 Common Issues & Fixes

### **Issue 1: "No logs appearing in Console"**

**Problem:** SpawnObstacle() is never being called

**Fix:**
1. Select both agents
2. Check TagAgent component
3. Make sure **"Obstacle Manager"** field has ObstacleManager linked
4. If empty, drag ObstacleManager to both agents

---

### **Issue 2: "⚠️ No existing obstacle linked!"**

**Problem:** You didn't link your obstacle to ObstacleManager

**Fix:**
1. Select **ObstacleManager** in Hierarchy
2. In Inspector, find **"Existing Obstacle"** field (should be at top)
3. Drag your obstacle from Hierarchy into this field
4. Press Play again

---

### **Issue 3: "⚠️ Obstacle has no Rigidbody!"**

**Problem:** Your obstacle is missing physics

**Fix:**
1. Select your obstacle
2. Add Component → **Rigidbody**
   - Mass: 1
   - Use Gravity: ✅ ON
   - Is Kinematic: ❌ OFF
3. Add Component → **Box Collider** (if missing)

---

### **Issue 4: Obstacle falls through floor**

**Problem:** Ground doesn't have a collider or obstacle spawns below ground

**Fix Option 1:** Check ground
- Select Ground
- Make sure it has a **Collider** (Box Collider or Mesh Collider)

**Fix Option 2:** Adjust spawn height
- Select ObstacleManager
- Change **Center Spawn Position** Y value to 0.5 (higher)

---

### **Issue 5: Obstacle resets but to wrong position**

**Problem:** Center Spawn Position is wrong

**Fix:**
- Select ObstacleManager
- Set **Center Spawn Position** to: (0, 0.2, 0)
- Adjust Y if needed based on obstacle size

---

### **Issue 6: "Obstacle IS resetting but I can't see it"**

**Problem:** Obstacle might be resetting under the floor or at wrong height

**Fix:**
1. Press Play
2. In Scene view (not Game view), look at position (0, 0, 0)
3. If you see it there, adjust Center Spawn Position Y value

---

## 📋 Complete Setup Checklist

Run through this in order:

- [ ] **ObstacleManager GameObject exists** in TrainingArea
- [ ] **ObstacleManager component** is on that GameObject
- [ ] **Your obstacle exists** in the scene (visible in Hierarchy)
- [ ] **Obstacle has Rigidbody** (Use Gravity = ON)
- [ ] **Obstacle has Box Collider**
- [ ] **Ground has a Collider**
- [ ] **Existing Obstacle field** has your obstacle linked
- [ ] **Both agents' TagAgent components** have ObstacleManager linked
- [ ] **Press Play** and check Console for green checkmarks

---

## 🎯 Quick Test

1. **Press Play** in Unity
2. **Immediately check Console** - should see:
   ```
   === SpawnObstacle called ===
   ✅ Using existing obstacle: ...
   ```
3. **Look at obstacle** - should jump to center (0, 0.2, 0)
4. **Wait for episode to end** - obstacle should reset again
5. **Each episode** = obstacle resets

---

## 💡 Understanding the Flow

```
Episode Starts
    ↓
TagAgent.OnEpisodeBegin() runs (ONE agent only)
    ↓
Calls obstacleManager.SpawnObstacle()
    ↓
ObstacleManager checks: Is existingObstacle linked?
    ↓
YES → Reset that obstacle's position/rotation/physics
    ↓
Obstacle now at center (0, 0.2, 0) with gravity ON
    ↓
Falls to ground naturally
    ↓
Ready for tag game!
```

---

## 🔍 Advanced Debug

If still not working, check these:

### **Check 1: Is OnEpisodeBegin being called?**

Add this to TagAgent.cs OnEpisodeBegin():
```csharp
Debug.Log($"OnEpisodeBegin called on {gameObject.name}");
```

Should see this log when episode starts.

### **Check 2: Is obstacle manager null?**

Add this before obstacleManager.SpawnObstacle():
```csharp
if (obstacleManager == null)
{
    Debug.LogError($"ObstacleManager is NULL on {gameObject.name}!");
}
```

### **Check 3: Which agent controls spawning?**

Only ONE agent spawns the obstacle (the one with lower sibling index).
This is controlled by:
```csharp
if (transform.GetSiblingIndex() == 0)
{
    obstacleManager.SpawnObstacle();
}
```

So only Agent1 spawns it, Agent2 doesn't.

---

## ✅ Success Indicators

You'll know it's working when:

1. ✅ Console shows green checkmarks
2. ✅ Obstacle jumps to center each episode
3. ✅ Obstacle falls to ground (gravity works)
4. ✅ Agents can push obstacle during play
5. ✅ Each new episode = obstacle resets position

---

## 🆘 Still Broken?

Run these checks IN ORDER:

1. Press Play → Check Console immediately
2. If NO logs → ObstacleManager not linked to agents
3. If "⚠️ No existing obstacle" → Drag obstacle to Existing Obstacle field
4. If "⚠️ No Rigidbody" → Add Rigidbody to obstacle
5. If green checkmarks but obstacle not moving → Check obstacle's Transform position in Inspector while playing
6. If obstacle disappears → Check it hasn't fallen through floor

---

**The debug logs will tell you EXACTLY what's wrong!** Just read the Console! 🎯
