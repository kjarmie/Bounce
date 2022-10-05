using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LevelGenerator;
using Tarodev;

namespace Bounce
{
    /// <summary>
    /// This class manages the tile assets so that loading is faster.
    /// </summary>
    public class SpriteManager : StaticInstance<SpriteManager>
    {
        public static Dictionary<TileType, Sprite> tile_sprites;
        /// <summary>
        /// When the scene is loaded, load all the assets for the tiles
        /// </summary>
        private void Awake()
        {
            tile_sprites = new Dictionary<TileType, Sprite>();

            string file_path = "kjarmie/Art/Tiles/";
            tile_sprites.Add(TileType.NormalAir, Resources.Load<Sprite>(file_path + "Air/normal_air"));
            tile_sprites.Add(TileType.Flowers, Resources.Load<Sprite>(file_path + "Air/flowers"));
            tile_sprites.Add(TileType.Mushrooms, Resources.Load<Sprite>(file_path + "Air/mushrooms"));
            tile_sprites.Add(TileType.Weeds, Resources.Load<Sprite>(file_path + "Air/weeds"));

            tile_sprites.Add(TileType.Brick, Resources.Load<Sprite>(file_path + "Ground/brick"));
            tile_sprites.Add(TileType.Dirt, Resources.Load<Sprite>(file_path + "Ground/dirt"));
            tile_sprites.Add(TileType.Grass, Resources.Load<Sprite>(file_path + "Ground/grass"));
            tile_sprites.Add(TileType.Stone, Resources.Load<Sprite>(file_path + "Ground/stone"));

            tile_sprites.Add(TileType.BlackRose, Resources.Load<Sprite>(file_path + "Trap/black_rose"));
            tile_sprites.Add(TileType.Boulder, Resources.Load<Sprite>(file_path + "Trap/boulder"));
            tile_sprites.Add(TileType.Spikes, Resources.Load<Sprite>(file_path + "Trap/spikes"));

            tile_sprites.Add(TileType.Chest, Resources.Load<Sprite>(file_path + "Treasure/chest"));
            tile_sprites.Add(TileType.Gold, Resources.Load<Sprite>(file_path + "Treasure/gold"));

            tile_sprites.Add(TileType.House, Resources.Load<Sprite>(file_path + "Start/house"));
            tile_sprites.Add(TileType.Flag, Resources.Load<Sprite>(file_path + "End/flag"));

            tile_sprites.Add(TileType.Skeleton, Resources.Load<Sprite>(file_path + "Air/normal_air"));

            tile_sprites.Add(TileType.None, Resources.Load<Sprite>(file_path + "Ground/stone"));
        }
    }
}
