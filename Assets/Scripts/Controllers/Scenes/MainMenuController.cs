using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LevelGenerator;

namespace Bounce
{
    public class MainMenuController : MonoBehaviour
    {
        public void OnPlayClicked()
        {
            SceneManager.LoadScene((int)Scenes.Selection);
        }

        public void OnVisualizeClicked()
        {
            SceneManager.LoadScene((int)Scenes.Visualize);
        }

        public void OnQuitClicked()
        {
            Application.Quit();
        }

    }

    /// <summary>
    /// This enum holds the various scenes used in the application. 
    /// </summary>
    public enum Scenes
    {
        MainMenu = 0,
        Selection = 1,
        Play = 2,
        Visualize = 3
    }
}

