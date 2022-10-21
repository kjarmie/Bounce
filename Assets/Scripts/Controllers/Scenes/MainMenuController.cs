using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LevelGenerator;

using System.Diagnostics;
using System.IO;

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
            //TODO: Change back to Loading visualize screen

            // // For Small level
            // System.Random random = new System.Random();
            // List<string> small = new List<string>();
            // for (int i = 0; i < 30; i++)
            // {
            //     int seed = random.Next();

            //     Stopwatch stopwatch = new Stopwatch();
            //     stopwatch.Reset();
            //     stopwatch.Restart();


            //     LevelGenerator.LevelGenerator.GenerateLevel(seed, LevelSize.Small, Preset.General);

            //     stopwatch.Stop();

            //     small.Add(seed + "," + stopwatch.ElapsedMilliseconds);

            // }
            // // For Medium level
            // List<string> medium = new List<string>();
            // for (int i = 0; i < 30; i++)
            // {
            //     int seed = random.Next();

            //     Stopwatch stopwatch = new Stopwatch();
            //     stopwatch.Reset();
            //     stopwatch.Restart();


            //     LevelGenerator.LevelGenerator.GenerateLevel(seed, LevelSize.Small, Preset.General);

            //     stopwatch.Stop();

            //     medium.Add(seed + "," + stopwatch.ElapsedMilliseconds);

            // }
            // // For Large level
            // List<string> large = new List<string>();
            // for (int i = 0; i < 30; i++)
            // {
            //     int seed = random.Next();

            //     Stopwatch stopwatch = new Stopwatch();
            //     stopwatch.Reset();
            //     stopwatch.Restart();


            //     LevelGenerator.LevelGenerator.GenerateLevel(seed, LevelSize.Small, Preset.General);

            //     stopwatch.Stop();

            //     large.Add(seed + "," + stopwatch.ElapsedMilliseconds);
            // }

            // StreamWriter _small = new StreamWriter(@"C:\Users\quzei\Desktop\small.txt");
            // StreamWriter _medium = new StreamWriter(@"C:\Users\quzei\Desktop\medium.txt");
            // StreamWriter _large = new StreamWriter(@"C:\Users\quzei\Desktop\large.txt");
            // for (int i = 0; i < 30; i++)
            // {
            //     _small.Write(small[i] + "\n");
            //     _medium.Write(medium[i] + "\n");
            //     _large.Write(large[i] + "\n");
            // }
            // _small.Close();
            // _medium.Close();
            // _large.Close();


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

