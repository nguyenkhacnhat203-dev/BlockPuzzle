using UnityEngine;

public class CalculateScore : MonoBehaviour
{
    public static CalculateScore Instance;

    [Header("Score Settings")]
    public int pointsPerLine = 8;
    public int maxLosesAllowed = 3;

    private int currentComboMultiplier = 1;
    private int consecutiveLoses = 0;

    void Awake()
    {
        Instance = this;
    }

    public void AddPlacementScore(int cellCount)
    {
        if (UiGame.Instance != null)
            UiGame.Instance.AddScore(cellCount);
    }

    public void AddClearScore(int linesCleared)
    {
        if (linesCleared > 0)
        {
            int points = pointsPerLine * linesCleared * currentComboMultiplier;
            UiGame.Instance.AddScore(points);

            currentComboMultiplier++;
            consecutiveLoses = 0;
        }
        else
        {
            consecutiveLoses++;

            if (consecutiveLoses > maxLosesAllowed)
            {
                currentComboMultiplier = 1;
                consecutiveLoses = 0;
            }
        }
    }

    public void ResetScoreData()
    {
        currentComboMultiplier = 1;
        consecutiveLoses = 0;

        if (UiGame.Instance != null)
        {
            UiGame.Instance.Score = 0;
            UiGame.Instance.UpdateUI();
        }
    }
}
