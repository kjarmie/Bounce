using System;
using System.Collections.Generic;
using LevelGenerator.Phases;
using System.IO;
using UnityEngine;

namespace LevelGenerator
{
    public static class LevelGenerator
    {
        // The Level Generator is responsible for generating a single level
        // The output is a grid of numbers in a text file, where each number represents a specific type of tile

        // The process is broken down into 4 phases
        // 1. Generate Paths
        // 2. Generate Sections
        // 3. Place Special Tiles
        // 4. Set Level Style

        static P1GenPaths p1GenPaths;
        public static P2GenSections p2GenSections;
        static P3PlaceSpecialTiles p3PlaceSpecialTiles;
        static P4SetLevelStyle p4SetLevelStyle;

        // region GLOBAL GENERATION VARIABLES

        // Random generation
        public static int seed;                             // the random seed used for the entire generation process
        public static System.Random random;                        // the random number generator which is used every time a random number is needed

        // Game Level
        public static TileArchetype[,] level_tile_grid;     // a collection of all the tile (archetypes) in the level
        public static TileType[,] final_tile_grid;          // a collection of all the tile types in the level (the final grid)
        public static SectionType[,] level_section_grid;    // a collection of all the sections SectionTypes in the level
        public static int[,] level_section_id_grid;         // a collection of all the section ids in the level
        public static LevelSize level_size;                 // the level size defines the dimensions of the level
        public static Preset preset;                        // defines the style of a level, either cave, grass, or dungeon

        // Dimensions
        public static int rows_in_level = 0;                // total number of rows of tiles in level
        public static int cols_in_level = 0;                // total number of columns of tiles in level
        public static int num_sections = 0;                 // total number of sections in the level
        public static int num_tiles = 0;                    // total number of tiles in the level
        public static int rows_in_sec = 0;                  // total number of rows of tiles in a section
        public static int cols_in_sec = 0;                  // total number of columns of tiles in a section
        public static int vert_sections = 0;                // total number of rows of sections in level
        public static int hor_sections = 0;                 // total number of columns of sections in level

        // Main Path

        public static List<int> level_path;                 // list of id's of sections on the main path
        public static int start_section_id;                 // the id of the start section
        public static int end_section_id;                   // the id of the end section
        public static int start_tile_id;                    // the id of the start tile
        public static int end_tile_id;                      // the id of the end tile

        // endregion

        public static void GenerateLevel(int seed, LevelSize level_size, Preset preset)
        {
            // This method will perform the generation of a level from start to finish

            // First, perform some setup
            doSetup(seed, level_size, preset);

            // Phase 1: Generate Paths
            p1GenPaths.Run();
            Debug.Log("Phase 1 Complete");

            // Phase 2: Generate Sections
            p2GenSections.Run();
            Debug.Log("Phase 2 Complete");

            // Phase 3: Place Special Tiles
            p3PlaceSpecialTiles.TrainWFC();
            p3PlaceSpecialTiles.Run();
            Debug.Log("Phase 3 Complete");

            // Phase 4: Set Level Style
            p4SetLevelStyle.TrainWFC(preset);
            p4SetLevelStyle.Run();
            Debug.Log("Phase 4 Complete");

            // Finally, we produce the output that can be used by other programs
            PrintFinalGrid("level.txt");

            Debug.Log("LEVEL GENERATION COMPLETE");
        }

        /// <summary>
        /// This method will run the generation algorithm from phase 3 onwards to reproduce the level in the new preset
        /// </summary>
        /// <param name="cur_seed">The seed used for Phase 1 and 2</param>
        /// <param name="wfc_seed">The seed used for Phase 3 and 4</param>
        /// <param name="preset">The preset to be used for Phase 4</param>
        public static void ChangePreset(int cur_seed, int wfc_seed, Preset preset)
        {
            // Change the preset
            var temp_preset = LevelGenerator.preset;
            var temp_seed = LevelGenerator.seed;
            LevelGenerator.preset = preset;

            LevelGenerator.seed = cur_seed;

            // Re-run all phases

            // First, perform some setup
            doSetup(LevelGenerator.seed, level_size, preset);

            // Phase 1: Generate Paths
            p1GenPaths.Run();
            Debug.Log("Phase 1 Complete");

            // Phase 2: Generate Sections
            p2GenSections.Run();
            Debug.Log("Phase 2 Complete");

            // Once the first two phase have been re-run exactly, we introduce some randomness to the phase 3 and 4 by changing the seed
            LevelGenerator.seed = wfc_seed;

            // Phase 3: Place Special Tiles
            p3PlaceSpecialTiles.TrainWFC();
            p3PlaceSpecialTiles.Run();
            Debug.Log("Phase 3 Complete");

            // Phase 4: Set Level Style
            p4SetLevelStyle.TrainWFC(preset);
            p4SetLevelStyle.Run();
            Debug.Log("Phase 4 Complete");

            // Finally, we produce the output that can be used by other programs
            PrintFinalGrid("level.txt");

            // Now, reset all changed variables
            LevelGenerator.preset = temp_preset;
            LevelGenerator.seed = temp_seed;
        }

