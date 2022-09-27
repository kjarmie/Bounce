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
            switch (tile.type)
            {
                // Air tiles
                case TileType.NormalAir:
                case TileType.Flowers:
                case TileType.Mushrooms:
                case TileType.Weeds:

                // Ground tiles
                case TileType.Brick:
                case TileType.Dirt:
                case TileType.Grass:
                case TileType.Stone:

                // Trap tiles
                case TileType.BlackRose:
                case TileType.Boulder:
                case TileType.Spikes:

                // Treasure tiles
                case TileType.Gold:
                case TileType.Chest:

                // Enemy tiles
                case TileType.Skeleton:
                    break;

            }

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
