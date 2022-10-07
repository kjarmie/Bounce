using UnityEngine;
using Random = UnityEngine.Random;

namespace Bounce
{
    /// <summary>
    /// This is a pretty filthy script. I was just arbitrarily adding to it as I went.
    /// You won't find any programming prowess here.
    /// This is a supplementary script to help with effects and animation. Basically a juice factory.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        private IPlayerController _player;
        private bool _playerGrounded;
        private ParticleSystem.MinMaxGradient _currentGradient;
        private Vector2 _movement;

        void Awake() => _player = GetComponentInParent<IPlayerController>();

        void Update()
        {
            if (_player == null) return;

            // Flip the sprite
            if (_player.Input.X != 0)
                transform.localScale = new Vector3(_player.Input.X > 0 ? (float)0.6 : (float)-0.6, (float)0.6, 0);

            _movement = _player.RawMovement; // Previous frame movement is more valuable
        }

        private void OnDisable()
        {
            //_moveParticles.Stop();
        }

        private void OnEnable()
        {
            //_moveParticles.Play();
        }

        void SetColor(ParticleSystem ps)
        {
            var main = ps.main;
            main.startColor = _currentGradient;
        }

        #region Animation Keys

        private static readonly int GroundedKey = Animator.StringToHash("Grounded");
        private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
        private static readonly int JumpKey = Animator.StringToHash("Jump");

        #endregion
    }
}