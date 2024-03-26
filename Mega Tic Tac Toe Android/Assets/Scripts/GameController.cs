using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Player
{
    public Image panel;
    public TextMeshProUGUI text;
    public Button button;
}

[System.Serializable]
public class ColorAssigner
{
    public Color panelColor;
    public Color textColor;
}

public class GameController : MonoBehaviour
{
    public TextMeshProUGUI[] buttonList;

    public GameObject[] Grid;
    public Button[] SelectButtons;

    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;

    public Player playerX;
    public Player playerO;

    public ColorAssigner activePlayerColor;
    public ColorAssigner inactivePlayerColor;

    public ColorAssigner activeGridColor;
    public ColorAssigner inactiveGridColor;

    private string playerSide;

    private int[] gridState;
    private Image[] grids;
    private int[,,] cellState;
    private Image[,,] cells;
    private TextMeshProUGUI[,,] text;

    private int activeGrid;

    public void Awake()
    {
        gridState = new int[Grid.Length];
        grids = new Image[Grid.Length];
        cellState = new int[9, 3, 3];
        cells = new Image[9, 3, 3];
        text = new TextMeshProUGUI[9, 3, 3];

        activeGrid = -1;
        playerSide = "";

        gameOverPanel.SetActive(false);

        InitializeCells();
        SetPlayBoardInteractable(false);
        SetBoardInteractable(false);
        SetGameControllerReferenceOnButtons();
    }

    public void FixedUpdate()
    {
        if(activeGrid != -1)
        { 
            SelectButtons[activeGrid].Select();
        }
        else if(playerSide != "")
        {
            SetBoardInteractable(false);
            SetBoardInteractable(true);
        }
    }

    public void FillGrid(string name)
    {
        int grid = GetGrid(name);
        FillGrid(activeGrid, grid % 3, grid / 3);
        CheckGrid(activeGrid);
        activeGrid = grid;
        if (gridState[grid] != 0)
        {
            activeGrid = -1;
        }
        else
        {
            SelectButtons[grid].Select();
            LoadGrid(grid);
        }
    }

    public void SetActiveGrid(string grid)
    {
        if (activeGrid == -1 && gridState[GetGrid(grid)] == 0)
        {
            activeGrid = GetGrid(grid);
            LoadGrid(GetGrid(grid));
        }
    }

