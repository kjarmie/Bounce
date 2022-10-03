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
        [SerializeField] public GameObject display;
        [SerializeField] public Image image;

        Texture2D texture;

        LevelSize size;
        int seed;

        // 

        public void OnMediumClicked() {
            // Set the seed to be random
            int seed = new System.Random().Next();
            size = LevelSize.Medium;

            // Generate a level
            LevelGenerator.LevelGenerator.GenerateLevel(seed, size);

            // Display the texture on the screen
            this.texture = new Texture2D(1, 1);
            this.texture.filterMode = FilterMode.Point;
            this.texture.LoadImage(File.ReadAllBytes(@"./Assets/Resources/kjarmie/LevelGenerator/outputs/level/level.png"), false);
            image.sprite = Sprite.Create(this.texture, new Rect(0, 0, this.texture.width, this.texture.height), new Vector2(0, 0));
        }

        public void OnLargeClicked()
        {
            // Set the seed to be random
            int seed = new System.Random().Next();
            size = LevelSize.Large;

            GenerateAndLoad(seed, LevelSize.Large);
        }

        public void OnSmallClicked()
        {
            // Clear the current sprite            
            SpriteRenderer renderer = display.GetComponent<SpriteRenderer>();
            Resources.UnloadAsset(renderer.sprite);
            Resources.UnloadUnusedAssets();
            renderer.sprite = null;

            // Set the seed to be random
            int seed = new System.Random().Next();
            size = LevelSize.Small;

            // Generate a level
            LevelGenerator.LevelGenerator.GenerateLevel(seed, size);

            // Refresh the directory
            DirectoryInfo directory = new DirectoryInfo(@"./Assets/Resources/kjarmie/LevelGenerator/outputs/level");
            directory.Refresh();

            // Load the texture generated
            // Texture2D texture = Resources.Load<Texture2D>("kjarmie/LevelGenerator/outputs/level/level");
            // this.texture = new Texture2D(texture.width, texture.height);
            // for (int x = 0; x < texture.width; x++)
            // {
            //     for (int y = 0; y < texture.height; y++)
            //     {
            //         this.texture.SetPixel(x, y, texture.GetPixel(x, y));
            //     }
            // }
            // this.texture.Apply();

            // Display the texture on the screen
            Sprite sprite = Resources.Load<Sprite>("kjarmie/LevelGenerator/outputs/level");
            sprite = Resources.Load<Sprite>("kjarmie/LevelGenerator/outputs/level/" + seed + @"/level");
            sprite = Resources.Load<Sprite>("kjarmie/LevelGenerator/outputs/level/level");

            this.texture = new Texture2D(1, 1);
            this.texture.filterMode = FilterMode.Point;
            this.texture.LoadImage(File.ReadAllBytes(@"./Assets/Resources/kjarmie/LevelGenerator/outputs/level/level.png"), false);
            sprite = Sprite.Create(this.texture, new Rect(0, 0, this.texture.width, this.texture.height), new Vector2(0, 0));
            image.sprite = sprite;
            renderer.sprite = sprite;
        }

        private void GenerateAndLoad(int seed, LevelSize level_size) {
            // Generate a level
            LevelGenerator.LevelGenerator.GenerateLevel(seed, level_size);

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
