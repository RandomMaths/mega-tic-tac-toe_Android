using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void LoadLocal()
    {
        SceneManager.LoadScene("Local");
    }

    public void LoadOnline()
    {
        SceneManager.LoadScene("Online");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
