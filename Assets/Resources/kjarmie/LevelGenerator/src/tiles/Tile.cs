using System;
using LevelGenerator;

namespace LevelGenerator.Tiles
{
    /// <summary>
    /// This is the base class for the Level Generator tile. It holds the tile archetype and type, and is used to store information about a tile 
    /// in the game. Note, this is not used in the LevelGenerator.
    /// </summary>
    public class Tile
    {
        internal TileArchetype archetype;    // the archetype this tile is based on (eg. ground, air, trap)
        internal TileType type;       // this is used to represent a tile in the output grid

        public int row;    // the row of the tile in the grid
        public int col;    // the col of the tile in the grid

        public Tile(TileArchetype archetype, TileType type, int row, int col)
        {
            this.archetype = archetype;
            this.type = type;

            this.row = row;
            this.col = col;
        }

        public Tile(int row, int col)
        {
            archetype = TileArchetype.None;
            type = TileType.None;

            this.row = row;
            this.col = col;
        }
    }
}