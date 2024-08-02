using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlineSelectButton : MonoBehaviour
{
    public Button button;

    private OnlineGameController gameController;

    public void SetGrid()
    {
        gameController.SetActiveGridRpc(button.name);
    }

    public void SetGameControllerReference(OnlineGameController controller)
    {
        gameController = controller;
    }
}
