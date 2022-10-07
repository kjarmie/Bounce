using System;
using System.IO;
using System.Collections.Generic;

namespace LevelGenerator.Phases
{
    /// <summary>
    /// This class is the base class for Phase 3: Place Special Tiles. 
    /// </summary>
    class P3PlaceSpecialTiles : Phase
    {
        // Random generation
        protected int seed;                         // the random seed used for the entire generation process
        protected System.Random random;             // the random number generator which is used every time a random number is needed

        // For WFC
        int[,,] weights;        // holds the weights for the neighbouring tiles for WFC where i-> symbol, j-> direction, k-> neighbouring symbol
        Dictionary<TileArchetype, int> symbol_count; // holds the number of times each unique symbol appeared in the training data
        int total_tiles = 0;
        List<TileArchetype> archetypes;  // holds each unique symbol
        List<Direction> directions;   // holds all the directions

        public P3PlaceSpecialTiles()
        {
            // Upon creation, cache references to often used global generation-variables from LevelGenerator
            seed = LevelGenerator.seed;
            random = LevelGenerator.random;
        }


        /// <summary>
        /// This method will update the level_tile_grid file to include treasure and enemy tiles and set all wildcard tiles. 
        /// Note: the previous phase has prefabs for the sections which could include special tiles, but this means that structures 
        /// and patterns will be repeated and noticeable, so this phase will perform the generation procedurally.
        /// Note: this phase works at a high level, only considering tile archetypes, not specifics.
        /// </summary>
        internal override void Run()
        {
            // Do the WFC    
            DoWFC();

            // Print the grid
            PrintLevelTileGrid("level_tile_grid.txt");
        }

        private void DoWFC()
        {
            // ASSIGN WILDCARDS

            // Here, the algorithm performs the WFC on all wildcard tiles. 
            // The process is as follows:
            // 1. Load the weights to use in the algorithm
            // 2. Load the level_tile_grid  
            // 3. Run WFC
            //    a) Start at a random tile, or that with lowest entropy
            //    b) Randomly choose one of the remaining options
            //    c) Propagate the change to all adjacent tiles recursively (ie. propagate to adjacent tiles, then propagate those changes to their adjacent tiles and so on)

            // Load the weights
            weights = GetWeights(out symbol_count);

            // Get the total count
            total_tiles = 0;
            for (int i = 0; i < symbol_count.Count; i++)
            {
                total_tiles += symbol_count[archetypes[i]];
            }

            // Load the list of archetypes
            Array symbols = Enum.GetValues(typeof(TileArchetype));
            archetypes = new List<TileArchetype>();

            foreach (TileArchetype c in symbols)
            {
                if (c != TileArchetype.None)
                    archetypes.Add(c);
            }

            // Load the directions
            Array direcs = Enum.GetValues(typeof(Direction));
            directions = new List<Direction>();
            foreach (Direction d in direcs)
            {
                if (d != Direction.None)
                    directions.Add(d);
            }


            // For each position, keep track of the remaining possibilities (so for tile i,j, has list of all remaining options)
            List<TileArchetype>[,] remaining = new List<TileArchetype>[LevelGenerator.rows_in_level, LevelGenerator.cols_in_level];
            for (int i = 0; i < LevelGenerator.rows_in_level; i++)
            {
                for (int j = 0; j < LevelGenerator.cols_in_level; j++)
                {
                    // If the location of the level_tile_grid is air or wildcard, continue (algorithm doesn't run on other tiles)
                    TileArchetype cur = LevelGenerator.level_tile_grid[i, j];
                    if (cur == TileArchetype.Wildcard)
                    {
                        // Initialize the list
                        List<TileArchetype> possibilities = new List<TileArchetype>();

                        // Add all possibilities
                        foreach (TileArchetype c in archetypes)
                        {
                            if (c != TileArchetype.Start && c != TileArchetype.End && c != TileArchetype.Wildcard)
                                possibilities.Add(c);
                        }

                        // Set the list
                        remaining[i, j] = possibilities;
                    }
                    else
                    {
                        remaining[i, j] = null;
                    }
                }
            }

            // BEGIN THE WFC ALGORITHM

            // Select a random tile to process
            int row, col;
            while (true)
            {
                // Get a random tile_id
                int tile_id = random.Next(0, LevelGenerator.num_tiles);

                // If that tile is not wildcard, try again
                if (LevelGenerator.GetTileArchetype(tile_id, out row, out col) == TileArchetype.Wildcard)
                {
                    break;
                }
            }

            // Get the selection
            TileArchetype selection = GetSelection(remaining, row, col);

            // Make the selection
            LevelGenerator.level_tile_grid[row, col] = selection;

            // Propagate the change
            Propagate(remaining, row, col, selection);

            // Now, perform the WFC on the rest of the grid until it is finished
            while (true)
            {
                // Determine which tiles are still remaining, and their entropy
                GetLowestEntropyTile(remaining, out row, out col);

                // If the found location is [-1, -1], then the processing is done
                if (row == -1 && col == -1)
                {
                    break;
                }

                // Get the selection
                selection = GetSelection(remaining, row, col);

                // If selection is None, there is an error, so we return
                if (selection == TileArchetype.None)
                {
                    return;
                }

                // Make the selection
                LevelGenerator.level_tile_grid[row, col] = selection;

                // Propagate the change
                Propagate(remaining, row, col, selection);
            }
        }

