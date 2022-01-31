using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuBehavior : MonoBehaviour
{
    private GameObject Timer;
    private void Start()
    {
        Timer = GameObject.Find("PlayingHUD");
    }
    public void ResumeGame()
    {
        Time.timeScale = 1;
        Timer.SetActive(true);
        gameObject.SetActive(false);

    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
