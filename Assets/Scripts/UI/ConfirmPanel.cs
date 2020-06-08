using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConfirmPanel : MonoBehaviour
{
    public string levelToLoad;
    public Image[] stars;
    public int level;
    private GoalManager goalManager;

    public void Cancel()
    {
        this.gameObject.SetActive(false);
    }

    public void Play()
    {
        PlayerPrefs.SetInt("Current Level", level - 1);
        SceneManager.LoadScene(levelToLoad);
    }

    void Start()
    {
        goalManager = GetComponent<GoalManager>();
        SetStars();
        ResetNumberCollected();
    }

    void SetStars()
    {
        // Remove filled stars.
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = false;
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
