using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuBtns : MonoBehaviour
{
    public GameObject tut;
    public void StartGameLevel()
    {
        SceneManager.LoadScene(1);
    }

    public void StartTut()
    {
        tut.SetActive(true);
    }

    public void ExitTut()
    {
        tut.SetActive(false);
    }
}
