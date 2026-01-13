using TMPro;
using UnityEngine;
using DG.Tweening;

public class UiGame : MonoBehaviour
{
    public static UiGame Instance;

    [Header("Score")]
    public int Score;
    public int HighestScore;

    [Header("UI")]
    public TextMeshProUGUI Score_text;
    public TextMeshProUGUI HighestScore_text;

    [Header("DOTween Settings")]
    public float pulseScale = 1.2f;
    public float pulseDuration = 0.15f;

    private Vector3 originalScale;

    private const string HIGHEST_SCORE_KEY = "HighestScore";
    private const string LAST_SCORE_KEY = "LastScore";

    void Awake()
    {
        Instance = this;

        if (Score_text != null)
            originalScale = Score_text.transform.localScale;
    }

    void Start()
    {
        HighestScore = PlayerPrefs.GetInt(HIGHEST_SCORE_KEY, 0);
        Score = PlayerPrefs.GetInt(LAST_SCORE_KEY, 0); 
        UpdateUI();
    }

    #region SCORE
    public void AddScore(int amount)
    {
        Score += amount;

        if (Score > HighestScore)
        {
            HighestScore = Score;
            PlayerPrefs.SetInt(HIGHEST_SCORE_KEY, HighestScore);
        }

        UpdateUI();
        PlayPulseEffect();
    }
    #endregion

    #region DOTWEEN EFFECT
    void PlayPulseEffect()
    {
        if (Score_text == null) return;

        Transform t = Score_text.transform;

        t.DOKill();

        t.localScale = originalScale;

        t.DOScale(originalScale * pulseScale, pulseDuration)
         .SetEase(Ease.OutBack)
         .OnComplete(() =>
         {
             t.DOScale(originalScale, pulseDuration)
              .SetEase(Ease.InBack);
         });
    }
    #endregion

    #region UI
    public void UpdateUI()
    {
        if (Score_text != null)
            Score_text.text = Score.ToString();

        if (HighestScore_text != null)
            HighestScore_text.text = HighestScore.ToString();
    }
    #endregion

    #region SAVE LAST SCORE
    void OnApplicationQuit()
    {
        SaveLastScore();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveLastScore();
    }

    void SaveLastScore()
    {
        PlayerPrefs.SetInt(LAST_SCORE_KEY, Score);
        PlayerPrefs.Save();
    }

    public int GetLastScore()
    {
        return PlayerPrefs.GetInt(LAST_SCORE_KEY, 0);
    }
    #endregion


    public void ResetScore()
    {
        Score = 0;
        PlayerPrefs.SetInt(LAST_SCORE_KEY, 0);
        PlayerPrefs.Save();
        UpdateUI();
    }


}
