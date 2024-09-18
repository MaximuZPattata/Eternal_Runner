using System.Collections;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class GameOver : MonoBehaviour
{
    #region Defining local variables

    [SerializeField]
    private GameObject gameOverCanvas;

    [SerializeField]
    private TMP_InputField inputField;

    private int score = 0;
    private int leaderboardId = 24551;
    private int leaderboardTopCount = 10;

    #endregion

    #region Public functions

    public void StopGame(int score)
    {
        //Time.timeScale = 0f;

        gameOverCanvas.SetActive(true);

        this.score = score;

        SubmitScore();
    }

    public void RestartGame()
    {

    }

    public void SubmitScore()
    {
        StartCoroutine(SubmitScoreToLeaderboard());
    }

    public void AddXP(int score)
    {

    }

    #endregion

    #region Private functions

    private IEnumerator SubmitScoreToLeaderboard()
    {
        bool? nameSet = null;

        LootLockerSDKManager.SetPlayerName(inputField.text, (response) =>
        {
            if (response.success)
            {
                Debug.Log("SUCCESS : Player name set");

                nameSet = true;
            }
            else
            {
                Debug.Log("ERROR : Unable to set player name");
                
                nameSet = false;
            }
        });

        yield return new WaitUntil(() => nameSet.HasValue);

        if(!nameSet.Value)      
            yield break;

        bool? scoreSubmitted = null;

        LootLockerSDKManager.SubmitScore("", score, leaderboardId.ToString(), (response) =>
        {
            if (response.success)
            {
                Debug.Log("SUCCESS : Score submitted to the leaderboard");

                scoreSubmitted = true;
            }
            else
            {
                Debug.Log("ERROR : Unable to submit score to the leaderboard");

                scoreSubmitted = false;
            }
        });

        yield return new WaitUntil(() => scoreSubmitted.HasValue);
        
        if (!scoreSubmitted.Value)
            yield break;

        GetLeaderboard();
    }

    private void GetLeaderboard()
    {

    }

    #endregion
}
