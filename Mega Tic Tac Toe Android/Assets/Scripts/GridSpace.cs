using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridSpace : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI label;
    public string playerSide;

    private GameController gameController;

    private void Start()
    {
        
    }

    public void SetSpace()
    {
        label.text = playerSide;
        gameController.FillGrid(button.name);
        gameController.LoadGrid(button.name);
    }

    public void SetGameControllerReference(GameController controller)
    {
        gameController = controller;
    }
}