        /// <summary>
        /// This method simply sets up the variables for processing
        /// </summary>
        private static void doSetup(int seed, LevelSize level_size, Preset preset)
        {
            // SET THE PRESET
            LevelGenerator.preset = preset;

            // CREATE THE RANDOM
            LevelGenerator.seed = seed;
            LevelGenerator.random = new System.Random(seed);

            // SET THE LEVEL SIZE
            LevelGenerator.level_size = level_size;
            switch (level_size)
            {
                // The char level_size controls the dimensions. Here, we set those dimensions
                //      rows_in_level:      n
                //      cols_in_level:      m  
                //      num_sections:       k                   

                // Each section will then have the following size
                //      rows_in_sec:        p
                //      cols_in_sec:        r                    

                // The entire grid will then have the following dimensions for the sections
                //      number of rows of sections (hor_sections):   q
                //      number of cols of sections (vert_sections):  s

                case LevelSize.Small:   // Spelunky - this custom size is 32x40 tiles with 4x4 sections of size 8x10
                    SetDimensions(32, 40, 16, 8, 10, 4, 4);
                    break;
                case LevelSize.Medium:  // Medium   - this size is nxm tiles with pxr sections of size 8x10
                    SetDimensions(48, 60, 36, 8, 10, 6, 6);
                    break;
                case LevelSize.Large:   // Large    - this size is nxm tiles with pxr sections of size 8x10
                    SetDimensions(64, 80, 64, 8, 10, 8, 8);
                    break;
                case LevelSize.Huge:    // HUGE Spelunky - this custom size is 32x40 tiles with 4x4 sections of size 8x10
                    //SetDimensions(64, 80, 64, 8, 10, 8, 8);
                    break;
                case LevelSize.Test:   // Test - for testing various features
                    //SetDimensions(25, 4, 16, 8, 10, 4, 4);
                    break;
            }

            LevelGenerator.level_tile_grid = new TileArchetype[rows_in_level, cols_in_level];
            LevelGenerator.final_tile_grid = new TileType[rows_in_level, cols_in_level];
            LevelGenerator.level_section_grid = new SectionType[hor_sections, vert_sections];
            LevelGenerator.level_section_id_grid = new int[hor_sections, vert_sections];
            LevelGenerator.level_path = new List<int>();

            // INITIALIZE ALL POSITIONS IN level_tile_grid
            for (int i = 0; i < rows_in_level; i++)
            {
                for (int j = 0; j < cols_in_level; j++)
                {
                    level_tile_grid[i, j] = TileArchetype.Wildcard;
                }
            }


            // SPLIT THE GRID INTO DIFFERENT SIZED SECTIONS
            // Note:    The original grid looks like
            //          0 0 0 0 
            //          0 0 0 0  
            //          0 0 0 0   
            //          0 0 0 0

            //          The section split (into 4) will look like (where the values in each tile is the number of the section): 
            //          1 1 | 2 2 
            //          1 1 | 2 2 
            //          ---------
            //          3 3 | 4 4          
            //          3 3 | 4 4                

            int section_id = 0;
            for (int i = 0; i < hor_sections; i++)
            {
                for (int j = 0; j < vert_sections; j++)
                {
                    // Set the section type
                    level_section_grid[i, j] = SectionType.Side;

                    // Set the section_id
                    level_section_id_grid[i, j] = section_id;

                    // Increment the section_id
                    section_id++;
                }
            }

            // CREATE THE PHASE OBJECTS
            p1GenPaths = new P1GenPaths();
            p2GenSections = new P2GenSections();
            p3PlaceSpecialTiles = new P3PlaceSpecialTiles();
            p4SetLevelStyle = new P4SetLevelStyle();
        }


