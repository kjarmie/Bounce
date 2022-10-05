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
        int seed;
        LevelSize level_size;

        void Start()
        {
            preset = Preset.General;
        }

        public void OnMediumClicked()
        {
            // Change the level size
            level_size = LevelSize.Medium;

            // Randomize the seed
            seed = new System.Random().Next();

            GenerateAndLoad(seed);
        }

        public void OnLargeClicked()
        {
            // Change the level size
            level_size = LevelSize.Large;

            // Randomize the seed
            seed = new System.Random().Next();

            GenerateAndLoad(seed);
        }

        public void OnGrass()
        {
            preset = Preset.Grass;

            RegenAndLoad(seed);
        }
        public void OnCave()
        {
            preset = Preset.Cave;

            RegenAndLoad(seed);
        }
        public void OnDungeon()
        {
            preset = Preset.Dungeon;

            RegenAndLoad(seed);
        }
        public void OnGeneral()
        {
            preset = Preset.General;

            RegenAndLoad(seed);
        }

        public void OnSmallClicked()
        {
            // Change the level size
            level_size = LevelSize.Small;

            // Randomize the seed
            seed = new System.Random().Next();

            GenerateAndLoad(seed);
        }

        private void RegenAndLoad(int seed) {
            // Generate a level
            LevelGenerator.LevelGenerator.ChangePreset(seed, preset);

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
