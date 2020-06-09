using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public bool isActive;
    public Sprite activeSprite;
    public Sprite lockedSprite;
    private Image buttonImage;
    private Button levelButton;
    private int starsActive;

    public Image[] stars;
    public Text levelText;
    public int level;
    public GameObject confirmPanel;

    private GameData gameData;
    void Start()
    {
        buttonImage = GetComponent<Image>();
        levelButton = GetComponent<Button>();
        gameData = FindObjectOfType<GameData>();
        LoadData();
        SetStars();
        ShowLevel();
        SetButtonSprite();
    }

    void LoadData()
    {
        Debug.Log("LevelButton Script, LoadData() Called");
        if (gameData != null)
        {
            Debug.Log("Level button script, gameData is not null in LoadData");
            if (gameData.saveData.isActive[level - 1])
            {
                isActive = true;
            } else
            {
                isActive = false;
            }
            starsActive = gameData.saveData.stars[level - 1];

        }
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
        for (int i = 0; i < starsActive; i++)
        {
            stars[i].enabled = true;
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
