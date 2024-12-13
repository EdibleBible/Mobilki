using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    private CharacterController _controller;
    private PlayerInputReader _inputReader;

    [Header("Movement Settings")]
    public float PlayerSpeed = 5f;
    public float MoveAcceleration = 0.1f;
    public float MoveBreak = 0.1f;

    [Header("Jump and Gravity Settings")]
    public float JumpForce = 5f;
    public float GravityValue = -9.81f;

    [Header("Cursor Settings")]
    public LayerMask GroundLayer;

    private Vector3 _velocity;
    private Vector3 _currentMoveVelocity;
    private Camera _mainCamera;

    private void OnEnable()
    {
        _controller = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        _inputReader = GetComponent<PlayerInputReader>();

        if(_mainCamera == null)
            _mainCamera = FindAnyObjectByType<Camera>();
    }

    public override void FixedUpdateNetwork()
    {
        if (_inputReader == null) return;

        ApplyMovement();
        RotateTowardsCursor();

    }

    private void Update()
    {
        FixedUpdateNetwork();
    }
    private void ApplyMovement()
    {
        // Get movement input from PlayerInputReader
        Vector3 inputDirection = _inputReader.MoveInput;

        // Smooth acceleration and deceleration
        if (inputDirection.magnitude > 0)
        {
            _currentMoveVelocity = Vector3.Lerp(_currentMoveVelocity, inputDirection * PlayerSpeed, MoveAcceleration);
        }
        else
        {
            _currentMoveVelocity = Vector3.Lerp(_currentMoveVelocity, Vector3.zero, MoveBreak);
        }

        Debug.Log(inputDirection.magnitude);

        // Apply gravity
/*        if (_controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // Keep grounded
        }
        else
        {
            _velocity.y += GravityValue * NetworkController.runnerInstance.DeltaTime;
        }

        // Apply jump force if triggered by PlayerInputReader
        if (_inputReader.IsJumping && _controller.isGrounded)
        {
            _velocity.y = JumpForce;
        }*/

        // Combine horizontal and vertical velocities
        Vector3 movement = _currentMoveVelocity * NetworkController.runnerInstance.DeltaTime;
        movement.y += _velocity.y * NetworkController.runnerInstance.DeltaTime;

        Debug.Log(movement.magnitude);

        _controller.Move(movement);

    }

    private void RotateTowardsCursor()
    {

        PhysicsScene physicsScene = Runner.GetPhysicsScene();
        if (physicsScene.IsValid())
        {

            if (_mainCamera == null)
            {
                _mainCamera = FindAnyObjectByType<Camera>();
                return;
            }

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (physicsScene.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity, GroundLayer))
            {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance);
                Vector3 direction = hit.point - transform.position;

                direction.y = 0; // Ignore vertical component

                if (direction.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.1f);
                }
            }

        }
    }
}