        /// <summary>
        /// This method sets the dimensions for the level based on the level type. 
        /// </summary>
        private static void SetDimensions(int rows_in_level, int cols_in_level, int num_sections, int rows_in_sec, int cols_in_sec, int hor_sections, int vert_sections)
        {
            LevelGenerator.rows_in_level = rows_in_level;
            LevelGenerator.cols_in_level = cols_in_level;
            LevelGenerator.num_sections = num_sections;
            LevelGenerator.rows_in_sec = rows_in_sec;
            LevelGenerator.cols_in_sec = cols_in_sec;
            LevelGenerator.hor_sections = hor_sections;
            LevelGenerator.vert_sections = vert_sections;

            LevelGenerator.num_tiles = rows_in_level * cols_in_level;
        }

        /// <summary>
        /// Gets the archetype of a tile with the provided tile_id, and allows the user to get the row and col of the tile
        /// </summary>
        /// <param name="tile_id">The id of the tile.</param>
        /// <param name="row">The row of the tile in the level_grid.</param>
        /// <param name="col">The column of the tile in the level_grid.</param>
        /// <returns></returns>
        public static TileArchetype GetTileArchetype(int tile_id, out int row, out int col)
        {
            if (tile_id >= 0 && tile_id < num_tiles)
            {
                row = tile_id / cols_in_level;
                col = tile_id % cols_in_level;
                return level_tile_grid[row, col];
            }
            else
            {
                row = -1;
                col = -1;
                return TileArchetype.None;
            }
        }

        /// <summary>
        /// Returns the id of the section in the provided direction, -1 if there is no neighbour, and -2 otherwise
        /// </summary>
        /// <param name="cur_section_id">The id of the section.</param>
        /// <returns></returns>
        public static int GetSectionNeighbourID(int section_id, char direction)
        {
            switch (direction)
            {
                case 'u':
                    // Its above neighbour is (its location - the number of cols) unless that is less than 0
                    if (section_id - LevelGenerator.vert_sections >= 0)
                    {
                        return section_id - LevelGenerator.vert_sections;
                    }
                    return -1;
                case 'd':
                    // Its below neighbour is its location + the number of cols unless that is more than the total number of sections
                    if (section_id + LevelGenerator.vert_sections < LevelGenerator.num_sections)
                    {
                        return section_id + LevelGenerator.vert_sections;
                    }
                    return -1;
                case 'l':
                    // Its left neighbour is its location - 1 unless it is a multiple of the number of cols
                    if (section_id % LevelGenerator.vert_sections != 0)
                    {
                        return section_id - 1;
                    }
                    else return -1; // a -1 indicates no neighbour
                case 'r':
                    // Its right neighbour is its location + 1 unless (loc + 1) is a multiple of the number of cols
                    if ((section_id + 1) % LevelGenerator.vert_sections != 0)
                    {
                        return section_id + 1;
                    }
                    else return -1; // a -1 indicates no neighbour
                default:
                    return -2;
            }
        }

