using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move,
    win,
    lose,
    pause
}

public enum TileKind
{
    Breakable,
    Blank,
    Normal
}

[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileKind tileKind;
}

public class Board : MonoBehaviour
{
    [Header ("Scriptable Objects")]
    public World world;
    public int level;

    public GameState currentGameState = GameState.wait;
    [Header ("Board Dimensions")]
    public int width;
    public int height;
    public int offset;

    [Header ("Prefabs")]
    public GameObject tilePrefab;
    public GameObject breakableTilePrefab;
    public GameObject[] dots;
    public GameObject destroyEffect;

    [Header ("Layout")]
    public TileType[] boardLayout;
    private bool[,] blankSpaces;
    private BackgroundTile[,] breakableTiles;
    public GameObject[,] allDots;
    public Dot currentDot;
    private FindMatches findMatches;
    private HintManager hintManager;
    private ScoreManager scoreManager;
    private SoundManager soundManager;
    private GoalManager goalManager;

    public int defaultScoreValue = 20;
    private int streakValue = 1;
    public float refillDelay = 0.5f;
    public float destroyParticleAfterNSeconds = 0.4f;
    public int[] scoreGoals;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("Current Level"))
        {
            level = PlayerPrefs.GetInt("Current Level");
        }

        if (world != null)
        {
            if (level < world.levels.Length)
            {
                if (world.levels[level] != null)
                {
                    width = world.levels[level].width;
                    height = world.levels[level].height;
                    dots = world.levels[level].dots;
                    scoreGoals = world.levels[level].scoreGoals;
                    boardLayout = world.levels[level].boardLayout;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        blankSpaces = new bool[width, height];
        breakableTiles = new BackgroundTile[width, height];
        allDots = new GameObject[width, height];
        findMatches = FindObjectOfType<FindMatches>();
        hintManager = FindObjectOfType<HintManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        soundManager = FindObjectOfType<SoundManager>();
        goalManager = FindObjectOfType<GoalManager>();
        currentGameState = GameState.pause;
        Setup();
    }

    
    public void GenerateBlankSpaces()
    {
        for (int i = 0; i <boardLayout.Length; i++)
        {
            if(boardLayout[i].tileKind == TileKind.Blank)
            {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }
    public void GenerateBreakableTiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Breakable)
            {
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(breakableTilePrefab, tempPosition, Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void Setup()
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offset);
                    Vector2 tilePosition = new Vector2(i, j);
                    GameObject backgroundTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform; // Parent this backgroundTile GameObject to the Board object (to make it tidy in Hierarchy window)
                    backgroundTile.name = "( " + i + ", " + j + " )"; // Name the tiles with their grid position.

                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0; // prevent potential inifinite loop. 
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                    }

                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().column = i;
                    dot.GetComponent<Dot>().row = j;
                    dot.transform.parent = this.transform;
                    dot.name = "( " + i + ", " + j + " )"; // name dot tile with initial grid position.
                    allDots[i, j] = dot;
                }
            }
        }
