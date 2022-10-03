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
    public class EnemyManager : StaticInstance<EnemyManager>
    {
        // Enemy AI Variables
        public static GameObject Enemies;
        public static Skeleton skeleton;

        static public Dictionary<string, Sprite> skeleton_assets; // controls all assets for the skeleton enemy

        private void Awake() {
            Enemies = GameObject.Find("Enemies");
            skeleton = GameObject.FindObjectOfType<Skeleton>();
        }

        void Start()
        {
            // Here, the manager will load all assets needed for the game
            Sprite skeleton_left = Resources.Load<Sprite>("kjarmie/Art/Enemies/skeleton_left");
            Sprite skeleton_right = Resources.Load<Sprite>("kjarmie/Art/Enemies/skeleton_right");

            skeleton_assets = new Dictionary<string, Sprite>();
            skeleton_assets.Add("Left", skeleton_left);
            skeleton_assets.Add("Right", skeleton_right);
        }

        public static void SpawnSkeleton(int row, int col)
        {
            Enemies = GameObject.Find("Enemies");
            skeleton = GameObject.FindObjectOfType<Skeleton>();
            Skeleton new_skeleton = (Skeleton) Instantiate(skeleton, new Vector3(col, -row), Quaternion.identity);
            new_skeleton.transform.parent = Enemies.transform;
        }
    }
}