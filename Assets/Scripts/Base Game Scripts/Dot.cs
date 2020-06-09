using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{

    public int column;
    public int row;
    public int targetX;
    public int targetY;
    public int originalColumn;
    public int originalRow;
    public bool isMatched = false;

    private EndGameManager endGameManager;
    private FindMatches findMatches;
    private Board board;
    public GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;
    public float swipeAngle = 0;
    public float swipeResist = 1f;
    public float lerpSpeed = 0.6f;
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isColorBomb;
    public bool isAdjacentBomb;
    public GameObject columnArrow;
    public GameObject rowArrow;
    public GameObject colorBomb;
    public GameObject adjacentBomb;
    
    // Start is called before the first frame update
    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;
        board = FindObjectOfType<Board>(); // there will only ever be one object of type Board.
        findMatches = FindObjectOfType<FindMatches>();
        endGameManager = FindObjectOfType<EndGameManager>();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentBomb, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
    }

    public void MakeRowBomb()
    {
        if (!isColumnBomb && !isColorBomb && !isAdjacentBomb)
        {
            isRowBomb = true;
            GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }
    }

    public void MakeColumnBomb()
    {
        if (!isRowBomb && !isColorBomb && !isAdjacentBomb)
        {
            isColumnBomb = true;
            GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }
    }

    public void MakeColorBomb()
    {
        if (!isColumnBomb && !isRowBomb && !isAdjacentBomb)
        {
            isColorBomb = true;
            GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
            color.transform.parent = this.transform;
            this.gameObject.tag = "Color";
        }
    }

    public void MakeAdjacentBomb()
    {
        if (!isColumnBomb && !isColorBomb && !isRowBomb)
        {
            isAdjacentBomb = true;
            GameObject adjacent = Instantiate(adjacentBomb, transform.position, Quaternion.identity);
            adjacent.transform.parent = this.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        targetX = column;
        targetY = row;
        if (Mathf.Abs(targetX - transform.position.x) > 0.1f)
        {
            // Move toward the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, lerpSpeed);
            if (board.allDots[column,row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
                findMatches.FindAllMatches();
            }
        } else
        {
            // Directly set the position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
            
        }

        if (Mathf.Abs(targetY - transform.position.y) > 0.1f)
        {
            // Move toward the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, lerpSpeed);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
                findMatches.FindAllMatches();
            }
        }
        else
        {
            // Directly set the position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }

    public IEnumerator CheckMoveCoroutine()
    {
        if (isColorBomb)
        {
            findMatches.MatchesPiecesOfColor(otherDot.tag);
            isMatched = true;
        } else if (otherDot.GetComponent<Dot>().isColorBomb)
        {
            findMatches.MatchesPiecesOfColor(this.gameObject.tag);
            otherDot.GetComponent<Dot>().isMatched = true;
        }

        board.currentDot = this;
        yield return new WaitForSeconds(board.refillDelay);
        if (otherDot != null)
        {
                
            if (!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                otherDot.GetComponent<Dot>().row = row;
                otherDot.GetComponent<Dot>().column = column;
                row = originalRow;
                column = originalColumn;
                yield return new WaitForSeconds(board.refillDelay);
                board.currentDot = null;
                board.currentGameState = GameState.move;
            } else
            {
                if (endGameManager != null)
                {
                    if (endGameManager.requirements.gameType == GameType.Moves)
                    {
                        endGameManager.DecreaseCounterValue();
                    }
                }
                board.DestroyMatches();
            }

        }
    }

    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (board.currentGameState == GameState.move) { 
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist ||
                    Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
            {
                board.currentGameState = GameState.wait;
                originalColumn = column;
                originalRow = row;
                CalculateAngle();
                MovePieces();
            }
        }
    }

    void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
    }

    void MoveDots(Vector2 direction)
    {
        otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];
        otherDot.GetComponent<Dot>().column += -1 * (int)direction.x;
        otherDot.GetComponent<Dot>().row += -1 * (int)direction.y;
        column += (int)direction.x;
        row += (int)direction.y;
        StartCoroutine(CheckMoveCoroutine());
    }

    void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            // Right swipe
            MoveDots(Vector2.right);
        } else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            // Up swipe
            MoveDots(Vector2.up);
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            // Left swipe
            MoveDots(Vector2.left);
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            // Down swipe
            MoveDots(Vector2.down);
        }
    }

}
