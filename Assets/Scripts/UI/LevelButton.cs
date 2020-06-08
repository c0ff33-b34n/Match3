using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public bool isActive;
    public Sprite activeSprite;
    public Sprite lockedSprite;
    private Image buttonImage;
    private Button levelButton;

    public Image[] stars;
    public Text levelText;
    public int level;
    public GameObject confirmPanel;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        levelButton = GetComponent<Button>();
        SetStars();
        ShowLevel();
        SetButtonSprite();
    }

    void SetButtonSprite()
    {
        if (isActive)
        {
            buttonImage.sprite = activeSprite;
            levelButton.enabled = true;
            levelText.enabled = true;
        } else
        {
            buttonImage.sprite = lockedSprite;
            levelButton.enabled = false;
            levelText.enabled = false;
        }
    }

    void SetStars()
    {
        // Remove filled stars.
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = false;
        }
    }

    void ShowLevel()
    {
        levelText.text = "" + level;
    }

    public void ConfirmPanel(int level)
    {
        confirmPanel.GetComponent<ConfirmPanel>().level = level;
        confirmPanel.SetActive(true);
    }
}