        private TileArchetype GetSelection(List<TileArchetype>[,] remaining, int row, int col)
        {
            // Select random tile, but make sure that treasure and enemy are selected with very low probability

            // Get the list of remaining options
            List<TileArchetype> options = remaining[row, col];

            // Randomly select a symbol
            int index = random.Next(0, options.Count);

            // To get a symbol, we check the proportion of times that symbol appears, and then base our value on that. 

            // We process all the remaining options. For each one, we add its cumulative probability to a list. 
            // Then process that list, at each step checking if the proportion is greater than a random probability
            // Thus, values that appear often will be selected more often than those who appear less frequently, rather than purely random

            List<double> frequency = new List<double>();
            double cum_frequency = 0;
            TileArchetype selection = TileArchetype.None;
            for (int i = 0; i < options.Count; i++)
            {
                // Get the archetype
                TileArchetype archetype = options[i];

                // Get the count
                int count = symbol_count[archetype];

                // Get the proportion
                double prop = ((double)count) / ((double)total_tiles);

                // Increase cum_frequency
                cum_frequency += prop;

                // Add it to the frequency list
                frequency.Add(cum_frequency);
            }

            // Run until a value is found
            bool still_processing = true;
            while (still_processing)
            {
                // Now, get a random probability
                double probability = random.NextDouble();

                // Process then entire list
                for (int i = 0; i < frequency.Count; i++)
                {
                    // Get the frequency
                    double freq = frequency[i];

                    // Check if the frequency is greater than the probability, return that option
                    if (freq >= probability)
                    {
                        // Get the archetype from the list of remaining
                        selection = options[i];

                        // Break
                        still_processing = false;
                        break;
                    }
                }
            }

            return selection;
        }

        private void Propagate(List<TileArchetype>[,] remaining, int row, int col, TileArchetype selection)
        {
            // This method needs to propagate the change of the provided tile to all adjacent tiles, and their adjacent tiles, and so on

            // Get index of symbol
            int index = archetypes.IndexOf(selection);

            // Remove all the remaining possibilities for this tile
            remaining[row, col] = null; // simply nullify the remaining possibilities list

            // For each direction, find all the rules and update the possibilities of the tile in that direction
            for (int j = 0; j < directions.Count; j++)
            {
                // First check if that direction is in the grid
                Direction d = directions[j];
                //Direction d = (Direction)Enum.GetValues(typeof(Direction)).GetValue(j);
                int next_row = row, next_col = col;
                TileArchetype archetype = GetSymbolInDirection(ref next_row, ref next_col, d, LevelGenerator.level_tile_grid);

                // Only if the direction is valid, continue processing
                if (next_row != -1 && next_col != -1)
                {
                    // First check if the remaining possibilities for the neighbour in the direction is null (meaning it is not considered)
                    if (remaining[next_row, next_col] != null)
                    {
                        // Get a list of all possible symbols that can appear in that direction
                        List<TileArchetype> valid = new List<TileArchetype>();
                        for (int k = 0; k < weights.GetUpperBound(2); k++)
                        {
                            // Get the symbol at position k 
                            TileArchetype symbol = archetypes[k];
                            if (weights[index, j, k] != 0)
                            { // a symbol is a valid option if the weight is not zero
                              // Add to the list of valid symbols
                                valid.Add(symbol);
                            }
                        }
                        // For the possibilities in that direction, remove any symbols that do not appear in the valid possibilities (or just replace the list)
                        remaining[next_row, next_col] = valid;
                    }
                }
            }

            // Once the immediate neighbours in the 8 cardinal/ordinal directions, these need to then have their neighbours updated
            // For every tile in the 8 directions, update their neighbours
            doCascadePropagation(remaining, row, col);
        }