//        currentGameState = GameState.move;
    }

    private bool MatchesAt(int column, int row, GameObject dot)
    {
        if (column > 1 && row > 1)
        {
  
            if (allDots[column - 1, row]?.tag == dot.tag && allDots[column - 2, row]?.tag == dot.tag)
            {
                return true;
            }

            if (allDots[column, row - 1]?.tag == dot.tag && allDots[column, row - 2]?.tag == dot.tag)
            {
                return true;
            }
        } else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1]?.tag == dot.tag && allDots[column, row - 2]?.tag == dot.tag)
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (allDots[column - 1, row]?.tag == dot.tag && allDots[column - 2, row]?.tag == dot.tag)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool ColumnOrRowMatch5()
    {
        int numberHorizontal = 0;
        int numberVertical = 0;
        Dot firstPiece = findMatches.currentMatches[0].GetComponent<Dot>();
        if (firstPiece != null)
        {
            foreach (GameObject currentPiece in findMatches.currentMatches)
            {
                Dot dot = currentPiece.GetComponent<Dot>();
                if (dot.row == firstPiece.row)
                {
                    numberHorizontal++;
                }
                if (dot.column == firstPiece.column)
                {
                    numberVertical++;
                }
            }
        }
        return (numberVertical == 5 || numberHorizontal == 5);
    }

    private void CheckToMakeBombs()
    {
        if (findMatches.currentMatches.Count == 4 || findMatches.currentMatches.Count == 7)
        {
            findMatches.CheckBombs();
        } else if (findMatches.currentMatches.Count == 5 || findMatches.currentMatches.Count == 8)
        {
            if (ColumnOrRowMatch5()) {
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isColorBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeColorBomb();
                        }
                    } else
                    {
                        if(currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isColorBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeColorBomb();
                                }
                            }
                        }
                    }
                }

            } else
            {
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isAdjacentBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeAdjacentBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isAdjacentBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeAdjacentBomb();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column,row].GetComponent<Dot>().isMatched)
        {
            if (findMatches.currentMatches.Count >= 4)
            {
                CheckToMakeBombs();
            }

            // Does a tile need to break?
            if (breakableTiles[column,row] != null)
            {
                // give 1 damage to tile.
                breakableTiles[column, row].TakeDamage(1);
                if (breakableTiles[column, row].hitPoints <= 0)
                {
                    breakableTiles[column, row] = null;
                }
            }

            if (goalManager != null)
            {
                goalManager.CompareGoal(allDots[column, row].tag.ToString());
                goalManager.UpdateGoals();
            }

            if (hintManager != null)
            {
                hintManager.DestroyHint();
            }

            if (soundManager != null)
            {
                soundManager.PlayRandomDestroyNoise();
            }

            GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            Destroy(particle, destroyParticleAfterNSeconds);
            Destroy(allDots[column, row]);
            scoreManager.IncreaseScore(defaultScoreValue * streakValue);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i,j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        findMatches.currentMatches.Clear();
        StartCoroutine(DecreaseRowCoroutine());
    }

    private IEnumerator DecreaseRowCoroutine()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // if current spot isn't blank and is empty
                if (!blankSpaces[i, j] && allDots[i, j] == null)
                {
                    // loop from the space above to the top of the column
                    for (int k = j + 1; k < height; k++)
                    {
                        // if a dot is found
                        if (allDots[i, k] != null)
                        {
                            // move that dot to this empty space
                            allDots[i, k].GetComponent<Dot>().row = j;
                            allDots[i, k] = null;
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(FillBoardCoroutine());
    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i,j] == null && !blankSpaces[i,j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offset);
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        maxIterations++;
                        dotToUse = Random.Range(0, dots.Length);
                    }

                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = dot;
                    dot.GetComponent<Dot>().column = i;
                    dot.GetComponent<Dot>().row = j;
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i,j] != null)
                {
                    if (allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCoroutine()
    {
        RefillBoard();
        yield return new WaitForSeconds(refillDelay);

        while (MatchesOnBoard())
        {
            streakValue += 1;
            DestroyMatches();
            yield return new WaitForSeconds(2 * refillDelay);
        }
        findMatches.currentMatches.Clear();
        currentDot = null;


        if (IsDeadlocked())
        {
            Debug.Log("Deadlocked!");
            ShuffleBoard();
        }

        yield return new WaitForSeconds(refillDelay);
        currentGameState = GameState.move;
        streakValue = 1;
    }

    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
        allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
        allDots[column, row] = holder;

    }

    private bool CheckForMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i,j] != null)
                {
                    // compare dot with dots in next two columns, so current column must be 2 less than width
                    if (i < width - 2) 
                    {
                        if (allDots[i + 1, j] != null && allDots[i + 2, j] != null)
                        {
                            if (allDots[i + 1, j].tag == allDots[i, j].tag &&
                                allDots[i + 2, j].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }

                    // compare dot with dots in next two rows, so current row must be 2 less than height
                    if (j < height - 2)
                    {
                        if (allDots[i, j + 1] != null && allDots[i, j + 2] != null)
                        {
                            if (allDots[i, j + 1].tag == allDots[i, j].tag &&
                                allDots[i, j + 2].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    public bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        SwitchPieces(column, row, direction);
        if (CheckForMatches())
        {
            SwitchPieces(column, row, direction);
            return true;
        } 

        SwitchPieces(column, row, direction);
        return false;
    }

    private bool IsDeadlocked()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i,j] != null)
                {
                    if (i < width - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.right))
                        {
                            return false;
                        }
                    }

                    if (j < height - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    private void ShuffleBoard()
    {
        List<GameObject> newBoard = new List<GameObject>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i,j] != null)
                {
                    newBoard.Add(allDots[i, j]);
                }
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j])
                {
                    int pieceToUse = Random.Range(0, newBoard.Count);

                    int maxIterations = 0; // prevent potential inifinite loop. 
                    while (MatchesAt(i, j, newBoard[pieceToUse]) && maxIterations < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                    }

                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();

                    piece.column = i;
                    piece.row = j;
                    allDots[i, j] = newBoard[pieceToUse];
                    newBoard.Remove(newBoard[pieceToUse]);
                }
            }
        }

        if (IsDeadlocked())
        {
            ShuffleBoard();
        }
    }
}
