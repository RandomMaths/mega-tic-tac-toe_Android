using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectButton : MonoBehaviour
{
    public Button button;

    private LocalGameController gameController;

    public void SetGrid()
    {
        gameController.SetActiveGrid(button.name);
    }

    public void SetGameControllerReference(LocalGameController controller)
    {
        gameController = controller;
    }
}