        private void doCascadePropagation(List<TileArchetype>[,] remaining, int row, int col)
        {
            // This method will perform cascading propagation from the provided row and col

            // For all directions
            for (int d = 0; d < directions.Count; d++)
            {
                // Get the direction
                Direction direction = directions[d];

                // Only if the direction is valid, continue processing
                int next_row = row, next_col = col;
                TileArchetype neighbour = GetSymbolInDirection(ref next_row, ref next_col, direction, LevelGenerator.level_tile_grid);
                if (next_row != -1 && next_col != -1)
                {
                    // First check if the remaining possibilities for the neighbour in the direction is null (meaning it is not considered)
                    if (remaining[next_row, next_col] != null)
                    {
                        // Check the valid positions and update the possibilities for adjacent tiles as if the possible options for the current tile had been selected
                        List<TileArchetype> possibilities = remaining[row, col];
                        if (possibilities != null)
                        {
                            for (int x = 0; x < possibilities.Count; x++)
                            {
                                // Get the symbol 
                                TileArchetype t = possibilities[x];

                                // Get its index
                                int index = archetypes.IndexOf(t);

                                // Update all the rules for direct neighbours
                                List<TileArchetype> valid = new List<TileArchetype>();
                                for (int k = 0; k < weights.GetUpperBound(2); k++)
                                {
                                    // Get the symbol at position k 
                                    TileArchetype symbol = archetypes[k];

                                    if (weights[index, d, k] != 0)
                                    { // a symbol is a valid option if the weight is not zero
                                      // Add to the list of valid symbols
                                        valid.Add(symbol);
                                    }
                                }
                                // Set the remaining possibilities
                                remaining[next_row, next_col] = valid;
                            }
                        }
                    }
                }
            }
        }

