using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnlineGridSpace : MonoBehaviour
{
    public Button button;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private string playerSide;

    private OnlineGameController gameController;

    private void Start()
    {
        
    }

    public void SetSpace()
    {
        label.text = playerSide;
        gameController.FillGridRpc(button.name);
    }

    public void SetGameControllerReference(OnlineGameController controller)
    {
        gameController = controller;
    }
}
