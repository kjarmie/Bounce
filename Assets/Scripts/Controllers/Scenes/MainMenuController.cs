using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void OnPlayClicked()
    {
        SceneManager.LoadScene((int)Scenes.Play);
    }

    public void OnTrainingClicked()
    {
        //SceneManager.LoadScene((int)Scenes.Options);

        //LevelGenerator.LevelGenerator.seed = 35050;
        //LevelGenerator.LevelGenerator.random = new System.Random(35050);
        //LevelGenerator.Phases.P3PlaceSpecialTiles p3 = new LevelGenerator.Phases.P3PlaceSpecialTiles();
        //p3.TrainWFC();
        int seed = new System.Random().Next();
        seed = 35050;
        LevelGenerator.LevelGenerator.GenerateLevel(seed, LevelGenerator.LevelSize.Small);

        //p3.Run();

    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }

    public enum Scenes
    {
        MainMenu = 0,
        Play = 1,
        Options = 2
    }
}
