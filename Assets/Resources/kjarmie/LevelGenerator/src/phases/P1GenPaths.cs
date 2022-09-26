using System;
using System.IO;
using System.Collections.Generic;

namespace LevelGenerator.Phases
{
    /// <summary>
    /// This class is the base class for Phase 1: Generate Paths. 
    /// </summary>
    internal class P1GenPaths : Phase
    {
        // Random generation
        protected int seed;                 // the random seed used for the entire generation process
        protected Random random;            // the random number generator which is used every time a random number is needed

        internal P1GenPaths()
        {
            // Upon creation, cache references to often used global generation-variables from LevelGenerator
            seed = LevelGenerator.seed;
            random = LevelGenerator.random;
        }

        /// <summary>
        /// This method will generate a grid, containing all the sections. The number for each section
        /// corresponds to the type. It will also output a list of sections on the main path
        /// </summary>
        internal override void Run()
        {
            // The process for Generating the paths is as follows:
            // 1. Select start section
            // 2. Find the path from the start section to the end

            // Select a start section
            SelectStartSection();

            // Find a path from the start to the bottom row and select end section
            FindPaths();

            // Produce the output
            Output();
        }

        private void SelectStartSection()
        {
            // Randomly select a section from the top row
            int start_section_id = random.Next(0, LevelGenerator.vert_sections);  // the top row has the location [0, hor_sections]

            // Set the start section
            LevelGenerator.start_section_id = start_section_id;
        }

        private void FindPaths()
        {
            // The process for finding a path is as follows:
            // 1. Pass over all sections and assign random section types
            // 2. Pass over all sections again, starting at the start section and find the path as follows:
            //    a) While still processing, select a random direct to move in
            //    b) Based on the direction, set the path type
            //    b) If that section is valid, add it to the main path
            //    c) Based on the direction and previous direction, select the section type of the cur and next sections
            //       i) If direction is left or right
            //          prev_direction = down   -> section is Landing (4), next is Normal (1)
            //          prev_direction = up     -> section is JumpLanding (5), next is Normal (1)
            //          default                 -> section is Normal (1), next is Normal (1)

            //       ii) If direction is up
            //          default                 -> section is Normal (1), next is Normal (1)

            //       iii) If direction is down
            //          prev_direction = down   -> section is Through (6), next is Landing (3)
            //          default                 -> section is Drop (2), next is Landing (3)

            //    d) If the newly added section is the end section, end


            // Pass over all section except start and end and set random section type
            Array array = Enum.GetValues(typeof(SectionType));
            List<SectionType> section_types = new List<SectionType>();
            foreach (SectionType c in array)
            {
                if (c != SectionType.None)
                    section_types.Add(c);
            }
            for (int i = 0; i < LevelGenerator.hor_sections; i++)
            {
                for (int j = 0; j < LevelGenerator.vert_sections; j++)
                {
                    // Get a random type of section
                    int type = random.Next(0, section_types.Count);

                    // Using the new type, set the section's type
                    LevelGenerator.level_section_grid[i, j] = section_types[type];
                }
            }

            //TODO: MAKE SURE TO SELECT THE STARTING SECTION IN THE LEVELGENERATOR

            // Continuously try to find a path from the start to the end recursively until a solution is found
            while (true)
            {
                bool still_processing = doFindSpelunkyPath(LevelGenerator.start_section_id, ' ');

                if (!still_processing)
                {
                    break;
                }
            }
        }

