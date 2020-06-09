using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConfirmPanel : MonoBehaviour
{
    public string levelToLoad;
    public Image[] stars;
    public Text highScoreText;
    public Text starText;
    public int level;
    private int starsActive;
    private int highScore;
    private GoalManager goalManager;
    private GameData gameData;

    public void Cancel()
    {
        this.gameObject.SetActive(false);
    }

    public void Play()
    {
        PlayerPrefs.SetInt("Current Level", level - 1);
        SceneManager.LoadScene(levelToLoad);
    }

    void OnEnable()
    {
        gameData = FindObjectOfType<GameData>();
        goalManager = GetComponent<GoalManager>();
        LoadData();
        SetStars();
        SetText();
        ResetNumberCollected();
    }

    void LoadData()
    {
        if (gameData != null)
        {
            starsActive = gameData.saveData.stars[level - 1];
            highScore = gameData.saveData.highScores[level - 1];
        }
    }

    void SetText()
    {
        highScoreText.text = "" + highScore;
        starText.text = "" + starsActive + "/3";
    }

    void SetStars()
    {
        for (int i = 0; i < starsActive; i++)
        {
            stars[i].enabled = true;
        }
    }

    void ResetNumberCollected()
    {
        if (goalManager != null)
        {
            goalManager.ResetGoals();
        }
    }
}
