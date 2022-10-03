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

            switch (tile.type)
            {
                // Air tiles
                case TileType.NormalAir:
                    directory = "kjarmie/Art/Tiles/Air/";
                    file_path = directory + "normal_air";
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    break;
                case TileType.Flowers:
                    directory = "kjarmie/Art/Tiles/Air/";
                    file_path = directory + "flowers";
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    break;
                case TileType.Mushrooms:
                    directory = "kjarmie/Art/Tiles/Air/";
                    file_path = directory + "mushrooms";
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    break;
                case TileType.Weeds:
                    directory = "kjarmie/Art/Tiles/Air/";
                    file_path = directory + "weeds";
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    break;

                // Ground tiles
                case TileType.Brick:
                    directory = "kjarmie/Art/Tiles/Ground/";
                    file_path = directory + "brick";
                    break;
                case TileType.Dirt:
                    directory = "kjarmie/Art/Tiles/Ground/";
                    file_path = directory + "dirt";
                    break;
                case TileType.Grass:
                    directory = "kjarmie/Art/Tiles/Ground/";
                    file_path = directory + "grass";
                    break;
                case TileType.Stone:
                    directory = "kjarmie/Art/Tiles/Ground/";
                    file_path = directory + "stone";
                    break;

                // Trap tiles
                case TileType.BlackRose:
                    directory = "kjarmie/Art/Tiles/Trap/";
                    file_path = directory + "black_rose";
                    break;
                case TileType.Boulder:
                    directory = "kjarmie/Art/Tiles/Trap/";
                    file_path = directory + "boulder";
                    break;
                case TileType.Spikes:
                    directory = "kjarmie/Art/Tiles/Trap/";
                    file_path = directory + "spikes";
                    break;

                // Treasure tiles
                case TileType.Chest:
                    directory = "kjarmie/Art/Tiles/Treasure/";
                    file_path = directory + "chest";
                    break;
                case TileType.Gold:
                    directory = "kjarmie/Art/Tiles/Treasure/";
                    file_path = directory + "gold";
                    break;

                // Enemy tiles - these are set to normal air since the game will handle the creation of the enemy AI's
                case TileType.Skeleton:
                    directory = "kjarmie/Art/Tiles/Air/";
                    file_path = directory + "normal_air";
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
                    break;

                // Start and End tiles
                case TileType.House:
                    directory = "kjarmie/Art/Tiles/Start/";
                    file_path = directory + "house";
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    break;
                case TileType.Flag:
                    directory = "kjarmie/Art/Tiles/End/";
                    file_path = directory + "flag";
                    this.gameObject.GetComponent<Collider2D>().enabled = false;
                    break;

                // If a None tile is given, it is a border tile, so we give it the default image, which is stone.
                case TileType.None:
                    directory = "kjarmie/Art/Tiles/Ground/";
                    file_path = directory + "stone";
                    break;
            }

            // Set the details
            //new_texture.LoadImage(File.ReadAllBytes(file_path));
            Sprite sprite = Resources.Load<Sprite>(file_path);
            renderer.sprite = sprite;
            //gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
            //gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(new_texture, new Rect(0, 0, 320, 320), new Vector2((float)0.5, (float)0.5));
            renderer.color = new Color(1, 1, 1, 1);

        }

        // public void Init(Tile tile)
        // {
        //     // Here, manage the setup of the TileView
        //     // System.Random random = new System.Random();
        //     // if (random.Next() > 0.4)
        //     //     GetComponent<Renderer>().material.color = Color.red;

        //     if (tile is AirTile)
        //     {
        //         // First check if the tile is the start or end
        //         if (tile.isStartTile)
        //         {
        //             // Colour the tile blue
        //             this.gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 0, 255, 0.2f);
        //         }
        //         else if (tile.isEndTile)
        //         {
        //             // Colour the tile red
        //             this.gameObject.GetComponent<SpriteRenderer>().color = new Color(255, 0, 0, 0.2f);
        //         }
        //         else
        //         {
        //             // Simply remove the colour
        //             //this.gameObject.SetActive(false);
        //             this.gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        //         }
        //         // Disable the colider
        //         this.gameObject.GetComponent<Collider2D>().enabled = false;
        //     }
        //     else if (tile is GroundTile)
        //     {
        //         // For a ground tile, assign the ground image
        //         string directory = @".\\Assets\\Resources\\data\\tiles\\ground\\";

        //         Texture2D new_texture = new Texture2D(100, 100);
        //         new_texture.LoadImage(File.ReadAllBytes(directory + "dirt.png"));

        //         gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(new_texture, new Rect(0, 0, 100, 100), new Vector3((float)0.5, (float)0.5, 0));


        //         // TODO: Check for material type, and assign based on that rather than tile type
        //         switch ((Tile.TileType)tile.GetTileType())
        //         {
        //             case Tile.TileType.Ground:
        //                 break;
        //             default:
        //                 break;
        //         }
        //     }
        //     else if (tile is TrapTile)
        //     {

        //         // TODO: Check for material type, and assign based on that rather than tile type
        //     }
        // }

        public void Dummy()
        {
            // For a ground tile, assign the ground image
            string directory = @".\\Assets\\Resources\\data\\tiles\\ground\\";

            Texture2D new_texture = new Texture2D(100, 100);
            new_texture.LoadImage(File.ReadAllBytes(directory + "dirt.png"));

            gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(new_texture, new Rect(0, 0, 100, 100), new Vector3((float)0.5, (float)0.5, 0));

        }
    }
}
