using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] GameObject pause_panel;
    [SerializeField] GameObject pause_button;

    private void Start() {
        pause_panel.SetActive(false);
    }

    public void OnPauseClicked()
    {
        // Run pause menu from Brackeys
        pause_panel.SetActive(true);
        pause_button.SetActive(false);

        Time.timeScale = 0f;
    }

    public void OnResumeClicked()
    { // continue the game
        pause_panel.gameObject.SetActive(false);
        pause_button.gameObject.SetActive(true);

        Time.timeScale = 1f;
    }

    public void OnRestartClicked()
    { // move the player back to the beginning of the level

    }

    public void OnExitClicked()
    {   // exit to the main menu
        int mainMenu = (int)MainMenuController.Scenes.MainMenu;

        SceneManager.LoadScene(mainMenu);
    }
}