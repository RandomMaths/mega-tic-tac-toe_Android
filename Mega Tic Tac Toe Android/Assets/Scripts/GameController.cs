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
}

[System.Serializable]
public class PlayerColor
{
    public Color panelColor;
    public Color textColor;
}

public class GameController : MonoBehaviour
{
    public TextMeshProUGUI[] buttonList;

    public GameObject[] Grid;
    public Button[] SelectButtons;

    public Player playerX;
    public Player playerO;
    public PlayerColor activePlayerColor;
    public PlayerColor inactivePlayerColor;

    private string playerSide;

    private int[,,] grids;
    private Image[,,] cells;
    private TextMeshProUGUI[,,] text;

    private int activeGrid;

    //[SerializeField]
    //public int grid;
    //public int gridX;
    //public int gridY;

    public void Awake()
    {
        grids = new int[9, 3, 3];
        cells = new Image[9, 3, 3];
        text = new TextMeshProUGUI[9, 3, 3];
        activeGrid = -1;

        //temporary
        playerSide = "X";
        SetPlayerColors(playerX, playerO);

        InitializeCells();
        SetPlayBoardInteractable(false);
        SetGameControllerReferenceOnButtons();
    }

    public void FixedUpdate()
    {
        if(activeGrid != -1)
        { 
            SelectButtons[activeGrid].Select();
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

    private void FillGrid(int grid, int gridX, int gridY)
    {
        if (((grid <= 8) && (grid >= 0)) && (((gridX >= 0) && (gridX <= 2)) && ((gridY >= 0) && (gridY <= 2))))
        {
            grids[grid , gridX, gridY] = (playerSide == "X") ? 1 : 2;
            cells[grid , gridX, gridY].color = Color.white;
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
        }
    }

    public void LoadGrid(string name)
    {
        int grid = GetGrid(name);

        for (int i = 0; i < buttonList.Length; i++)
        {
            if (grids[grid, i % 3, i / 3] == 1)
            {
                buttonList[i].text = "X";
                buttonList[i].GetComponentInParent<GridSpace>().button.interactable = false;
            }
            else if(grids[grid, i % 3, i / 3] == 2) 
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

    void ChangeSides()
    {
        playerSide = (playerSide == "X") ? "O" : "X";

        if(playerSide == "X")
        {
            SetPlayerColors(playerX, playerO);
        }
        else
        {
            SetPlayerColors(playerO, playerX);
        }
    }

    private static int GetGrid(string name)
    {
        return int.Parse(name.Substring(name.Length - 1)) - 1;
    }

    void SetBoardInteractable(bool toggle)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            SelectButtons[i].interactable = toggle;
        }
    }

    void SetPlayBoardInteractable(bool toggle)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = toggle;
        }
    }

    void SetPlayerColors(Player newPlayer, Player oldPlayer)
    {
        newPlayer.panel.color = activePlayerColor.panelColor;
        newPlayer.text.color = activePlayerColor.textColor;
        oldPlayer.panel.color = inactivePlayerColor.panelColor;
        oldPlayer.text.color = inactivePlayerColor.textColor;
    }

    public void FillGrid(string name)
    {
        int grid = GetGrid(name);
        FillGrid(activeGrid, grid % 3, grid / 3);
        activeGrid = grid;
        SelectButtons[grid].Select();
        ChangeSides();
    }

    public void SetActiveGrid(string grid)
    {
        if(activeGrid == -1)
        {
            activeGrid = GetGrid(grid);
            LoadGrid(grid);
        }
    }
}
