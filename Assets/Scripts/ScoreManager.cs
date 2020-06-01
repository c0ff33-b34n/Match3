using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{

    public Text scoreText;
    public int score;
    private Board board;
    public Image scoreBar;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        SetScoreBarPosition();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "" + score;
    }

    public void IncreaseScore(int addAmount)
    {
        score += addAmount;
        SetScoreBarPosition();
    }

    public void SetScoreBarPosition()
    {
        if (board != null && scoreBar != null)
        {
            int length = board.scoreGoals.Length;
            scoreBar.fillAmount = (float)score / (float)board.scoreGoals[length - 1];
        }
    }
}
