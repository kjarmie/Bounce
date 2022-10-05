using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bounce
{

    // This class is the AI for a Skeleton type enemy
    public class Skeleton : MonoBehaviour
    {
        [SerializeField] SpriteRenderer renderer;

        double speed = 0.005;    // the number of units the enemy moves per frame
        int direction = 1;      // 1 for right, -1 for left

        int max_frames_paused = 100; // 1 second in Unity is 50 frames
        int frames_paused = 0;  // the number of frames the enemy has been paused for
        bool paused = false;    // determines whether the enemy is still paused

        // Start is called before the first frame update
        void Start()
        {
            // Load any variables and necessary data
            renderer.sprite = EnemyManager.skeleton_assets.GetValueOrDefault("Right");
        }

        // Update is called once per frame
        void Update()
        {
            // Here, the AI of the skeleton is processed

            // The skeleton AI is very simple. It will move (left or right) until it hits the edge of a block. It will then pause for 1 second,
            // then turn and move the other direction, and repeats.

            // If not paused, continue
            if (!paused)
            {
                transform.position += new Vector3((float)(direction * speed), 0);

                // Check to see if it is on the edge of a block
                Vector2 angle;
                if (direction == 1)
                    angle = new Vector2(transform.position.x + 1, transform.position.y + 2);
                else
                    angle = new Vector2(-transform.position.x - 1, transform.position.y + 2);

                // Debug.DrawRay(transform.position, angle, Color.blue, 1);
                RaycastHit hit;
                bool isHit = Physics.Raycast(transform.position, angle, out hit);
                bool on_edge = !Physics.Raycast(transform.position, angle, (float)0.01, LayerMask.NameToLayer("Default"));
                // Debug.Log("Position x: " + transform.position.x);
                // Debug.Log("Position y: " + transform.position.y);

                // Debug.Log(message: "Angle x: " + angle.x);
                // Debug.Log("Angle y: " + angle.y);

                // If not, move in the same direction
                if (!on_edge)
                {
                    transform.position += new Vector3((float)(direction * speed), transform.position.y);
                }

                // Else, pause
                else
                {
                    paused = true;

                }
            }
            // else, increment the number of frames paused
            else
            {
                frames_paused++;

                // check if the AI should be unpaused
                if (frames_paused >= max_frames_paused)
                {
                    // If so, reverse the direction
                    paused = false;
                    if (direction == 1)
                    {
                        direction = -1;
                        renderer.sprite = EnemyManager.skeleton_assets.GetValueOrDefault("Left");
                    }
                    else
                    {
                        direction = 1;
                        renderer.sprite = EnemyManager.skeleton_assets.GetValueOrDefault("Right");
                    }
                    frames_paused = 0;
                }
            }
        }
    }
}
