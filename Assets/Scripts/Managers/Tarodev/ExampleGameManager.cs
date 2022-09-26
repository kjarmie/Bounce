using System;
using UnityEngine;

namespace Tarodev
{
    /// <summary>
    /// Nice, easy to understand enum-based game manager. For larger and more complex games, look into
    /// state machines. But this will serve just fine for most games.
    /// </summary>
    public class ExampleGameManager : StaticInstance<ExampleGameManager>
    {
        public static event Action<ExampleGameState> OnBeforeStateChanged;
        public static event Action<ExampleGameState> OnAfterStateChanged;

        public ExampleGameState State { get; private set; }

        // Kick the game off with the first state
        void Start() => ChangeState(ExampleGameState.Starting);

        public void ChangeState(ExampleGameState newState)
        {
            OnBeforeStateChanged?.Invoke(newState);

            State = newState;
            switch (newState)
            {
                case ExampleGameState.Starting:
                    HandleStarting();
                    break;
                case ExampleGameState.SpawningHeroes:
                    HandleSpawningHeroes();
                    break;
                case ExampleGameState.SpawningEnemies:
                    HandleSpawningEnemies();
                    break;
                case ExampleGameState.HeroTurn:
                    HandleHeroTurn();
                    break;
                case ExampleGameState.EnemyTurn:
                    break;
                case ExampleGameState.Win:
                    break;
                case ExampleGameState.Lose:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }

            OnAfterStateChanged?.Invoke(newState);

            Debug.Log($"New state: {newState}");
        }

        private void HandleStarting()
        {
            // Do some start setup, could be environment, cinematics etc

            // Eventually call ChangeState again with your next state

            ChangeState(ExampleGameState.SpawningHeroes);
        }

        private void HandleSpawningHeroes()
        {
            // ExampleUnitManager.Instance.SpawnHeroes();

            ChangeState(ExampleGameState.SpawningEnemies);
        }

        private void HandleSpawningEnemies()
        {

            // Spawn enemies

            ChangeState(ExampleGameState.HeroTurn);
        }

        private void HandleHeroTurn()
        {
            // If you're making a turn based game, this could show the turn menu, highlight available units etc

            // Keep track of how many units need to make a move, once they've all finished, change the state. This could
            // be monitored in the unit manager or the units themselves.
        }
    }

    /// <summary>
    /// This is obviously an example and I have no idea what kind of game you're making.
    /// You can use a similar manager for controlling your menu states or dynamic-cinematics, etc
    /// </summary>
    [Serializable]
    public enum ExampleGameState
    {
        Starting = 0,
        SpawningHeroes = 1,
        SpawningEnemies = 2,
        HeroTurn = 3,
        EnemyTurn = 4,
        Win = 5,
        Lose = 6,
    }
}