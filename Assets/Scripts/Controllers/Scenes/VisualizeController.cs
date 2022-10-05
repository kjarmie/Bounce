using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LevelGenerator;

namespace Bounce
{
    public class VisualizeController : MonoBehaviour
    {
        [SerializeField] public Image image;
        Preset preset;
        int cur_seed;
        LevelSize level_size;

        System.Random random;

        void Start()
        {
            preset = Preset.General;
            random = new System.Random();
        }

        public void OnSmallClicked()
        {
            // Change the level size
            level_size = LevelSize.Small;

            // Randomize the seed
            cur_seed = random.Next();

            GenerateAndLoad(cur_seed);
        }

        public void OnMediumClicked()
        {
            // Change the level size
            level_size = LevelSize.Medium;

            // Randomize the seed
            cur_seed = random.Next();

            GenerateAndLoad(cur_seed);
        }

        public void OnLargeClicked()
        {
            // Change the level size
            level_size = LevelSize.Large;

            // Randomize the seed
            cur_seed = random.Next();

            GenerateAndLoad(cur_seed);
        }

        public void OnGrass()
        {
            preset = Preset.Grass;

            RegenAndLoad();
        }
        public void OnCave()
        {
            preset = Preset.Cave;

            RegenAndLoad();
        }
        public void OnDungeon()
        {
            preset = Preset.Dungeon;

            RegenAndLoad();
        }
        public void OnGeneral()
        {
            preset = Preset.General;

            RegenAndLoad();
        }



        private void RegenAndLoad()
        {
            // Get the WFC seed
            int wfc_seed = random.Next();

            // Generate a level
            LevelGenerator.LevelGenerator.ChangePreset(cur_seed, wfc_seed, preset);

            // Display the texture on the screen
            Texture2D texture = new Texture2D(1, 1);
            texture.filterMode = FilterMode.Point;
            texture.LoadImage(File.ReadAllBytes(@"./Assets/Resources/kjarmie/LevelGenerator/outputs/level/level.png"), false);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
        }

        private void GenerateAndLoad(int seed)
        {
            // Generate a level
            LevelGenerator.LevelGenerator.GenerateLevel(seed, level_size, preset);

            // Display the texture on the screen
            Texture2D texture = new Texture2D(1, 1);
            texture.filterMode = FilterMode.Point;
            texture.LoadImage(File.ReadAllBytes(@"./Assets/Resources/kjarmie/LevelGenerator/outputs/level/level.png"), false);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
        }


        public void OnBackClicked()
        {
            SceneManager.LoadScene((int)Scenes.MainMenu);
        }
    }
}
