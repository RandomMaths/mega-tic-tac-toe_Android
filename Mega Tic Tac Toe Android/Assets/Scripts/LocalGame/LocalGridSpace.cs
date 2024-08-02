using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridSpace : MonoBehaviour
{
    public Button button;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private string playerSide;

    private LocalGameController gameController;

    private void Start()
    {
        
    }

    public void SetSpace()
    {
        label.text = playerSide;
        gameController.FillGrid(button.name);
    }

    public void SetGameControllerReference(LocalGameController controller)
    {
        gameController = controller;
    }
}
