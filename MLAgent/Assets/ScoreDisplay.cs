using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Text taggerWinsText;
    public Text runnerWinsText;
    public Text currentRoundText;
    
    [Header("Display Settings")]
    public bool showInGame = true;
    
    // Score tracking
    private int taggerWins = 0;
    private int runnerWins = 0;
    private int totalRounds = 0;

    private static ScoreDisplay instance;

    private void Awake()
    {
        // Singleton pattern so any script can access this
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateDisplay();
    }

    /// <summary>
    /// Call this when tagger wins
    /// </summary>
    public static void TaggerWon()
    {
        if (instance != null)
        {
            instance.taggerWins++;
            instance.totalRounds++;
            instance.UpdateDisplay();
            Debug.Log($"🔴 TAGGER WINS! (Total: {instance.taggerWins}/{instance.totalRounds})");
        }
    }

    /// <summary>
    /// Call this when runner wins
    /// </summary>
    public static void RunnerWon()
    {
        if (instance != null)
        {
            instance.runnerWins++;
            instance.totalRounds++;
            instance.UpdateDisplay();
            Debug.Log($"🔵 RUNNER WINS! (Total: {instance.runnerWins}/{instance.totalRounds})");
        }
    }

    /// <summary>
    /// Reset all scores
    /// </summary>
    public static void ResetScores()
    {
        if (instance != null)
        {
            instance.taggerWins = 0;
            instance.runnerWins = 0;
            instance.totalRounds = 0;
            instance.UpdateDisplay();
        }
    }

    /// <summary>
    /// Update the UI display
    /// </summary>
    private void UpdateDisplay()
    {
        if (!showInGame) return;

        if (taggerWinsText != null)
        {
            taggerWinsText.text = $"🔴 Tagger Wins: {taggerWins}";
        }

        if (runnerWinsText != null)
        {
            runnerWinsText.text = $"🔵 Runner Wins: {runnerWins}";
        }

        if (currentRoundText != null)
        {
            float taggerWinRate = totalRounds > 0 ? (taggerWins / (float)totalRounds) * 100f : 0f;
            float runnerWinRate = totalRounds > 0 ? (runnerWins / (float)totalRounds) * 100f : 0f;
            
            currentRoundText.text = $"Total Rounds: {totalRounds}\n" +
                                   $"Tagger Win Rate: {taggerWinRate:F1}%\n" +
                                   $"Runner Win Rate: {runnerWinRate:F1}%";
        }
    }

    /// <summary>
    /// Get current scores (useful for external systems)
    /// </summary>
    public static (int tagger, int runner, int total) GetScores()
    {
        if (instance != null)
        {
            return (instance.taggerWins, instance.runnerWins, instance.totalRounds);
        }
        return (0, 0, 0);
    }
}
