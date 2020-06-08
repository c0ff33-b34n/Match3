using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConfirmPanel : MonoBehaviour
{
    public string levelToLoad;
    public Image[] stars;
    public int level;

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
        SetStars();    
    }

    void SetStars()
    {
        // Remove filled stars.
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = false;
        }
    }
}
