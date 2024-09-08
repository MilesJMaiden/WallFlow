using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class RandomOppyBehavior : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f; // Speed of movement
    [SerializeField] private float movementChangeInterval = 2f; // Interval to change direction
    [SerializeField] private float movementRadius = 6f; // Radius limit for movement from the start position

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f; // Jump force
    [SerializeField] private float jumpInterval = 5f; // Interval to perform a jump
    [SerializeField] private float maxJumpHeight = 1.5f; // Maximum jump height
    [SerializeField] private float gravity = -9.8f; // Gravity value for downward force

    [Header("Timer Settings")]
    [SerializeField] private float activeDuration = 30f; // Duration in seconds before the NPC is deactivated

    private Animator _animator;
    private CharacterController _characterController;
    private Vector3 _startPosition;
    private Vector3 _moveDirection = Vector3.zero;
    private Vector3 _velocity = Vector3.zero;
    private bool _isGrounded;
    private JumpingState _jumpingState = JumpingState.Grounded;
    private Coroutine _deactivationCoroutine;

    private static readonly int Jumping = Animator.StringToHash("Jumping");
    private static readonly int Landed = Animator.StringToHash("Landed");
    private static readonly int Running = Animator.StringToHash("Running");

    private const float JumpDelay = 0.16f;

    private enum JumpingState
    {
        Grounded,
        JumpStarted,
        JumpedAndAirborne
    }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _startPosition = transform.position; // Set the start position to limit movement range
    }

    private void Start()
    {
        StartCoroutine(ChangeMovementDirection());
        StartCoroutine(RandomJump());
        StartDeactivationTimer(); // Start the timer for deactivation
    }

    private void Update()
    {
        HandleMovement();
        ApplyGravity(); // Apply gravity continuously
        CheckGroundStatus();
        UpdateAnimator();
        ConstrainMovement(); // Ensure NPC does not move too far from the start position
    }

    private void HandleMovement()
    {
        // Apply movement based on the current move direction
        if (_moveDirection != Vector3.zero)
        {
            Vector3 move = _moveDirection * moveSpeed * Time.deltaTime;
            _characterController.Move(move);

            // Rotate to face the movement direction
            if (move != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_moveDirection), Time.deltaTime * 5f);
            }
        }
    }

    private void ApplyGravity()
    {
        // Apply gravity manually to the NPC when not grounded
        if (!_isGrounded)
        {
            _velocity.y += gravity * Time.deltaTime;
        }
        else if (_velocity.y < 0)
        {
            _velocity.y = 0f; // Reset velocity when grounded
        }

        // Apply the velocity to move the NPC
        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        // Update running animation
        bool isMoving = _moveDirection.magnitude > 0.1f && _isGrounded;
        _animator.SetBool(Running, isMoving);

        // Handle landing and resetting to grounded state
        if (_isGrounded && _jumpingState == JumpingState.JumpedAndAirborne)
        {
            _animator.SetTrigger(Landed);
            _jumpingState = JumpingState.Grounded;
        }
    }

    private void CheckGroundStatus()
    {
        // Use CharacterController's isGrounded to check if the NPC is on the ground
        _isGrounded = _characterController.isGrounded || transform.position.y <= 0.05f; // Considering ground at Y = 0 with tolerance

        if (_isGrounded && _jumpingState != JumpingState.Grounded)
        {
            _jumpingState = JumpingState.Grounded;
            _animator.SetTrigger(Landed);
            _velocity.y = 0f; // Reset vertical velocity on landing
            transform.position = new Vector3(transform.position.x, 0, transform.position.z); // Snap to ground level
        }
    }

    private void ConstrainMovement()
    {
        // Keep the NPC within a certain radius of the start position
        Vector3 offset = transform.position - _startPosition;
        if (offset.magnitude > movementRadius)
        {
            _moveDirection = -offset.normalized; // Move back towards the start position
        }
    }

    private IEnumerator ChangeMovementDirection()
    {
        while (true)
        {
            // Randomly change movement direction at set intervals
            _moveDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            yield return new WaitForSeconds(movementChangeInterval);
        }
    }

    private IEnumerator RandomJump()
    {
        while (true)
        {
            // Perform a jump at random intervals if grounded
            yield return new WaitForSeconds(Random.Range(1f, jumpInterval));
            if (_isGrounded)
            {
                _animator.SetTrigger(Jumping);
                _jumpingState = JumpingState.JumpStarted;
                _velocity.y = jumpForce; // Apply upward force for jumping

                // Wait for the jump to register before allowing gravity to act
                yield return new WaitForSeconds(JumpDelay);

                // Ensure the NPC starts descending after reaching the max height
                if (transform.position.y >= maxJumpHeight)
                {
                    _jumpingState = JumpingState.JumpedAndAirborne;
                    _velocity.y = gravity * Time.deltaTime; // Start applying gravity to descend
                }
            }
        }
    }

    private IEnumerator DeactivateAfterTime()
    {
        // Wait for the specified active duration
        yield return new WaitForSeconds(activeDuration);
        // Set the NPC's active state to false
        gameObject.SetActive(false);
    }

    private void StartDeactivationTimer()
    {
        // Start the deactivation timer coroutine
        _deactivationCoroutine = StartCoroutine(DeactivateAfterTime());
    }

    public void ActivateAndRestartTimer()
    {
        // Reactivate the NPC
        gameObject.SetActive(true);

        // Restart the deactivation timer
        if (_deactivationCoroutine != null)
        {
            StopCoroutine(_deactivationCoroutine);
        }
        StartDeactivationTimer();
    }
}
