using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LevelGenerator;

namespace Bounce
{
    public class SelectionController : MonoBehaviour
    {
        public void OnSmallClicked()
        {
            // Load a small level
            GameManager.level_size = LevelGenerator.LevelSize.Small;
            SceneManager.LoadScene((int)Scenes.Play);
        }

        public void OnMediumClicked()
        {
            // Load a medium level
            GameManager.level_size = LevelGenerator.LevelSize.Medium;
            SceneManager.LoadScene((int)Scenes.Play);
        }

        public void OnLargeClicked()
        {
            // Load a large level
            GameManager.level_size = LevelGenerator.LevelSize.Large;
            SceneManager.LoadScene((int)Scenes.Play);
        }

        public void OnBackClicked()
        {
            Application.Quit();
        }

    }
}