        private bool doFindSpelunkyPath(int cur_section_id, char prev_direction)
        {
            // If the section is null, terminate 
            if (cur_section_id == -1)
            {
                return true;
            }

            // Add the section to the path (it will be removed if the method backtracks)
            LevelGenerator.level_path.Add(cur_section_id);

            // Keep trying a random direction, until all have been tried or the value returns false
            bool processing;
            bool up = false;
            bool down = false;
            bool left = false;
            bool right = false;
            do
            {
                // Select a direction
                char direction = ' ';
                int next_section_id = -1;
                while (next_section_id < 0 && !(left && right && up && down))
                {
                    double chance = random.Next(0, 4);
                    switch (chance) // only try a direction that was not opposite the direction this section was entered from
                    {
                        // Try to select a direction based on the chance. A direction may not be selected if it has been selected already,
                        // or if the previous direction was opposite (i.e. no left if previous was right, no up if down, etc.)

                        case 0: // up
                            if (!up)    // if up has not yet been checked
                            {
                                if (prev_direction != 'd' && (cur_section_id < LevelGenerator.num_sections - LevelGenerator.vert_sections))  // if the section is in the last row, may not move up
                                {   // can only move up if previous direction was not down or it is not in the last row
                                    direction = 'u';
                                }
                                up = true;
                            }
                            break;
                        case 1: // down
                            if (!down)  // if down has not yet been checked
                            {
                                if (prev_direction != 'u')
                                {
                                    direction = 'd';
                                }
                            }
                            down = true;
                            break;
                        case 2: // left
                            if (!left)  // if left has not yet been checked
                            {
                                if (prev_direction != 'r')
                                {
                                    direction = 'l';
                                }
                                left = true;
                            }
                            break;
                        case 3: // right    
                            if (!right)     // if right has not yet been checked
                            {
                                if (prev_direction != 'l')
                                {
                                    direction = 'r';
                                }
                                right = true;
                            }
                            break;
                        default:
                            break;
                    }

                    // Get the associated section
                    next_section_id = LevelGenerator.GetSectionNeighbourID(cur_section_id, direction);
                    if (next_section_id >= 0)   // not -1 or -2
                    {
                        // Check if the section is on the path
                        if (LevelGenerator.IsSectionOnPath(next_section_id))    // TODO: CHECK IF THE SECTION IS ON THE PATH
                        {
                            next_section_id = -1;    // Don't try to go back along a path that has already been checked
                            continue;
                        }

                        if (direction == 'l' || direction == 'r')
                        {
                            // Check the previous direction 
                            if (prev_direction == 'd')
                            {
                                //cur_section_id.SetSectionType(Section.SectionType.Landing);
                                LevelGenerator.SetSectionType(cur_section_id, SectionType.Landing);  //TODO: Update the section type with a method
                            }
                            else if (prev_direction == 'u')
                            {
                                //cur_section_id.SetSectionType(Section.SectionType.JumpLanding);
                                LevelGenerator.SetSectionType(cur_section_id, SectionType.JumpLanding);
                            }
                            else if (prev_direction == ' ')
                            {   // special case for when the first movement is down, and next is left
                                //cur_section_id.SetSectionType(Section.SectionType.Drop);
                                LevelGenerator.SetSectionType(cur_section_id, SectionType.Drop);
                            }
                            else    // direction was left or right
                            {
                                // cur_section_id.SetSectionType(Section.SectionType.Normal);
                                LevelGenerator.SetSectionType(cur_section_id, SectionType.Normal);
                            }
                            // next_section_id.SetSectionType(Section.SectionType.Normal);
                            LevelGenerator.SetSectionType(next_section_id, SectionType.Normal);
                        }
                        else if (direction == 'u')
                        {
                            // cur_section_id.SetSectionType(Section.SectionType.Jump);
                            // next_section_id.SetSectionType(Section.SectionType.JumpLanding);
                            LevelGenerator.SetSectionType(cur_section_id, SectionType.Jump);
                            LevelGenerator.SetSectionType(next_section_id, SectionType.JumpLanding);

                            // TODO: Make sure to add the appropriate entrances and exits based on the directions
                        }
                        else if (direction == 'd')
                        {
                            if (prev_direction == 'd')    // if the player moves through 2 levels down, the cur section must be Through
                            {
                                // cur_section_id.SetSectionType(Section.SectionType.Through);
                                LevelGenerator.SetSectionType(cur_section_id, SectionType.Through);
                            }
                            else
                            {
                                // cur_section_id.SetSectionType(Section.SectionType.Drop);
                                LevelGenerator.SetSectionType(cur_section_id, SectionType.Drop);
                            }
                            // next_section_id.SetSectionType(Section.SectionType.Landing);
                            LevelGenerator.SetSectionType(next_section_id, SectionType.JumpLanding);
                            // TODO: Make sure to add the appropriate entrances and exits based on the directions
                        }
                    }

                    // If the direction is down, check to see if the path should end
                    if (direction == 'd')
                    {
                        // When a section attempts to move down, if the section is in the last row (id >= num_sections - vert_sections && id < num_sections)
                        if ((cur_section_id >= LevelGenerator.num_sections - LevelGenerator.vert_sections) && (cur_section_id < LevelGenerator.num_sections))
                        {
                            // Make this section the end section, and finish processing
                            // LevelGenerator.SetEndSection(cur_section_id);
                            LevelGenerator.end_section_id = cur_section_id;   // TODO: Set the end section
                            return false;   // method is not still processing, so return false
                        }
                    }
                }

                // Move to next section
                processing = doFindSpelunkyPath(next_section_id, direction);

                // If processing is still true
                if (processing)
                {
                    // If all directions have been checked, then return to previous section and try different direction
                    if (up && down && left && right)
                    {
                        // Remove the current section from the path
                        LevelGenerator.level_path.Remove(cur_section_id);

                        return true;    // still processing
                    }

                    // If not all directions have been checked, method will try a new direction
                }
                else
                {
                    // Processing is finished, simply return to previous
                    return false;
                }
            } while (true);
        }


        /// <summary>
        /// This method will produce the output for Phase 1: Generate Paths, which is a grid of all sections with their types, and a list of sections on the main path
        /// </summary>
        private void Output()
        {
            // Output the section type grid
            PrintGridOfSections();

            // Output the path list
            PrintPath();
        }


        private void PrintPath()
        {

            // Create new directory
            string new_directory = @".\Assets\Resources\kjarmie\LevelGenerator\outputs\P1\" + seed + @"\";
            Directory.CreateDirectory(new_directory);

            if (File.Exists(new_directory + "paths.txt"))
            {
                File.Delete(new_directory + "paths.txt");
            }

            StreamWriter writer = new StreamWriter(new_directory + "paths.txt");

            int i;
            for (i = 0; i < LevelGenerator.level_path.Count - 1; i++)
            {
                writer.Write(LevelGenerator.level_path[i] + ",");
            }
            writer.Write(LevelGenerator.level_path[i]);
            writer.Flush();
            writer.Close();
        }

        // Method outputs all graph of links between sections
        private void PrintGridOfSections()
        {
            // Create new directory
            string new_directory = @".\Assets\Resources\kjarmie\LevelGenerator\outputs\P1\" + seed + @"\";
            Directory.CreateDirectory(new_directory);

            if (File.Exists(new_directory + "section_grid.txt"))
            {
                File.Delete(new_directory + "section_grid.txt");
            }

            StreamWriter writer = new StreamWriter(new_directory + "section_grid.txt");

            int id = 0;
            int i;
            for (i = 0; i < LevelGenerator.hor_sections; i++)
            {
                for (int j = 0; j < LevelGenerator.vert_sections; j++)
                {
                    writer.Write((int)LevelGenerator.level_section_grid[i, j]);
                    id++;
                }
                writer.Write("\n");
            }
            writer.Close();
        }
    }
}