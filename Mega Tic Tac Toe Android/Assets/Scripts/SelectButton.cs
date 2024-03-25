using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectButton : MonoBehaviour
{
    public Button button;

    private GameController gameController;

    public void SetGrid()
    {
        gameController.SetActiveGrid(button.name);
    }

    public void SetGameControllerReference(GameController controller)
    {
        gameController = controller;
    }
}