        /// <summary>
        /// Checks if the provided section is on the main path
        /// </summary>
        /// <param name="cur_section_id">The id of the section.</param>
        /// <returns></returns>
        internal static bool IsSectionOnPath(int cur_section_id)
        {
            for (int i = 0; i < level_path.Count; i++)
            {
                if (level_path[i] == cur_section_id)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the section type of the provided section to be the provided type
        /// </summary>
        /// <param name="section_id">The id of the section.</param>
        /// <param name="section_type">The section type.</param>
        internal static void SetSectionType(int section_id, SectionType section_type)
        {
            //section_types[section_id] = (int)section_type;   // sets the section type

            int row = section_id / vert_sections;   // the row is the id / vert_sections
            int col = section_id % vert_sections;   // the row is the id % vert_sections

            try
            {
                level_section_grid[row, col] = section_type;
            }
            catch (System.Exception)
            {
                UnityEngine.Debug.Log("Row: " + row);
                UnityEngine.Debug.Log("Row: " + row);
            }

        }

        /// <summary>
        /// This method will return the direction from one section to another.
        /// </summary>
        /// <param name="start_sec_id">ID of the origin section. </param>
        /// <param name="next_sec_id">ID of the target section. </param>
        /// <returns>The Direction of the target section, or none if the sections are not adjacent.</returns>
        public static Direction NextSectionDirection(int start_sec_id, int next_sec_id)
        {
            // Get the location of the start_sec
            int row = start_sec_id / LevelGenerator.vert_sections;
            int col = start_sec_id % LevelGenerator.vert_sections;

            // Check each direction to determine which direction the next section is relative to the start
            Array direcs = Enum.GetValues(typeof(Direction));
            foreach (Direction d in direcs)
            {
                int next_row = -1, next_col = -1;
                switch (d)
                {
                    case Direction.Up:
                        next_row = row - 1;
                        next_col = col;
                        break;
                    case Direction.Down:
                        next_row = row + 1;
                        next_col = col;
                        break;
                    case Direction.Left:
                        next_row = row;
                        next_col = col - 1;
                        break;
                    case Direction.Right:
                        next_row = row;
                        next_col = col + 1;
                        break;
                }

                // If the direction is in bounds, check if it is the location of the next section
                if (next_row >= 0 && next_row < vert_sections && next_col >= 0 && next_col < hor_sections)
                {
                    if (level_section_id_grid[next_row, next_col] == next_sec_id)
                    {
                        return d;
                    }
                }
            }
            return Direction.None;
        }


        /// <summary>
        /// Will produce the final output file containing the information needed for producing the game level. Will also produce a bitmap of the 
        /// output for visualization purposes.
        /// </summary>
        private static void PrintFinalGrid(String file_name)
        {
            // Create new directory
            //string path_name = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            //string new_directory = @".\Assets\Resources\kjarmie\LevelGenerator\outputs\level\" + seed + @"\";
            string local_dir = Directory.GetCurrentDirectory();
            string new_directory = local_dir + @"\outputs\level\";
            Directory.CreateDirectory(new_directory);

            string new_file = new_directory + file_name;

            // if (File.Exists(new_file))
            // {
            //     File.Delete(new_file);
            // }

            StreamWriter writer = new StreamWriter(new_file);

            int i = 0;
            int j = 0;
            String tile;
            char archetype;
            char type;

            TileArchetype _archetype;
            TileType _type;

            Color32 tile_color;
            for (i = 0; i < rows_in_level - 1; i++)
            {
                for (j = 0; j < cols_in_level - 1; j++)
                {
                    // Get the type and archetype as chars
                    _archetype = level_tile_grid[i, j];
                    _type = final_tile_grid[i, j];
                    archetype = (char)level_tile_grid[i, j];
                    type = (char)final_tile_grid[i, j];

                    // Print the string as TileArchetypeTileType
                    tile = "";
                    tile = String.Concat(tile, archetype);
                    tile = String.Concat(tile, type);

                    if (j != cols_in_level - 1)
                        writer.Write(tile + ",");
                    else
                        writer.Write(tile);
                }
                // Get the type and archetype as chars
                _archetype = level_tile_grid[i, j];
                _type = final_tile_grid[i, j];
                archetype = (char)level_tile_grid[i, j];
                type = (char)final_tile_grid[i, j];

                // Print the string as TileArchetypeTileType
                tile = "";
                tile = String.Concat(tile, archetype);
                tile = String.Concat(tile, type);

                writer.Write(tile);

                writer.Write("\n");


            }
            // Close the writer
            writer.Flush();
            writer.Close();


            // Now, create an image of the output
            // Create the texture
            Texture2D texture;
            texture = new Texture2D(cols_in_level, rows_in_level, TextureFormat.ARGB32, false);

            for (int x = 0; x < cols_in_level; x++)
            {
                for (int y = 0; y < rows_in_level; y++)
                {
                    // Get the type from the grid
                    _type = final_tile_grid[y, x];

                    // Print the tiles to the texture
                    tile_color = GetColor32(_type);

                    // Add the colour to the texture
                    texture.SetPixel(x, rows_in_level - y - 1, tile_color);
                }
            }

            // Save the texture
            File.WriteAllBytes(new_directory + "level.png", texture.EncodeToPNG());
        }

        /// <summary>
        /// This method will return, for each TileType, the associated Color32. Used when producing the output PNG.
        /// </summary>
        /// <param name="type">The TileType of a tile. </param>
        /// <returns>A Color32 that is associated with the TileType. </returns>
        private static Color32 GetColor32(TileType type)
        {
            Color32 tile_color = new Color32(0, 0, 0, 0);
            switch (type)
            {
                case TileType.Dirt:
                    tile_color = new Color32(111, 69, 33, 255); // light brown
                    break;
                case TileType.Stone:
                    tile_color = new Color32(77, 74, 72, 255);  // light grey 
                    break;
                case TileType.Grass:
                    tile_color = new Color32(r: 16, 57, 30, 255); // forest green
                    break;
                case TileType.Brick:
                    tile_color = new Color32(r: 136, 69, 42, 255);  // reddish-brown
                    break;
                case TileType.Weeds:
                    tile_color = new Color32(r: 16, 190, 30, 255); // forest green
                    break;
                case TileType.Mushrooms:
                    tile_color = new Color32(r: 102, 108, 99, 255); // beige
                    break;
                case TileType.Flowers:
                    tile_color = new Color32(210, 215, 40, 255); // light yellow
                    break;
                case TileType.NormalAir:
                    tile_color = new Color32(255, 255, 255, 255); // blank
                    break;
                case TileType.BlackRose:
                    tile_color = new Color32(0, 0, 0, 255); // black
                    break;
                case TileType.Boulder:
                    tile_color = new Color32(57, 57, 57, 255);  // dark grey
                    break;
                case TileType.Spikes:
                    tile_color = new Color32(43, 7, 7, 255);    // maroon
                    break;
                case TileType.Chest:
                    tile_color = new Color32(62, 35, 0, 255);    // dark brown
                    break;
                case TileType.Gold:
                    tile_color = new Color32(255, 169, 0, 255); // gold
                    break;
                case TileType.Skeleton:
                    tile_color = new Color32(230, 230, 202, 255);   // bone white
                    break;
                case TileType.House:
                    tile_color = new Color32(0, 0, 255, 255);    // blue
                    break;
                case TileType.Flag:
                    tile_color = new Color32(255, 0, 0, 255);    // red
                    break;
                default:
                    break;
            }

            return tile_color;
        }

        /// <summary>
        /// Returns the Tile Archetype associated with the specified Tile Type.
        /// </summary>
        /// <param name="type">The specified type.</param>
        /// <returns>The associated archetype.</returns>
        private static TileArchetype GetArchetype(TileType type)
        {
            switch (type)
            {
                // Ground
                case TileType.Brick:
                case TileType.Dirt:
                case TileType.Grass:
                case TileType.Stone:
                    return TileArchetype.Ground;

                // Air
                case TileType.Flowers:
                case TileType.NormalAir:
                case TileType.Mushrooms:
                case TileType.Weeds:
                    return TileArchetype.Air;

                // Trap
                case TileType.BlackRose:
                case TileType.Boulder:
                case TileType.Spikes:
                    return TileArchetype.Trap;

                // Treasure
                case TileType.Chest:
                case TileType.Gold:
                    return TileArchetype.Ground;
                // Enemy
                case TileType.Skeleton:
                    return TileArchetype.Enemy;

                default:
                    return TileArchetype.None;
            }
        }

    }

    /// <summary>
    /// This enum holds the different types of sections
    /// </summary>
    public enum SectionType
    {
        Side = 0,
        Normal = 1,
        Drop = 2,
        Landing = 3,
        Jump = 4,
        JumpLanding = 5,
        Through = 6,
        None = -1   // used as the 'null' type for this enum
    }

    /// <summary>
    /// This enum holds the different types of tile archetypes.
    /// </summary>
    public enum TileArchetype
    {
        Air = '0',
        Ground = '1',
        Trap = '2',
        Wildcard = '*',
        Treasure = '@',
        Enemy = '!',
        Start = 'S',
        End = 'X',
        None = ' '  // used as the 'null' type for this enum
    }

    /// <summary>
    /// This enum holds the specific types of tiles that can be used in the level.
    /// </summary>
    public enum TileType
    {
        // Ground
        Dirt = 'd',
        Stone = 's',
        Grass = 'g',
        Brick = 'b',

        // Air
        Weeds = 'w',
        Mushrooms = 'm',
        Flowers = 'f',
        NormalAir = 'a',

        // Trap
        BlackRose = '.',
        Boulder = 'o',
        Spikes = '∧',

        // Treasure
        Chest = 'c',
        Gold = 'G',

        // Enemy
        Skeleton = '#',

        // Start
        House = '~',

        // End
        Flag = '<',

        None = ' '
    }

    /// <summary>
    /// This enum holds the different level sizes.
    /// </summary>
    public enum LevelSize
    {
        Small = 's',
        Medium = 'm',
        Large = 'l',
        Huge = 'h',
        Test = 't',
        None = ' '   // used as the 'null' type for this enum
    }

    /// <summary>
    /// This enum holds the 8 cardinal + inter-cardinal different directions.
    /// </summary>
    public enum Direction
    {
        Up = 0,
        UpRight = 1,
        Right = 2,
        DownRight = 3,
        Down = 4,
        DownLeft = 5,
        Left = 6,
        UpLeft = 7,
        None = -1
    }

    /// <summary>
    /// Defines the preset that the WFC will be trained on for Phase 4.
    /// </summary>
    public enum Preset
    {
        Dungeon,
        Grass,
        Cave,
        General
    }
}