using System;
using System.Collections.Generic;
using LevelGenerator.Phases;
using System.IO;

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
        public static Random random;                        // the random number generator which is used every time a random number is needed

        // Game Level
        public static TileArchetype[,] level_tile_grid;     // a collection of all the tile (archetypes) in the level
        public static TileType[,] final_tile_grid;          // a collection of all the tile types in the level (the final grid)
        public static SectionType[,] level_section_grid;    // a collection of all the sections (section_ids) in the level
        public static char level_size;                      // the level size defines the dimensions of the level

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

        public static void GenerateLevel(int seed, LevelSize level_size)
        {
            // This method will perform the generation of a level from start to finish

            // First, perform some setup
            doSetup(seed, level_size);

            // Phase 1: Generate Paths
            p1GenPaths.Run();

            // Phase 2: Generate Sections
            p2GenSections.Run();

            // Phase 3: Place Special Tiles
            p3PlaceSpecialTiles.TrainWFC();
            p3PlaceSpecialTiles.Run();

            // Phase 4: Set Level Style
            p4SetLevelStyle.TrainWFC();
            p4SetLevelStyle.Run();

            // Finally, we produce the output that can be used by other programs
            PrintFinalGrid("level.txt");
        }

        /// <summary>
        /// Will produce images for each of the sections tile grids, and the main level tile grid
        /// </summary>
        private static void Visualize()
        {
            // Create the level visual

            // Create the section visual
        }

        /// <summary>
        /// This method simply sets up the variables for processing
        /// </summary>
        private static void doSetup(int seed, LevelSize level_size)
        {
            // CREATE THE RANDOM
            LevelGenerator.seed = seed;
            LevelGenerator.random = new Random(seed);

            // SET THE LEVEL SIZE
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
                    break;
                case LevelSize.Large:   // Large    - this size is nxm tiles with pxr sections of size 8x10
                    break;
                case LevelSize.Huge:    // HUGE Spelunky - this custom size is 32x40 tiles with 4x4 sections of size 8x10
                    SetDimensions(64, 80, 64, 8, 10, 8, 8);
                    break;
            }

            LevelGenerator.level_tile_grid = new TileArchetype[rows_in_level, cols_in_level];
            LevelGenerator.final_tile_grid = new TileType[rows_in_level, cols_in_level];
            LevelGenerator.level_section_grid = new SectionType[hor_sections, vert_sections];
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

            level_section_grid[row, col] = section_type;
        }

        /// <summary>
        /// Will produce the final output file containing the information needed for producing the game level.
        /// </summary>
        private static void PrintFinalGrid(String file_name)
        {
            // Create new directory
            //string path_name = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            string new_directory = @".\Assets\Resources\kjarmie\LevelGenerator\outputs\level\" + seed + @"\";
            Directory.CreateDirectory(new_directory);

            string new_file = new_directory + file_name;

            if (File.Exists(new_file))
            {
                File.Delete(new_file);
            }

            StreamWriter writer = new StreamWriter(new_file);

            for (int i = 0; i < rows_in_level; i++)
            {
                int j;
                String tile;
                char archetype;
                char type;

                TileArchetype _archetype;
                TileType _type;
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

                    writer.Write(tile + ",");
                }
                // Get the type and archetype as chars
                archetype = (char)level_tile_grid[i, j];
                type = (char)final_tile_grid[i, j];

                // Print the string as TileArchetypeTileType
                tile = "";
                tile = String.Concat(tile, archetype);
                tile = String.Concat(tile, type);

                writer.Write(tile);
                writer.Write("\n");
            }
            writer.Close();
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
}