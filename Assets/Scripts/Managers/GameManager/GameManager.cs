using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tarodev;
using LevelGenerator;
using LevelGenerator.Tiles;

namespace Bounce
{
    /// <summary>
    /// Game manager for Bounce. It acts as a central control area for the entire game. Handles the changes in game state,
    /// and any other controls. Uses enums for state changes. Based on the manager from Tarodev:
    /// https://www.patreon.com/tarodev
    /// https://www.youtube.com/watch?v=4I0vonyqMi8
    /// </summary>
    public class GameManager : StaticInstance<GameManager>
    {
        // Prefab and View Variables
        [SerializeField] private TileView tileViewPrefab;
        [SerializeField] private PlayerController player_avatar;
        [SerializeField] private Camera game_camera;

        // UI Variables
        [SerializeField] GameObject pause_panel;
        [SerializeField] GameObject win_panel;
        [SerializeField] GameObject pause_button;
        public static bool isPaused;
        public static Vector3 startLocation;

        // Terrain Variables
        [SerializeField] private GameObject terrain;
        [SerializeField] private GameObject blockers;
        [SerializeField] private int view_width, view_height;
        private TileView[,] tileview_grid;

        // Level Generation Variables
        int seed;
        public static LevelSize level_size;

        public void OnPauseClicked()
        {
            // Run pause menu from Brackeys
            pause_panel.SetActive(true);
            pause_button.SetActive(false);

            isPaused = true;

            Time.timeScale = 1f;
        }

        public void OnResumeClicked()
        { // continue the game
            pause_panel.gameObject.SetActive(false);
            pause_button.gameObject.SetActive(true);



            isPaused = false;

            Time.timeScale = 1f;
        }

        public void OnRestartClicked()
        { // move the player back to the beginning of the level
            player_avatar.transform.position = startLocation;

            OnResumeClicked();
        }

        public void OnNewGameClicked()
        {
            // Create a new game level
            SceneManager.LoadScene((int)Scenes.Play);
        }

        public void OnExitClicked()
        {   // exit to the main menu
            int mainMenu = (int)Scenes.MainMenu;

            SceneManager.LoadScene(mainMenu);
        }

        public static event Action<GameState> OnBeforeStateChanged;
        public static event Action<GameState> OnAfterStateChanged;

        public GameState State { get; private set; }

        // START THE GAME
        void Start()
        {
            // Reset the game variables
            OnResumeClicked();

            // Hide the win panel
            win_panel.gameObject.SetActive(false);

            // Set the game state to Initialize
            ChangeState(GameState.Initialize);
        }

        /// <summary>
        /// This method is used to change the game state to a new state.
        /// </summary>
        /// <param name="newState">The new game state. Select from enum GameState. </param>
        public void ChangeState(GameState newState)
        {
            OnBeforeStateChanged?.Invoke(newState);

            State = newState;
            switch (newState)
            {
                case GameState.Initialize:
                    HandlePreInit();
                    break;
                case GameState.GenerateLevel:
                    HandleLevelGeneration();
                    break;
                case GameState.CreateGameView:
                    HandleGameViewCreation();
                    break;
                case GameState.SpawnPlayer:
                    HandleSpawningPlayer();
                    break;
                case GameState.Playing:
                    HandlePlaying();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }

            OnAfterStateChanged?.Invoke(newState);

            Debug.Log($"New state: {newState}");
        }

        private void HandlePreInit()
        {
            // Here, we clear the output folder of the level generator to ensure storage doesnt fill up unnecessarily
            String path_name = @"./Assets/Resources/kjarmie/LevelGenerator/output/level";
            System.IO.DirectoryInfo di = new DirectoryInfo(path_name);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }


            // Update the game state to generate the level
            ChangeState(GameState.GenerateLevel);
        }

