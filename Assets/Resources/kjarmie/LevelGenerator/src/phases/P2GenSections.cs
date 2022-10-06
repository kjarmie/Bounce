using System;
using System.IO;
using UnityEngine;

namespace LevelGenerator.Phases
{
    /// <summary>
    /// This class is the base class for Phase 2: Generate Sections. 
    /// </summary>
    public class P2GenSections : Phase
    {
        // Random generation
        protected int seed;                 // the random seed used for the entire generation process
        protected System.Random random;            // the random number generator which is used every time a random number is needed

        public P2GenSections()
        {
            // Upon creation, cache references to often used global generation-variables from LevelGenerator
            seed = LevelGenerator.seed;
            random = LevelGenerator.random;
        }

        /// <summary>
        /// This method will produce a list of text files that contain grids of tiles for each section, 
        /// as well as a large file that contains tile info for the entire level. The tile information will 
        /// only be the tile archetype, not the specific style 
        /// </summary>
        internal override void Run()
        {
            // The process to generate a section is as follows:
            // 1.   Determine the prefab type, based on the section type
            // 2.   Select a random prefab of that type, and load the tiles
            // 2.   Load the main path data associated
            // 3.   Each prefab has certain wildcard values. Assign these randomly

            // FOR ALL SECTIONS, LOAD A PREFAB AND OUTPUT THE SECTION
            int row_start = 0;
            int col_start = 0;

            int section_id = 0;
            for (int i = 0; i < LevelGenerator.hor_sections; i++)
            {
                // Make sure to reset the col_start to 0
                col_start = 0;
                for (int j = 0; j < LevelGenerator.vert_sections; j++)
                {
                    // Get the section_type
                    SectionType section_type = LevelGenerator.level_section_grid[i, j];

                    // Generate the section
                    GenerateSection(section_id, section_type, row_start, row_start + LevelGenerator.rows_in_sec - 1, col_start, col_start + LevelGenerator.cols_in_sec - 1);

                    // Increment the start of the section columns
                    col_start += LevelGenerator.cols_in_sec;

                    // Increment the section_id
                    section_id++;
                }

                // Increment row
                row_start += LevelGenerator.rows_in_sec;
            }

            // OUTPUT THE FINAL GRID
            PrintLevelTileGrid("level_tile_grid.txt");
        }

        /// <summary>
        /// This method will perform the generation of a single section. 
        /// </summary>
        /// <param name="section_id">The id of the section.</param>
        /// <param name="section_type">The type of the section.</param>
        private void GenerateSection(int section_id, SectionType section_type, int start_row, int end_row, int start_col, int end_col)
        {
            // SELECT A PREFAB AND LOAD INTO THE SECTION GRID
            TileArchetype[,] section_tile_grid = LoadSection((int)section_type);

            // SET THE ITEMS IN THE SECTION GRID INTO THE LEVEL GRID

            int level_row = start_row;
            int level_col = start_col;
            for (int i = 0; i < LevelGenerator.rows_in_sec; i++)
            {
                // Reset the level_col
                level_col = start_col;

                int id = 0;

                for (int j = 0; j < LevelGenerator.cols_in_sec; j++)
                {
                    // Load the tile archetype from the text file
                    TileArchetype tile_archetype = section_tile_grid[i, j];

                    // Set tile in the level_tile_grid
                    LevelGenerator.level_tile_grid[level_row, level_col] = tile_archetype;

                    // SPECIAL CASE
                    if (tile_archetype == TileArchetype.Start)
                    {
                        // If the current section is start
                        if (section_id == LevelGenerator.start_section_id)
                        {
                            LevelGenerator.level_tile_grid[level_row, level_col] = TileArchetype.Start;
                            LevelGenerator.start_tile_id = id;
                        }
                        // If the current section is the end
                        else if (section_id == LevelGenerator.end_section_id)
                        {
                            LevelGenerator.level_tile_grid[level_row, level_col] = TileArchetype.End;
                            LevelGenerator.end_tile_id = id;
                        }
                        // Otherwise
                        else
                        {
                            LevelGenerator.level_tile_grid[level_row, level_col] = TileArchetype.Air;
                        }
                    }

                    // Increment the level_col
                    level_col++;

                    // Increment id
                    id++;
                }

                // Increment the level_row
                level_row++;
            }

            // OUTPUT THE TILE GRID
            PrintSectionTileGrid(section_id, section_tile_grid);
        }


