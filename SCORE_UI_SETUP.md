# Score Display UI Setup Guide

How to add a score counter showing Tagger vs Runner wins in your Unity game!

---

## 🎯 What You'll Get

A clean UI display showing:
- 🔴 **Tagger Wins**: Total times tagger caught runner
- 🔵 **Runner Wins**: Total times runner survived
- **Win Rates**: Percentage breakdown
- **Total Rounds**: Total episodes played

---

## 🛠️ Unity Setup (5 Minutes)

### **Step 1: Create the Canvas**

1. In Unity **Hierarchy**, right-click → **UI → Canvas**
2. A "Canvas" GameObject appears
3. Keep default settings (Screen Space - Overlay)

### **Step 2: Create Score Display Container**

1. Right-click on **Canvas** → **UI → Panel**
2. Rename it to: `ScorePanel`
3. In **Inspector**, adjust the panel:
   - **Rect Transform**:
     - Anchor: **Top Left** (click the square with the crosshair, then hold Shift+Alt and click top-left)
     - Pos X: **120**, Pos Y: **-100**
     - Width: **240**, Height: **200**
   - **Image** component:
     - Color: Black with alpha **150** (semi-transparent)

### **Step 3: Create Tagger Wins & Reward Text**

1. Right-click on **ScorePanel** → **UI → Text**
2. Rename to: `TaggerWinsText`
3. Configure:
   - **Rect Transform**: Pos Y: **-10**, Height: **30**
   - **Text**: `🔴 Tagger Wins: 0`, Color: **Red**, Size: **18**

4. Right-click on **ScorePanel** → **UI → Text**
5. Rename to: `TaggerRewardText`
6. Configure:
   - **Rect Transform**: Pos Y: **-40**, Height: **30**
   - **Text**: `🔴 Tagger Reward: 0.000`, Color: **Red**, Size: **16**

### **Step 4: Create Runner Wins & Reward Text**

1. Right-click on **ScorePanel** → **UI → Text**
2. Rename to: `RunnerWinsText`
3. Configure:
   - **Rect Transform**: Pos Y: **-80**, Height: **30**
   - **Text**: `🔵 Runner Wins: 0`, Color: **Blue**, Size: **18**

4. Right-click on **ScorePanel** → **UI → Text**
5. Rename to: `RunnerRewardText`
6. Configure:
   - **Rect Transform**: Pos Y: **-110**, Height: **30**
   - **Text**: `🔵 Runner Reward: 0.000`, Color: **Blue**, Size: **16**

### **Step 5: Create Stats Text**

1. Right-click on **ScorePanel** → **UI → Text**
2. Rename to: `CurrentRoundText`
3. Configure:
   - **Rect Transform**:
     - Pos Y: **-160**, Height: **30**
   - **Text** component:
     - Text: `Total Rounds: 0`
     - Font Size: **14**
     - Color: **White**
     - Alignment: **Left & Middle**

### **Step 6: Add ScoreDisplay Script**

1. Right-click in **Hierarchy** → **Create Empty**
2. Rename to: `ScoreManager`
3. In **Inspector**, click **Add Component**
4. Search for and add: **ScoreDisplay**
5. In the **ScoreDisplay** component, fill in:
   - **Tagger Wins Text**: Drag `TaggerWinsText` here
   - **Runner Wins Text**: Drag `RunnerWinsText` here
   - **Tagger Reward Text**: Drag `TaggerRewardText` here
   - **Runner Reward Text**: Drag `RunnerRewardText` here
   - **Current Round Text**: Drag `CurrentRoundText` here
   - **Show In Game**: ✅ Checked

---

## ✅ Testing

1. **Press Play** in Unity
2. Let a round play out (or control with Heuristic mode)
3. When episode ends, you should see:
   - Score counter updates
   - Win rate percentages
   - Console log: "🔴 TAGGER WINS!" or "🔵 RUNNER WINS!"

---

## 🎨 Customization (Optional)