    public void SetGameControllerReferenceOnButtons()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<GridSpace>().SetGameControllerReference(this);
            SelectButtons[i].GetComponentInParent<SelectButton>().SetGameControllerReference(this);
        }
    }

    public void LoadGrid(int grid)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            if (cellState[grid, i % 3, i / 3] == 1)
            {
                buttonList[i].text = "X";
                buttonList[i].GetComponentInParent<GridSpace>().button.interactable = false;
            }
            else if(cellState[grid, i % 3, i / 3] == 2) 
            {
                buttonList[i].text = "O";
                buttonList[i].GetComponentInParent<GridSpace>().button.interactable = false;
            }
            else
            {
                buttonList[i].text = "";
                buttonList[i].GetComponentInParent<GridSpace>().button.interactable = true;
            }
        }
    }

    public void SetStartingSide(string startingSide)
    {
        playerSide = startingSide;
        HighlightActiveSide();

        StartGame();
    }

    public void CheckGrid(int grid)
    {
        for(int i = 0;i <= 2; i++)
        {
            if(cellState[grid, i, 0] == GetPlayerState(playerSide) && cellState[grid, i, 1] == GetPlayerState(playerSide) && cellState[grid, i, 2] == GetPlayerState(playerSide))
            {
                ActivateGrid(grid, playerSide);
                gridState[grid] = GetPlayerState(playerSide);
            }

            if (cellState[grid, 0, i] == GetPlayerState(playerSide) && cellState[grid, 1, i] == GetPlayerState(playerSide) && cellState[grid, 2, i] == GetPlayerState(playerSide))
            {
                ActivateGrid(grid, playerSide);
                gridState[grid] = GetPlayerState(playerSide);
            }
        }

        if (cellState[grid, 0, 0] == GetPlayerState(playerSide) && cellState[grid, 1, 1] == GetPlayerState(playerSide) && cellState[grid, 2, 2] == GetPlayerState(playerSide))
        {
            ActivateGrid(grid, playerSide);
            gridState[grid] = GetPlayerState(playerSide);
        }

        if (cellState[grid, 0, 2] == GetPlayerState(playerSide) && cellState[grid, 1, 1] == GetPlayerState(playerSide) && cellState[grid, 2, 0] == GetPlayerState(playerSide))
        {
            ActivateGrid(grid, playerSide);
            gridState[grid] = GetPlayerState(playerSide);
        }

        if (GridComplete(grid))
        {
            ActivateGrid(grid, "-");
            gridState[grid] = 3;
        }

        CheckBoard();

        ChangeSides();
    }

    void CheckBoard()
    {
        for (int i = 0; i <= 2; i++)
        {
            if (gridState[i*3 + 0] == GetPlayerState(playerSide) && gridState[i*3 + 1] == GetPlayerState(playerSide) && gridState[i*3 + 2] == GetPlayerState(playerSide))
            {
                GameOver(playerSide);
            }

            if (gridState[i] == GetPlayerState(playerSide) && gridState[1*3 + i] == GetPlayerState(playerSide) && gridState[3*2 + i] == GetPlayerState(playerSide))
            {
                GameOver(playerSide);
            }
        }

        if (gridState[0] == GetPlayerState(playerSide) && gridState[4] == GetPlayerState(playerSide) && gridState[8] == GetPlayerState(playerSide))
        {
            GameOver(playerSide);
        }

        if (gridState[2] == GetPlayerState(playerSide) && gridState[4] == GetPlayerState(playerSide) && gridState[6] == GetPlayerState(playerSide))
        {
            GameOver(playerSide);
        }
    }

    void ChangeSides()
    {
        playerSide = (playerSide == "X") ? "O" : "X";
        HighlightActiveSide();
    }

    void SetPlayBoardInteractable(bool toggle)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = toggle;
        }
    }

    void SetBoardInteractable(bool toggle)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            SelectButtons[i].interactable = toggle;
        }
    }

    void SetPlayerColors(Player newPlayer, Player oldPlayer)
    {
        newPlayer.panel.color = activePlayerColor.panelColor;
        newPlayer.text.color = activePlayerColor.textColor;
        oldPlayer.panel.color = inactivePlayerColor.panelColor;
        oldPlayer.text.color = inactivePlayerColor.textColor;
    }

    void SetPlayerButtonInteractibility(bool toggle)
    {
        playerX.button.interactable = toggle;
        playerO.button.interactable = toggle;
    }

    void SetGameOverText(string value)
    {
        gameOverPanel.SetActive(true);
        gameOverText.text = value;
    }

    private void StartGame()
    {
        SetBoardInteractable(true);
        SetPlayerButtonInteractibility(false);
    }

    private void GameOver(string winningPlayer)
    {
        SetBoardInteractable(false);
        SetPlayBoardInteractable(false);
        SetGameOverText(winningPlayer + " Wins!");
    }

    private void FillGrid(int grid, int gridX, int gridY)
    {
        if (((grid <= 8) && (grid >= 0)) && (((gridX >= 0) && (gridX <= 2)) && ((gridY >= 0) && (gridY <= 2))))
        {
            cellState[grid, gridX, gridY] = (playerSide == "X") ? 1 : 2;
            cells[grid, gridX, gridY].color = Color.white;
            text[grid, gridX, gridY].text = playerSide;
        }
    }

    private void InitializeCells()
    {
        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                for (int k = 0; k < cells.GetLength(2); k++)
                {
                    cells[i, j, k] = Grid[i].transform.GetChild((k * 3) + j).gameObject.GetComponent<Image>();
                    text[i, j, k] = Grid[i].transform.GetChild((k * 3) + j).gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                }
            }
            grids[i] = Grid[i].transform.GetChild(9).GetComponent<Image>();
            DeactivateGrid(i);
        }
    }

    private void ActivateGrid(int i, string player)
    {
        grids[i].color = activeGridColor.panelColor;
        grids[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = activeGridColor.textColor;

        grids[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = player;
    }

    private void DeactivateGrid(int i)
    {
        grids[i].color = inactiveGridColor.panelColor;
        grids[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = inactiveGridColor.textColor;
    }

    private void HighlightActiveSide()
    {
        if (playerSide == "X")
        {
            SetPlayerColors(playerX, playerO);
        }
        else
        {
            SetPlayerColors(playerO, playerX);
        }
    }

    private bool GridComplete(int grid)
    {
        for(int i = 0;i < cellState.GetLength(1); i++)
        {
            for(int j = 0;j < cellState.GetLength(2); j++)
            {
                if (cellState[grid, i, j] == 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private int GetPlayerState(string player)
    {
        return (player == "X") ? 1 : 2;
    }

    private int GetGrid(string name)
    {
        return int.Parse(name.Substring(name.Length - 1)) - 1;
    }
}
