using System;
using System.IO;
using LevelGenerator.Tiles;
using LevelGenerator;


namespace Bounce
{
    /// <summary>
    /// This class contains the information needed for playing the game. It stores the data like the terrain, the player details, etc.
    /// </summary>
    internal static class GameLevel
    {
        // Terrain Information
        public static Tile[,] level_grid;     // the entire terrain grid
        public static int rows;
        public static int cols;
        public static Tile start_tile;        // the start tile
        public static Tile end_tile;          // the end tile

        // Player Information


        // 



        /// <summary>
        /// This method will create a new game level from a file. The file must be in the required format.
        /// </summary>
        /// <param name="file_path">The full location of the file. </param>
        public static void CreateGameLevelFromFile(String file_path)
        {
            // Load the file
            StreamReader reader = new StreamReader(file_path);
            string new_file = file_path;

            // Read the data into the level_grid
            // Determine the dimensions
            string line = "";
            while (!reader.EndOfStream)
            {
                // Read a line
                line = reader.ReadLine(); // get a line

                // Get the number of columns
                cols = line.Length;

                // Increment rows
                rows++;
            }
            cols = line.Length;

            // Now, load the data into memory
            reader = new StreamReader(file_path);
            int i = 0;
            string[] input;
            while (!reader.EndOfStream)
            {
                // Load a line
                line = reader.ReadLine();

                // Get a string array
                input = line.Split(",");

                // Read into a row
                for (int j = 0; j < line.Length; j++)
                {
                    // Get the char
                    char _archetype = input[j][0];  // the archetype is the first char
                    char _type = input[j][1];  // the type is the second char

                    // Get the archetype
                    TileArchetype archetype = (TileArchetype)_archetype;

                    // Get the type
                    TileType type = (TileType)_type;

                    // Create a new tile
                    Tile tile = new Tile(archetype, type, i, j);

                    // Add it to the grid
                    level_grid[i, j] = tile;
                }

                // Increment the row
                i++;
            }
            reader.Close();

        }
    }
}