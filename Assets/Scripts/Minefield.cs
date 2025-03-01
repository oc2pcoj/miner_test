using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public interface ICellClicked
{
    public void OnCellClick(int x, int y, bool open);
}
public class CellState 
{
    public static int MINE = -1;
    public static int EMPTY = 0;
}

public class Minefield : MonoBehaviour, ICellClicked
{
    [SerializeField] ResultPopup resultPopup;
    [SerializeField] private GameConfig config;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject content;

    private PlayerControls playerControls;
    private Cell[,] minefield = null;
    private bool firstClick = true;
    private Vector2Int firstCell;

    public void OnCellClick(int x, int y, bool open)
    {
        if (open)
        {
            if (minefield[x,y].IsMine())
            {
                minefield[x,y].OpenCell();
                resultPopup.gameObject.SetActive(true);
                resultPopup.SetState(false);
            }
            else
            {
                if (!firstClick)
                {
                    minefield[x,y].OpenCell();
                    OpenCells(x, y);
                } 
                else
                {
                    firstCell = new Vector2Int(x,y);
                    PlaceMines();
                    firstClick = false;             
                }

            }
        }
        CheckVictory();
    }

    public void Restart()
    {
        firstClick = true;
        resultPopup.gameObject.SetActive(false);
        foreach (var cell in minefield)
        {
            cell.ResetCell();
        }
        GenerateMinefield();
    }

    private void Start() 
    {
        playerControls = new PlayerControls();
        playerControls.AnyKey.Enable();
        playerControls.AnyKey.AnyKey.performed += OnKeyPerformed;
        GenerateMinefield();
    }

    private void OnKeyPerformed(InputAction.CallbackContext context)
    {
        Restart();
    }
    private void GenerateMinefield()
    {
        if (config.MinesCount >= config.Width * config.Height) 
        {
            Debug.Log("Wrong config");
            return;
        }
    
        if (minefield != null) return;
        minefield = new Cell[config.Width, config.Height];
        var contentRect = content.GetComponent<RectTransform>(); 
        contentRect.sizeDelta = new Vector3(config.Width * 100f, config.Height * 100f, 0f);
        var gridLayout = content.GetComponent<GridLayoutGroup>();
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = config.Width;

        for (int x = 0; x < config.Width; x++)
        {
            for (int y = 0; y < config.Height; y++)
            {
                var cellObj = Instantiate(cellPrefab, content.transform);
                var cell = cellObj.GetComponent<Cell>();
                cell.InitCell(x, y, this);
                minefield[x,y] = cell;
            }
        }
    }

    private void PlaceMines()
    {
        HashSet<Vector2Int> minesPositions = new HashSet<Vector2Int>();
        System.Random random = new System.Random();

        while (minesPositions.Count < config.MinesCount)
        {
            int x = random.Next(0, config.Width);
            int y = random.Next(0, config.Height);
            Vector2Int pos = new Vector2Int(x, y);

            if (!minesPositions.Contains(pos) && pos != firstCell)
            {
                minesPositions.Add(pos);
                minefield[x, y].SetState(CellState.MINE); 
            }
        }

        for (int x = 0; x < config.Width; x++)
        {
            for (int y = 0; y < config.Height; y++)
            {
                if (minefield[x,y].IsMine()) continue;
                int mineCount = GetMinesCountInNeighbors(x,y);
                minefield[x,y].SetState(mineCount);
            }
        }
        minefield[firstCell.x, firstCell.y].OpenCell();
        if (minefield[firstCell.x, firstCell.y].IsEmpty())
        {
            OpenCells(firstCell.x, firstCell.y);
        }
    }

    private void CheckVictory()
    {
        int markedBombs = 0;
        int openedAmount = 0;
        foreach (var cell in minefield)
        {
            if (cell.IsMarked() && cell.IsMine())
            {
                markedBombs++;
            } 
            else if (cell.Opened())
            {
                openedAmount++;
            } 
        }
        if (markedBombs == config.MinesCount || (config.Width * config.Height - openedAmount) == config.MinesCount)
        {
            resultPopup.gameObject.SetActive(true);
            resultPopup.SetState(true);
        }
    }

    private void OpenCells(int startX, int startY)
    {
        HashSet<Vector2Int> openedCells = new HashSet<Vector2Int>();
        List<Vector2Int> cellsToOpen = new List<Vector2Int>() { new Vector2Int(startX, startY)};
        if (!minefield[startX, startY].IsEmpty())
        {
            minefield[startX, startY].OpenCell();
            return;
        }
        while (cellsToOpen.Count > 0)
        {
            var checkingCell = new Vector2Int(cellsToOpen[0].x, cellsToOpen[0].y);
            for (int i = 0; i < dx.Length; i++)
            {
                int nx = checkingCell.x + dx[i];
                int ny = checkingCell.y + dy[i];
                if (nx >= 0 && nx < config.Width && ny >=0 && ny < config.Height)
                {
                    var aroundCell = new Vector2Int(nx, ny);
                    if (!minefield[nx, ny].IsMine() && !openedCells.Contains(aroundCell))
                    {
                        if (minefield[nx, ny].IsEmpty())
                        {
                            if (!cellsToOpen.Contains(aroundCell))
                            {
                                cellsToOpen.Add(aroundCell);
                            }
                        } 
                        else 
                        {
                            openedCells.Add(aroundCell);
                        }
                        minefield[nx, ny].OpenCell();
                    }
                }
            }
            if (!openedCells.Contains(checkingCell)) openedCells.Add(checkingCell);
            cellsToOpen.RemoveAt(0);
        }
    }

    private int GetMinesCountInNeighbors(int x, int y)
    {
        int mc = 0;
        for (int i = 0; i < dx.Length; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];
            if (nx >= 0 && nx < config.Width && ny >=0 && ny < config.Height)
            {
                if (minefield[nx, ny].IsMine())
                {
                    mc++;
                }
            }
        }
        return mc;
    }
    private int[] dx = {-1, -1, -1, 0, 1, 1, 1, 0};
    private int[] dy = {-1, 0, 1, 1, 1, 0, -1, -1};
}

