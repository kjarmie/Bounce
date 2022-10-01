using System;
using UnityEngine;
using System.Collections.Generic;
using Tarodev;

namespace Bounce
{
    /// <summary>
    /// Nice, easy to understand enum-based game manager. For larger and more complex games, look into
    /// state machines. But this will serve just fine for most games.
    /// </summary>
    public class EnemyManager : StaticInstance<ExampleGameManager>
    {
        static public Dictionary<string, Sprite> skeletons; // controls all assets for the skeleton enemy
        // Kick the game off with the first state
        void Start()
        {
            // Here, the manager will load all assets needed for the game
            Sprite skeleton_left = Resources.Load<Sprite>("kjarmie/Art/Enemies/skeleton_left");
            Sprite skeleton_right = Resources.Load<Sprite>("kjarmie/Art/Enemies/skeleton_right");

            skeletons = new Dictionary<string, Sprite>();
            skeletons.Add("Left", skeleton_left);
            skeletons.Add("Right", skeleton_right);
        }

    }


}