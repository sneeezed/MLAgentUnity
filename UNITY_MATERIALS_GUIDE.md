# How to Create Red & Blue Materials in Unity

Super simple step-by-step guide with pictures described!

---

## 🔴 Creating RED Material (for Tagger)

### Step 1: Create the Material
1. Look at the **Project panel** at the bottom of Unity
2. **Right-click** anywhere in the Assets folder
3. Hover over **Create**
4. Click **Material**

### Step 2: Name It
1. A new material appears (probably named "New Material")
2. Type: **`TaggerMaterial`**
3. Press Enter

### Step 3: Make It Red
1. **Click once** on the TaggerMaterial you just created
2. Look at the **Inspector panel** on the right side
3. Find the word **"Albedo"** near the top
4. Next to "Albedo" is a **white rectangular box** - click on it
5. A **color picker** window pops up
6. Either:
   - **Drag the circle** to the red corner, OR
   - Set the values: **R: 255, G: 0, B: 0**
7. Close the color picker

✅ **Done!** You now have a red material.

---

## 🔵 Creating BLUE Material (for Runner)

### Step 1: Create the Material
1. In the **Project panel** at the bottom
2. **Right-click** → **Create** → **Material**

### Step 2: Name It
1. Name it: **`RunnerMaterial`**
2. Press Enter

### Step 3: Make It Blue
1. **Click** on RunnerMaterial
2. In the **Inspector** (right side), find **"Albedo"**
3. Click the **white box** next to Albedo
4. In the color picker:
   - **Drag the circle** to the blue corner, OR
   - Set: **R: 0, G: 0, B: 255**
5. Close the color picker

✅ **Done!** You now have a blue material.

---

## 📍 What is "Area Center"?

**Area Center is just an empty marker at the center of your play area.**

### How to Create It:

1. In Unity's **Hierarchy** (left side), right-click
2. Click **Create Empty**
3. A new GameObject appears - rename it to: **`AreaCenter`**
4. In the **Inspector**, set its **Transform Position** to:
   - X: **0**
   - Y: **0**
   - Z: **0**

That's it! This is just a reference point for your agents to know where the center is.

---

## 🎯 Linking Everything in TagAgent

Now that you have both materials and the AreaCenter:

### On BOTH agents:

1. Select the agent
2. Find the **TagAgent** component in Inspector
3. You'll see several empty fields:

#### Fill them in:
- **Other Agent**: Drag the OTHER agent here
  - If you're on Agent1, drag Agent2 here
  - If you're on Agent2, drag Agent1 here

- **Obstacle Manager**: Drag your ObstacleManager GameObject here

- **Area Center**: Drag the AreaCenter GameObject you just created

- **Tagger Material**: 
  - Look in your Project panel
  - Find TaggerMaterial
  - **Drag it** to this field

- **Runner Material**:
  - Find RunnerMaterial in Project panel
  - **Drag it** to this field

---

## ✅ Quick Checklist

- [ ] Created TaggerMaterial (red)
- [ ] Created RunnerMaterial (blue)
- [ ] Created AreaCenter at (0,0,0)
- [ ] Both materials are assigned to BOTH agents
- [ ] AreaCenter is assigned to BOTH agents
- [ ] ObstacleManager is assigned to BOTH agents
- [ ] Each agent has reference to the OTHER agent

---

## 🎨 Visual Reference

When you're done:
- Both materials should appear in your **Project panel** (bottom)
- TaggerMaterial should show as **red** when you click it
- RunnerMaterial should show as **blue** when you click it
- AreaCenter should be in your **Hierarchy** at position (0,0,0)

---

## 🐛 Troubleshooting

**"I can't find the Albedo setting"**
- Make sure you clicked on the material in the Project panel
- Look in the Inspector on the right
- It should be near the top, below "Shader"

**"The color picker won't open"**
- Make sure you're clicking the small **color box**, not the word "Albedo"
- The box is to the right of the word "Albedo"

**"I don't see the materials"**
- Look in the Project panel at the bottom
- They should appear as spheres with your chosen colors
- Make sure you're in the right folder

**"Where is the Inspector?"**
- The Inspector is the panel on the **right side** of Unity
- If you don't see it, go to: Window → General → Inspector

---

That's it! You're ready to continue with the tag game setup! 🎮