        /// <summary>
        /// This methods creates a 2D array of TileArchetypes from the specified section_type. There is a 50/50 chance that the level is mirrored.
        /// </summary>
        /// <param name="section_type"></param>
        /// <returns></returns>
        private TileArchetype[,] LoadSection(int section_type)
        {
            // Select the folder
            string new_directory = @".\Assets\Resources\kjarmie\LevelGenerator\data\sections\8x10\prefabs\" + section_type;
            Directory.CreateDirectory(new_directory);   // create the folder if it doesnt already exist
            int fCount = Directory.GetFiles(new_directory, "*.txt", SearchOption.TopDirectoryOnly).Length;  // the number of files in the folder
            int chance = random.Next(0, fCount);    // the number of the file
            StreamReader reader = new StreamReader(new_directory + @"\" + chance + ".txt");

            // Assign all tiles from the grid into the section_grid
            TileArchetype[,] section_tile_grid = new TileArchetype[LevelGenerator.rows_in_sec, LevelGenerator.cols_in_sec]; // holds the tiles for a section

            double chance_mirrored = random.NextDouble();
            bool mirrored = (chance_mirrored < 0.5) ? true : false;
            for (int i = 0; i < LevelGenerator.rows_in_sec; i++)
            {
                // Load a line of types
                char[] line = reader.ReadLine().ToCharArray();

                int id = 0;

                if (!mirrored)
                {
                    for (int j = 0; j < LevelGenerator.cols_in_sec; j++)
                    {
                        // Load the tile archetype from the text file
                        TileArchetype tile_archetype = (TileArchetype)line[j];

                        // Set tile in the section_tile_grid
                        section_tile_grid[i, j] = tile_archetype;

                        // Increment id
                        id++;
                    }
                }
                else
                {
                    for (int j = 0; j < LevelGenerator.cols_in_sec; j++)
                    {
                        // Load the tile archetype from the text file
                        TileArchetype tile_archetype = (TileArchetype)line[j];

                        // Set tile in the section_tile_grid
                        section_tile_grid[i, LevelGenerator.cols_in_sec - j - 1] = tile_archetype;

                        // Increment id
                        id++;
                    }
                }
            }

            reader.Close();

            return section_tile_grid;
        }

        public void PrintLevelTileGrid(String file_name)
        {
            // Create new directory
            string new_directory = @".\Assets\Resources\kjarmie\LevelGenerator\outputs\P2\";
            Directory.CreateDirectory(new_directory);

            string new_file = new_directory + file_name;

            if (File.Exists(new_file))
            {
                File.Delete(new_file);
            }

            StreamWriter writer = new StreamWriter(new_file);

            for (int i = 0; i < LevelGenerator.rows_in_level; i++)
            {
                for (int j = 0; j < LevelGenerator.cols_in_level; j++)
                {
                    writer.Write((char)LevelGenerator.level_tile_grid[i, j]);
                }
                writer.Write("\n");
            }
            writer.Close();
        }

        private void PrintSectionTileGrid(int section_id, TileArchetype[,] section_tile_grid)
        {
            // Create new directory
            string new_directory = @".\Assets\Resources\kjarmie\LevelGenerator\outputs\P2\sections\";
            Directory.CreateDirectory(new_directory);

            string new_file = new_directory + "section_" + section_id + "_tile_grid.txt";

            if (File.Exists(new_file))
            {
                File.Delete(new_file);
            }

            StreamWriter writer = new StreamWriter(new_file);

            for (int i = 0; i < LevelGenerator.rows_in_sec; i++)
            {
                for (int j = 0; j < LevelGenerator.cols_in_sec; j++)
                {
                    writer.Write((char)section_tile_grid[i, j]);
                }
                writer.Write("\n");
            }
            writer.Close();
        }
    }

}