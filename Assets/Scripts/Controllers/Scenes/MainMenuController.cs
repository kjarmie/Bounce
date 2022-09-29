using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bounce
{
    public class MainMenuController : MonoBehaviour
    {
        public void OnPlayClicked()
        {
            SceneManager.LoadScene((int)Scenes.Selection);
        }

        public void OnTrainingClicked()
        {
            //SceneManager.LoadScene((int)Scenes.Options);

            //LevelGenerator.LevelGenerator.seed = 35050;
            //LevelGenerator.LevelGenerator.random = new System.Random(35050);
            //LevelGenerator.Phases.P3PlaceSpecialTiles p3 = new LevelGenerator.Phases.P3PlaceSpecialTiles();
            //p3.TrainWFC();
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            System.Random random = new System.Random();
            int seed;
            for (int i = 0; i < 100; i++)
            {
                watch.Reset();
                watch.Start();

                //seed = random.Next();
                seed = 25;
                LevelGenerator.LevelGenerator.GenerateLevel(seed, LevelGenerator.LevelSize.Small);

                watch.Stop();

                Debug.Log("Time elapsed: " + watch.ElapsedMilliseconds + "ms");

                Dictionary<LevelGenerator.Tiles.Tile, int> dict = new Dictionary<LevelGenerator.Tiles.Tile, int>();

                LevelGenerator.Tiles.Tile tile = new LevelGenerator.Tiles.Tile(LevelGenerator.TileArchetype.None, LevelGenerator.TileType.None, 0, 1);
            }



            //p3.Run();

        }

        public void OnQuitClicked()
        {
            Application.Quit();
        }

    }
    public enum Scenes
    {
        MainMenu = 0,
        Selection = 1,
        Play = 2
    }
}