        /// <summary>
        /// This method starts the level generation. It does not actually perform any logic.
        /// </summary>
        private void HandleLevelGeneration()
        {
            // Perform the level generation
            seed = new System.Random().Next();
            //seed = 823463766;
            LevelGenerator.LevelGenerator.GenerateLevel(seed, level_size);

            // Once the level is generated, move to create the game view and update game state
            ChangeState(GameState.CreateGameView);
        }

        // <summary>
        // This method uses the newly created GameLevel to populate the game world
        // </summary>
        private void HandleGameViewCreation()
        {
            // Create the GameLevel manager
            GameLevel.CreateGameLevelFromFile(@".\Assets\Resources\kjarmie\LevelGenerator\outputs\level\" + seed + @"level.txt");

            // Create some dummy tiles that frame the level
            for (int x = -10; x < GameLevel.cols + 10; x++)
            {
                for (int y = -10; y < GameLevel.rows + 10; y++)
                {
                    // Only place blockers at positions if they are not in the bounds of the level
                    if (!(x > 0 && x < GameLevel.cols && y > 0 && y < GameLevel.rows))
                    {
                        TileView tile_view = Instantiate(tileViewPrefab, new Vector3(x, -y), Quaternion.identity);
                        tile_view.Init(new Tile(TileArchetype.None, TileType.None, -1, -1));
                        tile_view.name = String.Format("Blocker");
                        tile_view.transform.parent = blockers.transform;
                    }
                }
            }

            // Initialize the TileView grid
            tileview_grid = new TileView[GameLevel.rows, GameLevel.cols];

            int k = 0;
            // For each tile in the GameLevel grid, create a new TileView for it
            for (int i = 0; i < GameLevel.rows; i++)
            {
                for (int j = 0; j < GameLevel.cols; j++)
                {
                    // Get the tile
                    Tile tile = GameLevel.level_grid[i, j];

                    if (tile == null)
                    {
                        k++;
                        Debug.Log(String.Format(@"Tile ({0}, {1})", i, j));
                    }

                    // Create a new TileView
                    TileView tile_view = Instantiate(tileViewPrefab, new Vector3(j, -i), Quaternion.identity);

                    // Give it a name and add it to the Terrain object
                    tile_view.name = String.Format(@"Tile ({0}, {1})", i, j);
                    tile_view.transform.parent = terrain.transform;

                    // Instantiate the TileView
                    tile_view.Init(tile);

                    // Add it to the TileView grid
                    tileview_grid[i, j] = tile_view;
                }
            }

            // Fix the camera 
            //GetComponent<Camera>().transform.position = new Vector3((float)view_width / 2 - 0.5f, (float)view_height / 2 - 0.5f, -200);

            // Ensure the level is bordered
            //TODO: add some of the dummy tiles around the entire level

            // Change the game state to spawn player
            ChangeState(GameState.SpawnPlayer);

        }

        /// <summary>
        /// This method find the spawn location and places the player avatar there.
        /// </summary>
        private void HandleSpawningPlayer()
        {
            // Find the spawn location (in the start section)
            Tile startTile = GameLevel.start_tile;

            // Spawn the player avatar
            startLocation = new Vector2(startTile.col, -startTile.row);
            player_avatar.transform.position = startLocation;

            // Change game state to playing
            ChangeState(GameState.Playing);
        }

        private void HandlePlaying()
        {

        }


        private void Update()
        {
            // Check if the player is at the finish
            float x = player_avatar.transform.position.x;
            float y = player_avatar.transform.position.y;

            float end_x = GameLevel.end_tile.col;
            float end_y = - GameLevel.end_tile.row;

            if (x >= end_x - 0.5 && x < end_x + 0.5 && y >= end_y - 0.5 && y < end_y + 0.5)
            {
                // The player is at the end location, so we end the game
                isPaused = true;
                win_panel.gameObject.SetActive(true);

            }
        }
    }

    /// <summary>
    /// The set of possible game states
    /// </summary>
    [Serializable]
    public enum GameState
    {
        Initialize = 0,
        GenerateLevel = 1,
        CreateGameView = 2,
        SpawnPlayer = 3,
        Playing = 4
    }
}