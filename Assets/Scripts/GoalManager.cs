﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlankGoal
{
    public int numberNeeded;
    public int numberCollected;
    public Sprite goalSprite;
    public string matchValue;
}

public class GoalManager : MonoBehaviour
{
    public BlankGoal[] levelGoals;
    public List<GoalPanel> currentGoals = new List<GoalPanel>();
    public GameObject goalPrefab;
    public GameObject goalIntroParent;
    public GameObject goalGameParent;
    private Board board;
    private EndGameManager endGameManager;


    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        endGameManager = FindObjectOfType<EndGameManager>();
        GetGoals();
        SetupGoals();
    }

    void GetGoals()
    {
        if (board != null)
        {
            if (board.world != null)
            {
                if (board.world.levels[board.level] != null)
                {
                    ResetGoals();
                    levelGoals = board.world.levels[board.level].levelGoals;
                }
            }
        }
    }

    public void ResetGoals()
    {
        for (int i = 0; i < levelGoals.Length; i++)
        {
            levelGoals[i].numberCollected = 0;
        }
    }

    void SetupGoals()
    {
        for (int i = 0; i < levelGoals.Length; i++)
        {
            // Create a new Goal Panel at the GoalIntroParent position.
            GameObject goal = Instantiate(goalPrefab, goalIntroParent.transform.position, Quaternion.identity);
            goal.transform.SetParent(goalIntroParent.transform, false);
            // Set the image and the text of the goal
            GoalPanel goalPanel = goal.GetComponent<GoalPanel>();
            goalPanel.thisSprite = levelGoals[i].goalSprite;
            goalPanel.thisString = "0/" + levelGoals[i].numberNeeded;

            // Create a new Goal Panel at the goalGameParent position
            GameObject gameGoal = Instantiate(goalPrefab, goalGameParent.transform.position, Quaternion.identity);
            gameGoal.transform.SetParent(goalGameParent.transform, false);
            goalPanel = gameGoal.GetComponent<GoalPanel>();
            currentGoals.Add(goalPanel);
            goalPanel.thisSprite = levelGoals[i].goalSprite;
            goalPanel.thisString = "0/" + levelGoals[i].numberNeeded;
        }
    }

    public void UpdateGoals()
    {
        int goalsCompleted = 0;
        for (int i = 0; i < levelGoals.Length; i++)
        {
            currentGoals[i].thisText.text = "" + levelGoals[i].numberCollected + "/" + levelGoals[i].numberNeeded;
            if(levelGoals[i].numberCollected >= levelGoals[i].numberNeeded)
            {
                goalsCompleted++;
                currentGoals[i].thisText.text = "" + levelGoals[i].numberNeeded + "/" + levelGoals[i].numberNeeded;
            }
        }
        if (goalsCompleted >= levelGoals.Length)
        {
            if (endGameManager != null)
            {
                endGameManager.WinGame();
            }
            Debug.Log("You win!");
        }
    }

    public void CompareGoal(string goalToCompare)
    {
        for (int i = 0; i < levelGoals.Length; i++)
        {
            if (goalToCompare == levelGoals[i].matchValue)
            {
                levelGoals[i].numberCollected++;
            }
        }
    }
}
