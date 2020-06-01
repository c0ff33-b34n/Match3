﻿using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{

    public Text scoreText;
    public int score;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "" + score;
    }

    public void IncreaseScore(int addAmount)
    {
        score += addAmount;
    }
}
