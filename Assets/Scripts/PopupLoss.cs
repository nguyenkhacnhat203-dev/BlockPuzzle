using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupLoss : PopupBase
{
    public TextMeshProUGUI Score_text;
    public TextMeshProUGUI BestScore_text;


    private bool isRevive = false;
    protected override void OnShow()
    {
        int currentScore = UiGame.Instance != null ? UiGame.Instance.Score : 0;
        int highestScore = UiGame.Instance != null ? UiGame.Instance.HighestScore : 0;

        Score_text.gameObject.SetActive(false);
        BestScore_text.gameObject.SetActive(false);

        if (currentScore > highestScore)
        {
            BestScore_text.gameObject.SetActive(true);
            BestScore_text.text = $"Best Score : {currentScore}";
        }
        else
        {
            Score_text.gameObject.SetActive(true);
            Score_text.text = $"Score : {currentScore}";
        }
    }


    public override void DestroyPopup()
    {
    }


    public void OnClickNewGame()
    {
        if (UiGame.Instance != null)
        {
            UiGame.Instance.ResetScore(); 
        }

        Board board = FindObjectOfType<Board>();
        if (board != null)
        {
            board.ResetGameFromPopup();
        }

        PopupManager.Instance.isShowPopup = false;
        Destroy(gameObject);
    }

    public void OnClickRevive()
    {
        AdManager.Instance.ShowRewarded(() =>
        {
            isRevive = true;    
            Board board = FindObjectOfType<Board>();
            BlockManager bm = BlockManager.Instance;

            if (board != null && bm != null)
            {
                board.ReviveClearHalfBoard();
                bm.ReviveSpawnBlocks();
            }

            PopupManager.Instance.isShowPopup = false;
            Destroy(gameObject);
        });
    }
    private void OnApplicationQuit()
    {
        if(isRevive == false)
        {
            Board board = FindObjectOfType<Board>();
            if (board != null)
            {
                board.ResetGameFromPopup();
            }
            UiGame.Instance.Score = 0;

            PlayerPrefs.SetInt("LastScore", 0);
            PlayerPrefs.Save();
            Destroy(gameObject);
            PopupManager.Instance.isShowPopup = false;
        }
    }
}
