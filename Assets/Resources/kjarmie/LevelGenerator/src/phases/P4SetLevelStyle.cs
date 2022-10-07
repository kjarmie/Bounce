using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LevelGenerator.Phases
{
    /// <summary>
    /// This class is the base class for Phase 4: Set Level Style. 
    /// </summary>
    class P4SetLevelStyle : Phase
    {
        // For testing
        int num_zero_possibilities = 0;

        // Random generation
        protected int seed;                 // the random seed used for the entire generation process
        protected System.Random random;     // the random number generator which is used every time a random number is needed

        // For WFC
        int[,,] weights;                    // holds the weights for the neighbouring tiles for WFC where i-> symbol, j-> direction, k-> neighbouring symbol
        Dictionary<TileType, int> symbol_count;
        int total_tiles = 0;
        List<TileType> types;               // holds each unique symbol
        List<Direction> directions;         // holds all the directions
        Dictionary<TileArchetype, List<TileType>> valid_types; // holds a list of the valid types for each archetype


        public P4SetLevelStyle()
        {
            // Upon creation, cache references to often used global generation-variables from LevelGenerator
            seed = LevelGenerator.seed;
            random = LevelGenerator.random;
        }        

        /// <summary>
        /// This method will use the Wave Function Collapse Algorithm to set a style for a level from a predefined style. The WFC is a constraint optimization technique 
        /// that works as follows. In a problem with n variables, each variable has a number of possible values. These values are constrained to only be selected based on the values of 
        /// the other variables. The algorithm starts by assigning each variable to have all the values. Then, the variable with the lowest entropy (a measure of how many possible 
        /// values are still valid) is selected and one of its remaining values assigned (either randomly or using some other selection algorithm). All its constrained values are then updated
        /// based on the value selected. This continues until all variables have their value assigned.
        /// </summary>
        internal override void Run()
        {
            // The process is as follows:
            // 1. Load the weights to use in the algorithm
            // 2. Load the level_tile_grid  
            // 3. For each tile in the grid, eliminate all options which do not match the archetype
            // 4. Run WFC
            //    a) Start at a random tile, or that with lowest entropy
            //    b) Randomly choose one of the remaining options
            //    c) Propagate the change to all adjacent tiles recursively (ie. propagate to adjacent tiles, then propagate those changes to their adjacent tiles and so on)
            // 5. Print the final output

            // The WFC algorithm starts with every cell having ALL possible options selected. In this case, it would mean that every tile is simultaneously every type of Ground, Air, Trap, Enemy, Treasure, etc.
            // This obviously will not work since the input it is given is a grid where the archetypes are set. Thus, the algorithm first needs eliminate all types that do not match the archetype. 
            // For example, if a tile is 1 (Ground), the algorithm would eliminate all Air, Trap, Enemy, and Treasure types from the possible list. 

            // The final output of the level will be a grid of comma-delimited pairs of symbols. The first is the archetype, the second is the specific type.
            // Here is a very small example which has a floor and ceiling all of dirt, air in the middle, with a spike trap on the left and a treasure chest on the right

            // 1d,1d,1d,1d
            // 00,00,00,00
            // 2∧,00,00,@c
            // 1d,1d,1d,1d

            // SAVE THE VALID TILE TYPES FOR EACH TILE ARCHETYPE
            valid_types = new Dictionary<TileArchetype, List<TileType>>();
            Array archetypes = Enum.GetValues(typeof(TileArchetype));
            for (int x = 0; x < archetypes.Length; x++) // minus 1 for the None type
            {
                // Initialize the list
                List<TileType> possibilities = new List<TileType>();

                // Get the archetype
                TileArchetype archetype = (TileArchetype)archetypes.GetValue(x);

                // Add only the options which are allowed by that type
                switch (archetype)
                {
                    case TileArchetype.Start:
                        possibilities.Add(TileType.House);
                        break;
                    case TileArchetype.End:
                        possibilities.Add(TileType.Flag);
                        break;
                    case TileArchetype.Air:                    
                        possibilities.Add(TileType.Flowers);
                        possibilities.Add(TileType.Mushrooms);
                        possibilities.Add(TileType.Weeds);
                        break;
                    case TileArchetype.Ground:
                        possibilities.Add(TileType.Dirt);
                        possibilities.Add(TileType.Stone);
                        possibilities.Add(TileType.Grass);
                        possibilities.Add(TileType.Brick);
                        break;
                    case TileArchetype.Trap:
                        possibilities.Add(TileType.BlackRose);
                        possibilities.Add(TileType.Boulder);
                        possibilities.Add(TileType.Spikes);
                        break;
                    case TileArchetype.Treasure:
                        possibilities.Add(TileType.Gold);
                        possibilities.Add(TileType.Chest);
                        break;
                    case TileArchetype.Enemy:
                        possibilities.Add(TileType.Skeleton);
                        break;
                    default:
                        break;
                }

                // Add to the dictionary
                valid_types.Add(archetype, possibilities);
            }

            // PERFORM WAVE FUNCTION COLLAPSE
            Preset preset = LevelGenerator.preset;
            bool completed = DoWFC(preset);
            while (!completed)
            {
                // Reset the LevelGenerator final grid
                LevelGenerator.final_tile_grid = new TileType[LevelGenerator.rows_in_level, LevelGenerator.cols_in_level];

                // Perform WFC
                completed = DoWFC(preset);
            }

            // SAVE THE FILE AS THE FINAL OUTPUT
            PrintFinalGrid("final_grid.txt");


            //TODO: REMOVE TESTING
            Debug.Log("Number of times there were zero remaining options: " + num_zero_possibilities);
        }

        private bool DoWFC(Preset preset)
        {
            // ASSIGN TILES

            // Here, the algorithm performs the WFC on all wildcard tiles. 
            // The process is as follows:
            // 1. Load the weights to use in the algorithm
            // 2. Load the level_tile_grid  
            // 3. Run WFC
            //    a) Start at a random tile, or that with lowest entropy
            //    b) Randomly choose one of the remaining options
            //    c) Propagate the change to all adjacent tiles recursively (ie. propagate to adjacent tiles, then propagate those changes to their adjacent tiles and so on)

            // Load the weights
            weights = GetWeights(out symbol_count, preset);

            // Get the total count
            total_tiles = 0;
            for (int i = 0; i < symbol_count.Count; i++)
            {
                total_tiles += symbol_count[types[i]];
            }

            // Create the final grid
            LevelGenerator.final_tile_grid = new TileType[LevelGenerator.rows_in_level, LevelGenerator.cols_in_level];
            for (int i = 0; i < LevelGenerator.rows_in_level; i++)
            {
                for (int j = 0; j < LevelGenerator.cols_in_level; j++)
                {
                    LevelGenerator.final_tile_grid[i, j] = TileType.None;
                }
            }

            // Load the list of archetypes
            Array symbols = Enum.GetValues(typeof(TileType));
            types = new List<TileType>();

            foreach (TileType c in symbols)
            {
                //if (c != TileArchetype.Start && c != TileArchetype.End && c != TileArchetype.Wildcard)
                if (c != TileType.None)
                    types.Add(c);
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
            List<TileType>[,] remaining = new List<TileType>[LevelGenerator.rows_in_level, LevelGenerator.cols_in_level];
            for (int i = 0; i < LevelGenerator.rows_in_level; i++)
            {
                for (int j = 0; j < LevelGenerator.cols_in_level; j++)
                {
                    // Initialize the list
                    List<TileType> possibilities = new List<TileType>();

                    // Get the archetype
                    TileArchetype archetype = LevelGenerator.level_tile_grid[i, j];

                    // Add only the options which are allowed by that type
                    switch (archetype)
                    {
                        case TileArchetype.Start:
                            possibilities.Add(TileType.House);
                            break;
                        case TileArchetype.End:
                            possibilities.Add(TileType.Flag);
                            break;
                        case TileArchetype.Air:
                            possibilities.Add(TileType.NormalAir);
                            possibilities.Add(TileType.Flowers);
                            possibilities.Add(TileType.Mushrooms);
                            possibilities.Add(TileType.Weeds);

                            break;
                        case TileArchetype.Ground:
                            possibilities.Add(TileType.Dirt);
                            possibilities.Add(TileType.Stone);
                            possibilities.Add(TileType.Grass);
                            possibilities.Add(TileType.Brick);
                            break;
                        case TileArchetype.Trap:
                            possibilities.Add(TileType.BlackRose);
                            possibilities.Add(TileType.Boulder);
                            possibilities.Add(TileType.Spikes);
                            break;
                        case TileArchetype.Treasure:
                            possibilities.Add(TileType.Gold);
                            possibilities.Add(TileType.Chest);
                            break;
                        case TileArchetype.Enemy:
                            possibilities.Add(TileType.Skeleton);
                            break;
                        default:
                            break;
                    }

                    // Set the list
                    remaining[i, j] = possibilities;
                }
            }

            // BEGIN THE WFC ALGORITHM

            // TODO: This is the code for having structs for each archetype that define the possible archetypes that occur in each direction

            // Here, the entire input is scanned and determines, for each archetype, the allowed archetypes in each direction

            // TODO

            // Select a random tile to process
            int row = random.Next(0, LevelGenerator.rows_in_level);
            int col = random.Next(0, LevelGenerator.cols_in_level);

            // Get the selection
            TileType selection = GetSelection(remaining, row, col);

            // Make the selection
            LevelGenerator.final_tile_grid[row, col] = selection;

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
                if (selection == TileType.None)
                {
                    return false;
                }

                // Make the selection
                LevelGenerator.final_tile_grid[row, col] = selection;

                // Propagate the change
                Propagate(remaining, row, col, selection);
            }

            // Process is completed, now return true
            return true;
        }

        /// <summary>
        /// This method gets a TileType from the remaining options.
        /// </summary>
        /// <param name="remaining">The list of remaining options for all the tiles in the grid. </param>
        /// <param name="row">Row of the tile.</param>
        /// <param name="col">Column of the tile.</param>
        /// <returns>The TileType selected.</returns>
        private TileType GetSelection(List<TileType>[,] remaining, int row, int col)
        {
            // Select random tile, but make sure that treasure and enemy are selected with very low probability
            TileArchetype archetype = LevelGenerator.level_tile_grid[row, col];

            List<TileType> options = remaining[row, col];
            int index = random.Next(0, options.Count);
            TileType selection = TileType.None;

            // To get a symbol, we check the proportion of times that symbol appears, and then base our value on that. 

            // We process all the remaining options. For each one, we add its cumulative probability to a list. 
            // Then process that list, at each step checking if the proportion is greater than a random probability
            // Thus, values that appear often will be selected more often than those who appear less frequently, rather than purely random

            List<double> frequency = new List<double>();
            double cum_frequency = 0;
            for (int i = 0; i < options.Count; i++)
            {
                // Get the archetype
                TileType type = options[i];

                // Get the count
                int count = symbol_count[type];

                // Get the proportion
                double prop = ((double)count) / ((double)total_tiles);

                // Increase cum_frequency
                cum_frequency += prop;

                // Add it to the frequency list
                frequency.Add(cum_frequency);
            }

            // If options.Count is 0, we have run into and error, so we return None
            if (options.Count == 0)
            {
                // Since we have no options to pick for WFC, we simply randomly assign a value from those available based on the archetype.
                // Get the archetype
                archetype = LevelGenerator.level_tile_grid[row, col];

                // Get the valid possibilities
                List<TileType> possibilities = valid_types.GetValueOrDefault(archetype);

                // Select a random type
                index = random.Next(0, possibilities.Count);

                // Make the selection
                selection = possibilities[index];                

                // Return
                return selection;
            }

            else if (options.Count == 1)
            {
                List<TileType> possibilities = valid_types.GetValueOrDefault(archetype);
                selection = possibilities[0];
                return selection;
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

        private void Propagate(List<TileType>[,] remaining, int row, int col, TileType selection)
        {
            // This method needs to propagate the change of the provided tile to all adjacent tiles, and their adjacent tiles, and so on

            // Get index of symbol
            int index = types.IndexOf(selection);

            // Remove all the remaining possibilities for this tile
            remaining[row, col] = null; // simply nullify the remaining possibilities list

            // For each direction, find all the rules and update the possibilities of the tile in that direction
            for (int j = 0; j < directions.Count; j++)
            {
                // First check if that direction is in the grid
                Direction d = directions[j];
                //Direction d = (Direction)Enum.GetValues(typeof(Direction)).GetValue(j);
                int next_row = row, next_col = col;
                TileArchetype archetype = GetArchetypeInDirection(ref next_row, ref next_col, d, LevelGenerator.level_tile_grid);

                // Only if the direction is valid, continue processing
                if (next_row != -1 && next_col != -1)
                {
                    // First check if the remaining possibilities for the neighbour in the direction is null (meaning it is not considered)
                    List<TileType> possibilities = remaining[next_row, next_col];
                    if (possibilities != null)
                    {
                        // For all the types in the list of the remaining possibilities
                        int count = possibilities.Count;
                        for (int k = count - 1; k >= count; k--)
                        {
                            // Get the symbol at position k 
                            TileType symbol = possibilities[k];

                            // Get the symbol index
                            int location = types.IndexOf(symbol);

                            // If the weight for this option is 0, remove it                            
                            if (weights[index, j, location] == 0)
                            {
                                // Add to valid
                                possibilities.Remove(symbol);
                            }
                        }
                    }
                }
            }

            // Once the immediate neighbours in the 8 cardinal/ordinal directions, these need to then have their neighbours updated
            // For every tile in the 8 directions, update their neighbours
            doCascadePropagation(remaining, row, col);
        }

        private void doCascadePropagation(List<TileType>[,] remaining, int row, int col)
        {
            // This method will perform cascading propagation from the provided row and col

            // For all directions
            for (int d = 0; d < directions.Count; d++)
            {
                // Get the direction
                Direction direction = directions[d];

                // Only if the direction is valid, continue processing
                int next_row = row, next_col = col;
                TileArchetype archetype = GetArchetypeInDirection(ref next_row, ref next_col, direction, LevelGenerator.level_tile_grid);
                if (next_row != -1 && next_col != -1)
                {
                    // First check if the remaining possibilities for the neighbour in the direction is null (meaning it is not considered)
                    if (remaining[next_row, next_col] != null)
                    {
                        // Check the valid positions and update the possibilities for adjacent tiles as if the possible options for the current tile had been selected
                        List<TileType> possibilities = remaining[row, col];
                        if (possibilities != null)
                        {
                            for (int x = 0; x < possibilities.Count; x++)
                            {
                                // Get the symbol 
                                TileType t = possibilities[x];

                                // Get its index
                                int index = types.IndexOf(t);

                                // Update all the rules for direct neighbours
                                List<TileType> valid = new List<TileType>();
                                for (int k = 0; k < weights.GetUpperBound(2); k++)
                                {
                                    // Get the symbol at position k 
                                    TileType symbol = types[k];

                                    // A symbol is a valid option if the weight is not zero and if it matches the archetype in the direction
                                    if (weights[index, d, k] != 0 && archetype == LevelGenerator.level_tile_grid[next_row, next_col])
                                    {
                                        // Add to valid
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

        private void GetLowestEntropyTile(List<TileType>[,] remaining, out int row, out int col)
        {
            // Save details for the minimum
            List<TileType> min = new List<TileType>();
            for (int i = 0; i < types.Count + 1; i++)
            {
                min.Add(TileType.None);    // add some dummy data so that it will always be greater than the min
            }
            row = -1;
            col = -1;

            // Check all the tiles
            for (int i = 0; i < LevelGenerator.rows_in_level; i++)
            {
                for (int j = 0; j < LevelGenerator.cols_in_level; j++)
                {
                    // If the location is not null, continue (only looks at the wildcard and air tiles)
                    List<TileType> cur = remaining[i, j];
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
            System.Console.WriteLine();
        }

        /// <summary>
        /// This phase uses the Wave Function Collapse to decide what the wildcard tiles will be. The algorithm needs some examples to learn from
        /// before it can be used to create the level. In this method, the algorithm is provided examples and learns how to make the level.
        /// </summary>
        internal void TrainWFC(Preset preset)
        {
            // The process to train the weights is as follows:
            // 1. For each of the training files
            //    a) Load the file into memory
            //    b) Create a list of all unique symbols (all the main archetypes)
            //    c) For each unique symbol, find all occurrences and determine which symbols border it on the 8 cardinal directions
            //    d) Update the weightings for the that symbol

            // GET ALL UNIQUE SYMBOLS
            Array symbols = Enum.GetValues(typeof(TileType));
            types = new List<TileType>();
            foreach (TileType c in symbols)
            {
                //if (c != TileArchetype.Start && c != TileArchetype.End && c != TileArchetype.Wildcard)
                if (c != TileType.None)
                    types.Add(c);
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
            List<TileType[,]> training_data = GetTrainingData(preset);    // will hold all the unique symbols
            Dictionary<TileType, int> symbol_count = new Dictionary<TileType, int>();           // will hold the number of times each unique symbol appears
            List<int[,]> weights = new List<int[,]>();          // This list holds, for each symbol, the weights for all symbols and all directions

            for (int i = 0; i < types.Count; i++)
            {
                symbol_count.Add(types[i], 0);
            }

            foreach (TileType[,] input_grid in training_data)
            {
                int rows = input_grid.GetLength(0);
                int cols = input_grid.GetLength(1);

                // For all symbols in the list, process the grid to determine the weights for that symbol
                for (int y = 0; y < types.Count; y++)
                {
                    // Get the archetype
                    TileType t = types[y];

                    // Check if the weight grid exists for this symbol
                    int[,] w;
                    if (weights.Count > y)  // if the count is greater than the current symbol index, then the weight array has been created before
                    {
                        w = weights[y];
                    }
                    else
                    {
                        // Create a new grid to contain all the weights for a single symbol
                        w = new int[directions.Count, types.Count]; // we use 8 directions
                        weights.Add(w);
                    }

                    // Get the current archetype
                    TileType cur_type = types[y];

                    // Process the grid
                    for (int row = 0; row < rows; row++)
                    {
                        for (int col = 0; col < cols; col++)
                        {
                            // If the symbol at the location is the same as the current symbol being processed, continue
                            TileType cur_symbol = input_grid[row, col];
                            if (cur_symbol == cur_type)
                            {
                                // Increment the count for this symbol
                                symbol_count[cur_symbol] += 1;

                                // Process all directions
                                foreach (Direction d in directions)
                                {
                                    // Get the symbol
                                    int next_row = row, next_col = col;
                                    TileType symbol_in_direction = GetTileTypeInDirection(ref next_row, ref next_col, d, input_grid);

                                    // If the symbol is valid
                                    if (symbol_in_direction != TileType.None)
                                    {
                                        // Get its index
                                        int index = types.IndexOf(symbol_in_direction);

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
            string new_directory = local_dir + @"\data\phase4\" + preset + @"\weights\";
            ClearDirectory(new_directory); // clear the directory (gets rid of previous weights so no old values are left)
            Directory.CreateDirectory(new_directory);
            StreamWriter writer;
            for (int i = 0; i < weights.Count; i++)
            {
                int[,] w = weights[i];
                writer = new StreamWriter(new_directory + i + ".txt");

                for (int p = 0; p < 8; p++)
                {
                    int q;
                    for (q = 0; q < types.Count - 1; q++)
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
            new_directory = local_dir +  @"\data\phase4\" + preset + @"\";
            Directory.CreateDirectory(new_directory);
            writer = new StreamWriter(new_directory + "counts.txt");
            int k;
            string str;
            for (k = 0; k < symbol_count.Count - 1; k++)
            {
                // Write the archetype + , + the count
                str = types[k] + "," + symbol_count[types[k]];
                writer.Write(str + "\n");
            }
            str = types[k] + "," + symbol_count[types[k]];
            writer.Write(str);
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// Loads the weights for the WFC algorithm that were created in the training of the algorithm.
        /// </summary>
        /// <returns>A 3D array ([i,j,k]) of integers, where i = symbol, j = direction, k = neighbouring symbol.</returns>
        private int[,,] GetWeights(out Dictionary<TileType, int> symbol_count, Preset preset)
        {
            // Get the 3D array
            int[,,] weights;

            // Get all of the files
            string local_dir = Directory.GetCurrentDirectory();
            string new_directory = local_dir + @"\data\phase4\" + preset + @"\weights\";
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
                reader = new StreamReader(new_directory + i + @".txt");

                // Read the data into the array
                string[] line = new string[num_inputs];
                for (j = 0; j < num_directions; j++)
                {
                    // Load a line
                    line = reader.ReadLine().Split(',');

                    // For each item in the file
                    for (k = 0; k < line.Length; k++)
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
            symbol_count = new Dictionary<TileType, int>();
            new_directory = local_dir + @"\data\phase4\" + preset + @"\";
            reader = new StreamReader(new_directory + "counts.txt");

            while (!reader.EndOfStream)
            {
                // Read a line and split into array
                string[] input = reader.ReadLine().Split(",");

                // The first val will be the archetype, the second the count
                TileType type = (TileType)Enum.Parse(typeof(TileType), input[0]);
                int count = int.Parse(input[1]);

                // Add the the symbol_count
                symbol_count.Add(type, count);
            }
            reader.Close();

            return weights;
        }

        /// <summary>
        /// Finds the char in the provided direction in the provided grid at the provided row,col index
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="col">The column index.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="grid">The grid.</param>
        /// <returns>The char at the provided in the provided direction if valid, else ' ' if invalid.</returns>
        private TileArchetype GetArchetypeInDirection(ref int row, ref int col, Direction direction, TileArchetype[,] grid)
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
        /// Finds the char in the provided direction in the provided grid at the provided row,col index
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="col">The column index.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="grid">The grid.</param>
        /// <returns>The char at the provided in the provided direction if valid, else ' ' if invalid.</returns>
        private TileType GetTileTypeInDirection(ref int row, ref int col, Direction direction, TileType[,] grid)
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
                        return TileType.None;
                }
            }
            catch (System.IndexOutOfRangeException)
            {
                // If an invalid location is accessed, an out of range exception is caught, and a None returned
                row = -1;
                row = -1;
                return TileType.None;
            }
        }


        private List<TileType[,]> GetTrainingData(Preset preset)
        {
            // Get the list
            List<TileType[,]> training_data = new List<TileType[,]>();

            // Get all of the files
            string local_dir = Directory.GetCurrentDirectory();
            string new_directory = local_dir + @"\data\phase4\training\" + preset + @"\";
            Directory.CreateDirectory(new_directory);   // create the folder if it doesnt already exist

            int num_inputs = Directory.GetFiles(new_directory, "*.txt", SearchOption.TopDirectoryOnly).Length;  // the number of files in the folder
            StreamReader reader;
            for (int i = 0; i < num_inputs; i++)
            {
                // Load a file
                reader = new StreamReader(new_directory + i + ".txt");
                TileType[,] input_grid; // will hold the symbols from the file

                // Determine the dimensions
                int rows = 0, cols = 0;
                char[] line = new char[1];
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine().ToCharArray(); // get a line
                    rows++;
                }
                cols = line.Length;

                // Now, load the data into memory
                input_grid = new TileType[rows, cols];
                reader = new StreamReader(new_directory + i + ".txt");
                int row = 0;
                while (!reader.EndOfStream)
                {
                    // Load a line
                    line = reader.ReadLine().ToCharArray();

                    // Read into a row
                    for (int j = 0; j < line.Length; j++)
                    {
                        input_grid[row, j] = (TileType)line[j];
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
            if(di == null)
                return;

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private void PrintFinalGrid(String file_name)
        {
            // Create new directory
            string local_dir = Directory.GetCurrentDirectory();
            string new_directory = local_dir + @"\outputs\P4\";
            Directory.CreateDirectory(new_directory);

            string new_file = new_directory + file_name;

            StreamWriter writer = new StreamWriter(new_file);

            for (int i = 0; i < LevelGenerator.rows_in_level; i++)
            {
                int j;
                String tile;
                char archetype;
                char type;

                TileArchetype _archetype;
                TileType _type;
                for (j = 0; j < LevelGenerator.cols_in_level - 1; j++)
                {
                    // Get the type and archetype as chars
                    _archetype = LevelGenerator.level_tile_grid[i, j];
                    _type = LevelGenerator.final_tile_grid[i, j];
                    archetype = (char)LevelGenerator.level_tile_grid[i, j];
                    type = (char)LevelGenerator.final_tile_grid[i, j];

                    // Print the string as TileArchetypeTileType
                    tile = "";
                    tile = String.Concat(tile, archetype);
                    tile = String.Concat(tile, type);

                    writer.Write(tile + ",");
                }
                // Get the type and archetype as chars
                archetype = (char)LevelGenerator.level_tile_grid[i, j];
                type = (char)LevelGenerator.final_tile_grid[i, j];

                // Print the string as TileArchetypeTileType
                tile = "";
                tile = String.Concat(tile, archetype);
                tile = String.Concat(tile, type);

                writer.Write(tile);
                writer.Write("\n");
            }
            writer.Flush();
            writer.Close();
        }
    }

    /// <summary>
    /// Defines the adjacency rules for an archetype
    /// </summary>
    struct ArchetypeAdjacency
    {
        TileArchetype archetype;    // the archetype 
        Dictionary<Direction, List<KeyValuePair<TileArchetype, int>>> adjacencies;   // the adjacent tiles 

        public ArchetypeAdjacency(TileArchetype archetype)
        {
            // Assign the archetype and create the adjacency dictionary
            this.archetype = archetype;

            adjacencies = new Dictionary<Direction, List<KeyValuePair<TileArchetype, int>>>();
            foreach (Direction d in Enum.GetValues(typeof(Direction)))
            {
                if (d != Direction.None)
                    adjacencies.Add(d, new List<KeyValuePair<TileArchetype, int>>());
            }
        }
    }

    /// <summary>
    /// Defines the adjacency rules for an type
    /// </summary>
    struct TypeAdjacency
    {
        TileType type;    // the archetype 
        Dictionary<Direction, List<TileType>> adjacencies;   // the adjacent tiles 

        public TypeAdjacency(TileType type)
        {
            // Assign the archetype and create the adjacency dictionary
            this.type = type;

            adjacencies = new Dictionary<Direction, List<TileType>>();
            foreach (Direction d in Enum.GetValues(typeof(Direction)))
            {
                if (d != Direction.None)
                    adjacencies.Add(d, new List<TileType>());
            }
        }
    }
}

