using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OnlineGameController : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI[] buttonList;

    [SerializeField] private GameObject[] Grid;
    [SerializeField] private Button[] SelectButtons;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private TextMeshProUGUI gameOverText;

    [SerializeField] private Player playerX;
    [SerializeField] private Player playerO;

    [SerializeField] private ColorAssigner activePlayerColor;
    [SerializeField] private ColorAssigner inactivePlayerColor;

    [SerializeField] private ColorAssigner activeGridColor;
    [SerializeField] private ColorAssigner inactiveGridColor;

    private string hostSide;
    private string currentSide;
    private string winningPlayer;

    public List<int> gridState;
    public List<int> cellState;
    
    private Image[] grids;
    private Image[,,] cells;
    private TextMeshProUGUI[,,] text;

    private int activeGrid;

    public static bool isHost;

    private void Awake()
    {
        gridState = new List<int>();
        cellState = new List<int>();
        grids = new Image[Grid.Length];
        cells = new Image[9, 3, 3];
        text = new TextMeshProUGUI[9, 3, 3];

        activeGrid = -1;
        hostSide = "";
        currentSide = "";
        winningPlayer = "";

        gameOverPanel.SetActive(false);
        restartButton.SetActive(false);

        for (int i = 0;i < grids.Length; i++)
            gridState.Add(0);
        for(int i = 0;i < cells.Length; i++)
            cellState.Add(0);

        InitializeCells();
        SetPlayBoardInteractable(false);
        SetBoardInteractable(false);
        SetGameControllerReferenceOnButtons();
    }

    private void SelectGrid(int grid)
    {
        for(int i = 0;i < grids.Length;i++)
            if (gridState[i] == 0)
                grids[i].color = Color.clear;

        if(gridState[grid] != 0)
        {
            Debug.Log("CurrentSide:" + currentSide + "\tHostSide:" + hostSide + "\tIs Host?:" + isHost);
            if (isHost && currentSide.Equals(hostSide))
                SetBoardInteractable(true);
            else if (!isHost && !currentSide.Equals(hostSide))
                SetBoardInteractable(true);
            else
                SetBoardInteractable(false);
        }
        if (gridState[grid] == 0 && grid != -1)
        {
            SetBoardInteractable(false);
            grids[grid].color = new Color(255, 255, 255, 0.2f);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void FillGridRpc(string name)
    {

        int grid = GetGrid(name);
        FillGrid(activeGrid, grid % 3, grid / 3);
        CheckGrid(activeGrid);
        activeGrid = grid;
        if (!winningPlayer.Equals(""))
            return;
        ChangeSides();
        LoadGrid(grid);
        SelectGrid(grid);

        if (gridState[grid] != 0 && winningPlayer == "")
        {
            activeGrid = -1;
            SetPlayBoardInteractable(false);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SetActiveGridRpc(string grid)
    {
        if (activeGrid == -1 && gridState[GetGrid(grid)] == 0)
        {
            activeGrid = GetGrid(grid);
            LoadGrid(GetGrid(grid));
            SelectGrid(GetGrid(grid));
        }
    }

    public void SetGameControllerReferenceOnButtons()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<OnlineGridSpace>().SetGameControllerReference(this);
            SelectButtons[i].GetComponentInParent<OnlineSelectButton>().SetGameControllerReference(this);
        }
    }

    public void LoadGrid(int grid)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            if (cellState[((grid % 3) * 3) + ((grid / 3) * 27) + (i % 3) + ((i / 3) * 9)] == 1)
            {
                buttonList[i].text = "X";
                buttonList[i].GetComponentInParent<OnlineGridSpace>().button.interactable = false;
            }
            else if(cellState[((grid % 3) * 3) + ((grid / 3) * 27) + (i % 3) + ((i / 3) * 9)] == 2) 
            {
                buttonList[i].text = "O";
                buttonList[i].GetComponentInParent<OnlineGridSpace>().button.interactable = false;
            }
            else
            {
                buttonList[i].text = "";
                if(isHost && hostSide.Equals(currentSide))
                    buttonList[i].GetComponentInParent<OnlineGridSpace>().button.interactable = true;
                else if(!isHost && !hostSide.Equals(currentSide))
                    buttonList[i].GetComponentInParent<OnlineGridSpace>().button.interactable = true;
                else
                    buttonList[i].GetComponentInParent<OnlineGridSpace>().button.interactable = false;
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SetStartingSideRpc(string startingSide)
    {
        hostSide = startingSide;
        currentSide = startingSide;
        HighlightActiveSide();

        StartGame();
    }

    [Rpc(SendTo.Everyone)]
    public void RestartGameRpc()
    {
        activeGrid = -1;
        currentSide = "";
        winningPlayer = "";

        for (int i = 0; i < cells.GetLength(0); i++)
        {
            buttonList[i].text = "";
            gridState[i] = 0;
            DeactivateGrid(i);

            for (int j = 0; j < cells.GetLength(1); j++)
            {
                for (int k = 0; k < cells.GetLength(2); k++)
                {
                    cellState[(i * 9) + (j * 3) + k] = 0;
                    cells[i, j, k].color = activePlayerColor.panelColor;
                    text[i, j, k].text = "";
                }
            }
        }

        gameOverPanel.SetActive(false);
        restartButton.SetActive(false);

        if (isHost)
            SetPlayerButtonInteractibility(true);
        else
            SetPlayerButtonInteractibility(false);
        SetPlayBoardInteractable(false);
        SetBoardInteractable(false);
        SetPlayerColors(playerX, playerX);
        SetPlayerColors(playerO, playerO);
    }

    private void CheckGrid(int grid)
    {
        int currentGrid;
        for(int i = 0;i <= 2; i++)
        {
            currentGrid = (grid % 3) * 3 + (grid / 3) * 27 + (i * 9);
            if (cellState[currentGrid + 0] == GetPlayerState(currentSide) && cellState[currentGrid + 1] == GetPlayerState(currentSide) && cellState[currentGrid + 2] == GetPlayerState(currentSide))
            {
                ActivateGrid(grid, currentSide);
                gridState[grid] = GetPlayerState(currentSide);
            }

            currentGrid = (grid / 3) * 27 + (grid % 3) * 3 + i;
            if (cellState[currentGrid + 0*9] == GetPlayerState(currentSide) && cellState[currentGrid + 1*9] == GetPlayerState(currentSide) && cellState[currentGrid + 2*9] == GetPlayerState(currentSide))
            {
                ActivateGrid(grid, currentSide);
                gridState[grid] = GetPlayerState(currentSide);
            }
        }

        if (cellState[(grid / 3) * 27 + (grid % 3) * 3 + 0 + (0) * 9] == GetPlayerState(currentSide) && cellState[(grid / 3) * 27 + (grid % 3) * 3 + 1 + (1) * 9] == GetPlayerState(currentSide) && cellState[(grid / 3) * 27 + (grid % 3) * 3 + 2 + (2) * 9] == GetPlayerState(currentSide))
        {
            ActivateGrid(grid, currentSide);
            gridState[grid] = GetPlayerState(currentSide);
        }

        if (cellState[(grid / 3) * 27 + (grid % 3) * 3 + 2 + (0) * 9] == GetPlayerState(currentSide) && cellState[(grid / 3) * 27 + (grid % 3) * 3 + 1 + (1) * 9] == GetPlayerState(currentSide) && cellState[(grid / 3) * 27 + (grid % 3) * 3 + 0 + (2) * 9] == GetPlayerState(currentSide))
        {
            ActivateGrid(grid, currentSide);
            gridState[grid] = GetPlayerState(currentSide);
        }

        if (GridComplete(grid))
        {
            ActivateGrid(grid, "-");
            gridState[grid] = 3;
        }

        CheckBoard();
    }

    private void CheckBoard()
    {
        for (int i = 0; i <= 2; i++)
        {
            if (gridState[i*3 + 0] == GetPlayerState(currentSide) && gridState[i*3 + 1] == GetPlayerState(currentSide) && gridState[i*3 + 2] == GetPlayerState(currentSide))
            {
                GameOver(currentSide);
            }

            if (gridState[i] == GetPlayerState(currentSide) && gridState[1*3 + i] == GetPlayerState(currentSide) && gridState[3*2 + i] == GetPlayerState(currentSide))
            {
                GameOver(currentSide);
            }
        }

        if (gridState[0] == GetPlayerState(currentSide) && gridState[4] == GetPlayerState(currentSide) && gridState[8] == GetPlayerState(currentSide))
        {
            GameOver(currentSide);
        }

        if (gridState[2] == GetPlayerState(currentSide) && gridState[4] == GetPlayerState(currentSide) && gridState[6] == GetPlayerState(currentSide))
        {
            GameOver(currentSide);
        }
    }
    private void ChangeSides()
    {
        currentSide = (currentSide == "X") ? "O" : "X";
        HighlightActiveSide();
    }

    private void SetPlayBoardInteractable(bool toggle)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = toggle;
        }
    }

    private void SetBoardInteractable(bool toggle)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            SelectButtons[i].interactable = toggle;
        }
    }

    private void SetPlayerColors(Player newPlayer, Player oldPlayer)
    {
        newPlayer.panel.color = activePlayerColor.panelColor;
        newPlayer.text.color = activePlayerColor.textColor;
        oldPlayer.panel.color = inactivePlayerColor.panelColor;
        oldPlayer.text.color = inactivePlayerColor.textColor;
    }

    private void SetPlayerButtonInteractibility(bool toggle)
    {
        playerX.button.interactable = toggle;
        playerO.button.interactable = toggle;
    }

    private void SetGameOverText(string value)
    {
        gameOverPanel.SetActive(true);
        gameOverText.text = value;
    }

    private void StartGame()
    {
        if(isHost) SetBoardInteractable(true);
        SetPlayerButtonInteractibility(false);
    }

    public void GameOver(string winningPlayer)
    {
        this.winningPlayer = winningPlayer;

        restartButton.SetActive(true);
        SetBoardInteractable(false);
        SetPlayBoardInteractable(false);
        if(hostSide.Equals(winningPlayer) && isHost)
        {
            SetGameOverText("You Win!");
            isHost = true;
        }
        else if(!hostSide.Equals(winningPlayer) && !isHost)
        {
            SetGameOverText("You Win!");
            isHost = true;
        }
        else
        {
            SetGameOverText("You Lost");
            isHost = false;
        }
    }

    private void FillGrid(int grid, int gridX, int gridY)
    {
        if (((grid <= 8) && (grid >= 0)) && (((gridX >= 0) && (gridX <= 2)) && ((gridY >= 0) && (gridY <= 2))))
        {
            cellState[(grid % 3) * 3 + (grid / 3) * 27 + gridX + gridY * 9] = (currentSide == "X") ? 1 : 2;
            cells[grid, gridX, gridY].color = Color.white;
            text[grid, gridX, gridY].text = currentSide;
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

        grids[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
    }

    private void HighlightActiveSide()
    {
        if (currentSide == "X")
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
        for(int i = 0;i < buttonList.Length; i++)
        {
            if (cellState[((grid % 3) * 3) + ((grid / 3) * 27) + (i % 3) + ((i / 3) * 9)] == 0)
            {
                return false;
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
