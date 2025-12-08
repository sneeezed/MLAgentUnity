using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;

public class PlayerControlUI : MonoBehaviour
{
    [Header("References")]
    public BehaviorParameters agent1BehaviorParams;
    public BehaviorParameters agent2BehaviorParams;
    public GameObject agent1Object;
    public GameObject agent2Object;
    
    [Header("UI Settings")]
    public bool showControls = true;
    public KeyCode toggleControlsKey = KeyCode.H;
    
    private GUIStyle labelStyle;
    private GUIStyle controlsStyle;
    private GUIStyle headerStyle;
    
    void Start()
    {
        InitializeStyles();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleControlsKey))
        {
            showControls = !showControls;
        }
    }
    
    void OnGUI()
    {
        if (!showControls) return;
        
        // Control instructions panel
        GUI.Box(new Rect(10, 10, 300, 200), "");
        
        GUILayout.BeginArea(new Rect(20, 20, 280, 180));
        
        GUILayout.Label("PLAYER CONTROLS", headerStyle);
        GUILayout.Space(10);
        
        GUILayout.Label("Movement:", labelStyle);
        GUILayout.Label("  W/S or ↑/↓ - Forward/Backward", controlsStyle);
        GUILayout.Label("  A/D or ←/→ - Turn Left/Right", controlsStyle);
        GUILayout.Label("  Space - Jump", controlsStyle);
        
        GUILayout.Space(10);
        GUILayout.Label("Press H to toggle this help", controlsStyle);
        
        GUILayout.EndArea();
        
        // Agent status panel
        GUI.Box(new Rect(10, 220, 300, 120), "");
        
        GUILayout.BeginArea(new Rect(20, 230, 280, 100));
        
        GUILayout.Label("AGENT STATUS", headerStyle);
        GUILayout.Space(5);
        
        if (agent1BehaviorParams != null)
        {
            string agent1Status = GetBehaviorStatus(agent1BehaviorParams);
            Color agent1Color = agent1BehaviorParams.BehaviorType == BehaviorType.HeuristicOnly ? Color.green : Color.yellow;
            GUI.color = agent1Color;
            GUILayout.Label($"Agent 1: {agent1Status}", labelStyle);
            GUI.color = Color.white;
        }
        
        if (agent2BehaviorParams != null)
        {
            string agent2Status = GetBehaviorStatus(agent2BehaviorParams);
            Color agent2Color = agent2BehaviorParams.BehaviorType == BehaviorType.HeuristicOnly ? Color.green : Color.yellow;
            GUI.color = agent2Color;
            GUILayout.Label($"Agent 2: {agent2Status}", labelStyle);
            GUI.color = Color.white;
        }
        
        GUILayout.EndArea();
    }
    
    string GetBehaviorStatus(BehaviorParameters bp)
    {
        switch (bp.BehaviorType)
        {
            case BehaviorType.HeuristicOnly:
                return "🎮 PLAYER (You)";
            case BehaviorType.Default:
                return "🤖 AI (Training)";
            case BehaviorType.InferenceOnly:
                return "🤖 AI (Inference)";
            default:
                return "Unknown";
        }
    }
    
    void InitializeStyles()
    {
        headerStyle = new GUIStyle();
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.white;
        
        labelStyle = new GUIStyle();
        labelStyle.fontSize = 14;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = Color.white;
        
        controlsStyle = new GUIStyle();
        controlsStyle.fontSize = 12;
        controlsStyle.normal.textColor = Color.white;
    }
}

