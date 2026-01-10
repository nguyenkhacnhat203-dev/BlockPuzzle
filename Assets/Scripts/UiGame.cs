using TMPro;
using UnityEngine;

public class UiGame : MonoBehaviour
{
    public static UiGame Instance;
    public int Score;
    public int HighestScore;
    public TextMeshProUGUI Score_text;
    public TextMeshProUGUI HighestScore_text; 

    void Awake() { Instance = this; }

    void Start()
    {
        HighestScore = PlayerPrefs.GetInt("HighestScore", 0);
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        Score += amount;
        if (Score > HighestScore)
        {
            HighestScore = Score;
            PlayerPrefs.SetInt("HighestScore", HighestScore);
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (Score_text != null)
            Score_text.text = Score.ToString();

        if (HighestScore_text != null)
            HighestScore_text.text = HighestScore.ToString();
    }
}