        private void GetLowestEntropyTile(List<TileArchetype>[,] remaining, out int row, out int col)
        {
            // Save details for the minimum
            List<TileArchetype> min = new List<TileArchetype>();
            for (int i = 0; i < archetypes.Count + 1; i++)
            {
                min.Add(TileArchetype.None);    // add some dummy data so that it will always be greater than the min
            }
            row = -1;
            col = -1;

            // Check all the tiles
            for (int i = 0; i < LevelGenerator.rows_in_level; i++)
            {
                for (int j = 0; j < LevelGenerator.cols_in_level; j++)
                {
                    // If the location is not null, continue (only looks at the wildcard and air tiles)
                    List<TileArchetype> cur = remaining[i, j];
                    if (cur != null)
                    {
                        // Get the count 
                        int count = cur.Count;

                        // Check if it is the lowest
                        if (count < min.Count)
                        {
                            // Set the min
                            min = cur;

                            // Set the out indices
                            row = i;
                            col = j;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This phase uses the Wave Function Collapse to decide what the wildcard tiles will be. The algorithm needs some examples to learn from
        /// before it can be used to create the level. In this method, the algorithm is provided examples and learns how to make the level.
        /// </summary>
        internal void TrainWFC()
        {
            // The process to train the weights is as follows:
            // 1. For each of the training files
            //    a) Load the file into memory
            //    b) Create a list of all unique symbols (all the main archetypes)
            //    c) For each unique symbol, find all occurrences and determine which symbols border it on the 8 cardinal directions
            //    d) Update the weightings for the that symbol

            // GET ALL UNIQUE SYMBOLS
            Array symbols = Enum.GetValues(typeof(TileArchetype));
            archetypes = new List<TileArchetype>();
            foreach (TileArchetype c in symbols)
            {
                //if (c != TileArchetype.Start && c != TileArchetype.End && c != TileArchetype.Wildcard)
                if (c != TileArchetype.None)
                    archetypes.Add(c);
            }

            // Load the directions
            Array direcs = Enum.GetValues(typeof(Direction));
            directions = new List<Direction>();
            foreach (Direction d in direcs)
            {
                if (d != Direction.None)
                    directions.Add(d);
            }

            // GET TRAINING DATA
            List<TileArchetype[,]> training_data = GetTrainingData();    // will hold all the unique symbols
            Dictionary<TileArchetype, int> symbol_count = new Dictionary<TileArchetype, int>();           // will hold the number of times each unique symbol appears
            List<int[,]> weights = new List<int[,]>();          // This list holds, for each symbol, the weights for all symbols and all directions

            for (int i = 0; i < archetypes.Count; i++)
            {
                symbol_count.Add(archetypes[i], 0);
            }

            foreach (TileArchetype[,] input_grid in training_data)
            {
                int rows = input_grid.GetLength(0);
                int cols = input_grid.GetLength(1);

                // For all symbols in the list, process the grid to determine the weights for that symbol
                for (int y = 0; y < archetypes.Count; y++)
                {
                    // Get the archetype
                    TileArchetype t = archetypes[y];

                    // Check if the weight grid exists for this symbol
                    int[,] w;
                    if (weights.Count > y)  // if the count is greater than the current symbol index, then the weight array has been created before
                    {
                        w = weights[y];
                    }
                    else
                    {
                        // Create a new grid to contain all the weights for a single symbol
                        w = new int[directions.Count, archetypes.Count]; // we use 8 directions
                        weights.Add(w);
                    }

                    // Get the current archetype
                    TileArchetype cur_archetype = archetypes[y];

                    // Process the grid
                    for (int row = 0; row < rows; row++)
                    {
                        for (int col = 0; col < cols; col++)
                        {
                            // If the symbol at the location is the same as the current symbol being processed, continue
                            TileArchetype cur_symbol = input_grid[row, col];
                            if (cur_symbol == cur_archetype)
                            {
                                // Increment the count for this symbol
                                symbol_count[cur_symbol] += 1;

                                // Process all directions
                                Array directions = Enum.GetValues(typeof(Direction));
                                foreach (Direction d in directions)
                                {
                                    // Get the symbol
                                    int next_row = row, next_col = col;
                                    TileArchetype symbol_in_direction = GetSymbolInDirection(ref next_row, ref next_col, d, input_grid);

                                    // If the symbol is valid
                                    if (symbol_in_direction != TileArchetype.None)
                                    {
                                        // Get its index
                                        int index = archetypes.IndexOf(symbol_in_direction);

                                        // Increment the weight
                                        w[(int)d, index] += 1;
                                    }   // otherwise, continue to the other directions
                                }
                            }
                        }
                    }
                }
            }

            // Save all the weights for later use
            string local_dir = Directory.GetCurrentDirectory();
            string new_directory = local_dir + @"\data\phase3\weights\";
            ClearDirectory(new_directory); // clear the directory (gets rid of previous weights so no old values are left)
            Directory.CreateDirectory(new_directory);
            StreamWriter writer;
            for (int i = 0; i < weights.Count; i++)
            {
                int[,] w = weights[i];
                writer = new StreamWriter(new_directory + @"\" + i + ".txt");

                for (int p = 0; p < 8; p++)
                {
                    int q;
                    for (q = 0; q < archetypes.Count - 1; q++)
                    {
                        writer.Write(w[p, q] + ",");
                    }
                    writer.Write(w[p, q]);
                    writer.Write("\n");
                }
                writer.Flush();
                writer.Close();

            }

            // Save the counts of the symbols
            new_directory = local_dir + @"\data\phase3\";
            writer = new StreamWriter(new_directory + "counts.txt");
            int k;
            string str;
            for (k = 0; k < symbol_count.Count - 1; k++)
            {
                // Write the archetype + , + the count
                str = archetypes[k] + "," + symbol_count[archetypes[k]];
                writer.Write(str + "\n");
            }
            str = archetypes[k] + "," + symbol_count[archetypes[k]];
            writer.Write(str);
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// Finds the char in the provided direction in the provided grid at the provided row,col index
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="col">The column index.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="grid">The grid.</param>
        /// <returns>The char at the provided in the provided direction if valid, else ' ' if invalid.</returns>
        private TileArchetype GetSymbolInDirection(ref int row, ref int col, Direction direction, TileArchetype[,] grid)
        {
            // Determine the direction
            try
            {
                switch (direction)
                {
                    case Direction.Up:
                        row = row - 1;
                        return grid[row - 1, col];
                    case Direction.UpRight:
                        row = row - 1;
                        col = col + 1;
                        return grid[row - 1, col + 1];
                    case Direction.Right:
                        col = col + 1;
                        return grid[row, col + 1];
                    case Direction.DownRight:
                        row = row + 1;
                        col = col + 1;
                        return grid[row + 1, col + 1];
                    case Direction.Down:
                        row = row + 1;
                        return grid[row + 1, col];
                    case Direction.DownLeft:
                        row = row + 1;
                        col = col - 1;
                        return grid[row + 1, col - 1];
                    case Direction.Left:
                        col = col - 1;
                        return grid[row, col - 1];
                    case Direction.UpLeft:
                        row = row - 1;
                        col = col - 1;
                        return grid[row - 1, col - 1];
                    default:
                        row = -1;
                        row = -1;
                        return TileArchetype.None;
                }
            }
            catch (System.IndexOutOfRangeException)
            {
                // If an invalid location is accessed, an out of range exception is caught, and a None returned
                row = -1;
                row = -1;
                return TileArchetype.None;
            }
        }

        /// <summary>
        /// Loads the weights for the WFC algorithm that were created in the training of the algorithm.
        /// </summary>
        /// <returns>A 3D array ([i,j,k]) of integers, where i = symbol, j = direction, k = neighbouring symbol.</returns>
        private int[,,] GetWeights(out Dictionary<TileArchetype, int> symbol_count)
        {
            // Get the 3D array
            int[,,] weights;

            // Get all of the files
            string local_dir = Directory.GetCurrentDirectory();
            string new_directory = local_dir + @"\data\phase3\weights\";
            Directory.CreateDirectory(new_directory);   // create the folder if it doesnt already exist

            int num_inputs = Directory.GetFiles(new_directory, "*.txt", SearchOption.TopDirectoryOnly).Length;  // the number of files in the folder

            // Dimensions
            int num_symbols = num_inputs;   // the total number of unique symbols used (equal to the number of files)
            int num_directions = 8;         // the number of directions (4 cardinal + 4 inter-cardinal)
            weights = new int[num_symbols, num_directions, num_symbols];

            // Read the data
            StreamReader reader;
            int i;  // index of current symbol
            int k;  // index of direction
            int j;  // index of neighbouring symbol
            for (i = 0; i < num_inputs; i++)
            {
                // Obtain the file
                reader = new StreamReader(new_directory + i + ".txt");

                // Read the data into the array
                string[] line = new string[num_inputs];
                for (j = 0; j < num_directions; j++)
                {
                    // Load a line
                    line = reader.ReadLine().Split(',');

                    // For each item in the file
                    for (k = 0; k < line.Length; k++)   // TODO: should be equal to num_inputs
                    {
                        // Get the weight
                        int weight = int.Parse(line[k]);

                        // Place into array
                        weights[i, j, k] = weight;
                    }
                }
                reader.Close();
            }

            // Load the counts for the symbols
            symbol_count = new Dictionary<TileArchetype, int>();
            new_directory = local_dir + @"\data\phase3\";
            reader = new StreamReader(new_directory + "counts.txt");

            while (!reader.EndOfStream)
            {
                // Read a line and split into array
                string[] input = reader.ReadLine().Split(",");

                // The first val will be the archetype, the second the count
                TileArchetype archetype = (TileArchetype)Enum.Parse(typeof(TileArchetype), input[0]);
                int count = int.Parse(input[1]);

                // Add the the symbol_count
                symbol_count.Add(archetype, count);
            }
            reader.Close();

            return weights;
        }

        /// <summary>
        /// Loads each of the example files for the WFC to learn from.
        /// </summary>
        /// <returns>A char[] list of all the files in array format. </returns>
        private List<TileArchetype[,]> GetTrainingData()
        {
            // Get the list
            List<TileArchetype[,]> training_data = new List<TileArchetype[,]>();

            // Get all of the files
            string local_dir = Directory.GetCurrentDirectory();
            string new_directory = local_dir + @"\data\phase3\training\";
            Directory.CreateDirectory(new_directory);   // create the folder if it doesnt already exist

            int num_inputs = Directory.GetFiles(new_directory, "*.txt", SearchOption.TopDirectoryOnly).Length;  // the number of files in the folder
            StreamReader reader;
            for (int i = 0; i < num_inputs; i++)
            {
                // Load a file
                reader = new StreamReader(new_directory + i + ".txt");
                TileArchetype[,] input_grid; // will hold the symbols from the file

                // Determine the dimensions
                int rows = 0, cols = 0;
                char[] line = new char[1];
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine().ToCharArray(); // get a line
                    rows++;
                }
                cols = line.Length;
                reader.Close();

                // Now, load the data into memory
                input_grid = new TileArchetype[rows, cols];
                reader = new StreamReader(new_directory + i + ".txt");
                int row = 0;
                while (!reader.EndOfStream)
                {
                    // Load a line
                    line = reader.ReadLine().ToCharArray();

                    // Read into a row
                    for (int j = 0; j < line.Length; j++)
                    {
                        input_grid[row, j] = (TileArchetype)line[j];
                    }

                    // Increment the row
                    row++;
                }
                reader.Close();

                // Add the new char grid into the list
                training_data.Add(input_grid);
            }

            // Return the list
            return training_data;
        }

        private void ClearDirectory(string path_name)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path_name);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public void PrintLevelTileGrid(String file_name)
        {
            // Create new directory
            string local_dir = Directory.GetCurrentDirectory();
            string new_directory = local_dir + @"\outputs\P3\";
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
            writer.Flush();
            writer.Close();
        }
    }
}