### **Change Position**
- Select `ScorePanel`
- Drag it in Scene view to reposition
- Or adjust Pos X/Y in Inspector

### **Change Size**
- Select `ScorePanel`
- Adjust Width/Height in Rect Transform

### **Change Colors**
- Select individual text elements
- Change color in Text component

### **Change Font**
- Import a custom font into Unity
- Select text element → Text component → Font → Choose your font

### **Add Background Image**
- Select `ScorePanel`
- Image component → Source Image → Choose a sprite
- Or change background color

### **Make it Bigger/Smaller**
- Select all text elements
- Change Font Size in Text component

---

## 🔧 Advanced: Multiple Training Areas

If you have multiple training areas (for parallel training):

1. **Each training area should have its own ScoreManager**
2. Duplicate the Canvas for each area
3. Make sure each Canvas renders to **World Space** instead of Overlay:
   - Select Canvas
   - Canvas component → Render Mode → **World Space**
   - Position it above each training area
   - Scale: 0.01, 0.01, 0.01 (to make it readable in world)

---

## 📊 What Gets Tracked

### **Tagger Wins When:**
- ✅ Tagger gets within 2 units of runner
- ✅ Tag happens before time limit

### **Runner Wins When:**
- ✅ Survives 500 timesteps without being tagged
- ✅ Time runs out

### **Neither Wins (No Score) When:**
- ❌ Agent goes out of bounds
- ❌ Agent gets stuck for 5+ seconds
- ❌ Episode ends early due to error

---

## 🐛 Troubleshooting

**"I don't see the UI"**
- Make sure Canvas is in the scene
- Check that ScorePanel is a child of Canvas
- Verify Canvas render mode is "Screen Space - Overlay"

**"UI doesn't update"**
- Check ScoreManager has ScoreDisplay script
- Verify all text fields are linked in ScoreDisplay component
- Look for errors in Console

**"Scores are always 0"**
- Make sure TagAgent.cs has been updated (should have ScoreDisplay.TaggerWon() calls)
- Check that episodes are actually ending
- Look for console logs showing wins

**"UI is too small/big"**
- Adjust Font Size in text components
- Resize ScorePanel width/height

**"Can't find emoji in UI"**
- Unity's default font doesn't support emoji
- Remove the 🔴 🔵 from the text if they don't show
- Or import a font that supports emoji

**"ScoreDisplay script not found"**
- Make sure ScoreDisplay.cs is in your Assets folder
- Wait for Unity to recompile (watch bottom-right of Unity)

---

## 🎮 Quick Layout Reference

```
Hierarchy Structure:
├─ Canvas
│  └─ ScorePanel
│     ├─ TaggerWinsText
│     ├─ RunnerWinsText
│     └─ CurrentRoundText
├─ ScoreManager (with ScoreDisplay script)
├─ TaggerRunner1
├─ TaggerRunner2
└─ ObstacleManager
```

---

## 💡 Pro Tips

1. **Testing manually?** Set one agent to Heuristic mode and play - you'll see scores update in real-time!

2. **Want to reset scores?** Add a button:
   - UI → Button
   - Button text: "Reset Scores"
   - OnClick() → ScoreManager → ScoreDisplay → ResetScores()

3. **Want to see win streaks?** Modify ScoreDisplay.cs to track consecutive wins

4. **Training with multiple arenas?** Each gets its own score display!

5. **Want to hide during training?** Uncheck "Show In Game" in ScoreDisplay component

---

## 📝 Summary Checklist

- [ ] Created Canvas
- [ ] Created ScorePanel
- [ ] Created TaggerWinsText (red)
- [ ] Created RunnerWinsText (blue)
- [ ] Created CurrentRoundText
- [ ] Created ScoreManager with ScoreDisplay script
- [ ] Linked all text fields to ScoreDisplay
- [ ] Tested - scores update when episodes end!

---

That's it! You now have a working score display that tracks tagger vs runner wins! 🎯

The scores will automatically update every time an episode ends. Perfect for watching training progress or playing manually!
