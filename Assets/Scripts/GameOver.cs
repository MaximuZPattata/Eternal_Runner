using System.Collections;
using UnityEngine;
using LootLocker.Requests;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class GameOver : MonoBehaviour
{
    #region Defining local variables

    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private TextMeshProUGUI leaderboardScoreText;

    [SerializeField]
    private TextMeshProUGUI leaderboardNameText;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    private int score = 0;
    private string leaderboardId = "24551";
    private int leaderboardTopCount = 10;

    #endregion

    #region Public functions

    public void StopGame(int score)
    {
        //Time.timeScale = 0f;

        this.score = score;
        scoreText.text = "Score : " + score.ToString();

        GetLeaderboard();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

        LootLockerSDKManager.SubmitScore("", score, leaderboardId, (response) =>
        {
            if (response.success)
            {
                Debug.Log("DEBUG : Score - " + score + " | LeaderboardId - " + leaderboardId);
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
        LootLockerSDKManager.GetScoreList(leaderboardId, leaderboardTopCount, (response) =>
        {
            if (response.success)
            {                
                if (response.items != null)
                {
                    string leaderboardName = "";
                    string leaderboardScore = "";

                    LootLockerLeaderboardMember[] members = response.items;

                    Debug.Log("inside condition");

                    for (int i = 0; i < members.Length; i++)
                    {
                        Debug.Log("DEBUG : Fetching player " + i);

                        LootLockerPlayer currentPlayer = members[i].player;

                        if (currentPlayer == null)
                            continue;

                        if (currentPlayer.name != "")
                            leaderboardName += currentPlayer.name + "\n";
                        else
                            leaderboardName += currentPlayer.id + "\n";

                        leaderboardScore += members[i].score + "\n";
                    }

                    leaderboardNameText.SetText(leaderboardName);
                    leaderboardScoreText.SetText(leaderboardScore);

                    Debug.Log("SUCCESS : Scores retrieved from leaderboard");
                }
                else
                    Debug.Log("SUCCESS : No scores stored in leaderboard");
            }
            else
                Debug.Log("ERROR : Unable to get scores from leaderboard");
        });
    }

    #endregion
}
