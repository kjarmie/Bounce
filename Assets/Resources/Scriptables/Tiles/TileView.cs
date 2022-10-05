using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LevelGenerator;
using LevelGenerator.Tiles;

namespace Bounce
{
    public class TileView : MonoBehaviour
    {
        // Useful variables
        [SerializeField] SpriteRenderer renderer;

        // Tile Variables
        Tile tile;


        // Game World Variables



        /// <summary>
        /// This method handles the creation of a TileView based on the data in the provided Tile object
        /// </summary>
        /// <param name="tile">The tile object this view represents.</param>
        public void Init(Tile tile)
        {
            // Set the Tile in this view to be the provided Tile
            this.tile = tile;

            // Set the data based on the type
            string directory = "";
            string file_path = "";
            Texture2D new_texture = new Texture2D(0, 0);

            if (tile == null)
            {
                Debug.Log("");
            }

            // Reset the collider 
            this.gameObject.GetComponent<Collider2D>().enabled = true;

            switch (tile.type)
            {
                // Air tiles
                case TileType.NormalAir:
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    break;
                case TileType.Flowers:
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    break;
                case TileType.Mushrooms:
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    break;
                case TileType.Weeds:
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    break;

                // Enemy tiles - these are set to normal air since the game will handle the creation of the enemy AI's
                case TileType.Skeleton:
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
                    break;

                // Start and End tiles
                case TileType.House:
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    break;
                case TileType.Flag:
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    break;

                // If a None tile is given, it is a border tile, so we give it the default image, which is stone.
                case TileType.None:
                    directory = "kjarmie/Art/Tiles/Ground/";
                    file_path = directory + "stone";
                    break;
            }

            // Set the details
            renderer.sprite = SpriteManager.tile_sprites.GetValueOrDefault(tile.type);
            renderer.color = new Color(1, 1, 1, 1);

        }

        // public void Init(Tile tile)
        // {
        //     // Set the Tile in this view to be the provided Tile
        //     this.tile = tile;

        //     // Set the data based on the type
        //     string directory = "";
        //     string file_path = "";
        //     Texture2D new_texture = new Texture2D(0, 0);

        //     if (tile == null)
        //     {
        //         Debug.Log("");
        //     }

        //     // Reset the collider 
        //     this.gameObject.GetComponent<Collider2D>().enabled = true;

        //     switch (tile.type)
        //     {
        //         // Air tiles
        //         case TileType.NormalAir:
        //             directory = "kjarmie/Art/Tiles/Air/";
        //             file_path = directory + "normal_air";
        //             this.gameObject.GetComponent<Collider2D>().enabled = false;
        //             break;
        //         case TileType.Flowers:
        //             directory = "kjarmie/Art/Tiles/Air/";
        //             file_path = directory + "flowers";
        //             this.gameObject.GetComponent<Collider2D>().enabled = false;
        //             break;
        //         case TileType.Mushrooms:
        //             directory = "kjarmie/Art/Tiles/Air/";
        //             file_path = directory + "mushrooms";
        //             this.gameObject.GetComponent<Collider2D>().enabled = false;
        //             break;
        //         case TileType.Weeds:
        //             directory = "kjarmie/Art/Tiles/Air/";
        //             file_path = directory + "weeds";
        //             this.gameObject.GetComponent<Collider2D>().enabled = false;
        //             break;

        //         // Ground tiles
        //         case TileType.Brick:
        //             directory = "kjarmie/Art/Tiles/Ground/";
        //             file_path = directory + "brick";
        //             break;
        //         case TileType.Dirt:
        //             directory = "kjarmie/Art/Tiles/Ground/";
        //             file_path = directory + "dirt";
        //             break;
        //         case TileType.Grass:
        //             directory = "kjarmie/Art/Tiles/Ground/";
        //             file_path = directory + "grass";
        //             break;
        //         case TileType.Stone:
        //             directory = "kjarmie/Art/Tiles/Ground/";
        //             file_path = directory + "stone";
        //             break;

        //         // Trap tiles
        //         case TileType.BlackRose:
        //             directory = "kjarmie/Art/Tiles/Trap/";
        //             file_path = directory + "black_rose";
        //             break;
        //         case TileType.Boulder:
        //             directory = "kjarmie/Art/Tiles/Trap/";
        //             file_path = directory + "boulder";
        //             break;
        //         case TileType.Spikes:
        //             directory = "kjarmie/Art/Tiles/Trap/";
        //             file_path = directory + "spikes";
        //             break;

        //         // Treasure tiles
        //         case TileType.Chest:
        //             directory = "kjarmie/Art/Tiles/Treasure/";
        //             file_path = directory + "chest";
        //             break;
        //         case TileType.Gold:
        //             directory = "kjarmie/Art/Tiles/Treasure/";
        //             file_path = directory + "gold";
        //             break;

        //         // Enemy tiles - these are set to normal air since the game will handle the creation of the enemy AI's
        //         case TileType.Skeleton:
        //             directory = "kjarmie/Art/Tiles/Air/";
        //             file_path = directory + "normal_air";
        //             this.gameObject.GetComponent<Collider2D>().enabled = false;
        //             gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
        //             break;

        //         // Start and End tiles
        //         case TileType.House:
        //             directory = "kjarmie/Art/Tiles/Start/";
        //             file_path = directory + "house";
        //             this.gameObject.GetComponent<Collider2D>().enabled = false;
        //             break;
        //         case TileType.Flag:
        //             directory = "kjarmie/Art/Tiles/End/";
        //             file_path = directory + "flag";
        //             this.gameObject.GetComponent<Collider2D>().enabled = false;
        //             break;

        //         // If a None tile is given, it is a border tile, so we give it the default image, which is stone.
        //         case TileType.None:
        //             directory = "kjarmie/Art/Tiles/Ground/";
        //             file_path = directory + "stone";
        //             break;
        //     }

        //     // Set the details
        //     //new_texture.LoadImage(File.ReadAllBytes(file_path));
        //     Sprite sprite = Resources.Load<Sprite>(file_path);
        //     renderer.sprite = sprite;
        //     //gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
        //     //gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(new_texture, new Rect(0, 0, 320, 320), new Vector2((float)0.5, (float)0.5));
        //     renderer.color = new Color(1, 1, 1, 1);

        // }
    }
}
