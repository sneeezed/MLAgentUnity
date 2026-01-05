using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this line

public class ScoreDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text taggerWinsText;
    public TMP_Text runnerWinsText;
    public TMP_Text taggerRewardText;
    public TMP_Text runnerRewardText;
    public TMP_Text currentRoundText;
    
    [Header("Display Settings")]
    public bool showInGame = true;
    
    // Score tracking
    private int taggerWins = 0;
    private int runnerWins = 0;
    private int totalRounds = 0;
    private float taggerReward = 0f;
    private float runnerReward = 0f;

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
    /// Update the current rewards for both agents
    /// </summary>
    public static void UpdateRewards(float taggerRew, float runnerRew)
    {
        if (instance != null)
        {
            instance.taggerReward = taggerRew;
            instance.runnerReward = runnerRew;
            instance.UpdateDisplay();
        }
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
            instance.taggerReward = 0f;
            instance.runnerReward = 0f;
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
            taggerWinsText.text = $"<color=red>Tagger</color> Wins: {taggerWins}";
        }

        if (runnerWinsText != null)
        {
            runnerWinsText.text = $"<color=blue>Runner</color> Wins: {runnerWins}";
        }

        if (taggerRewardText != null)
        {
            taggerRewardText.text = $"<color=red>Tagger</color> Reward: {taggerReward:F3}";
        }

        if (runnerRewardText != null)
        {
            runnerRewardText.text = $"<color=blue>Runner</color> Reward: {runnerReward:F3}";
        }

        if (currentRoundText != null)
        {
            currentRoundText.text = $"Total Rounds: {totalRounds}";
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
