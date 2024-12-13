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

        RotateTowardsCursor();
        ApplyMovement();
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

        // Apply gravity
        if (_controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // Keep grounded
        }
        else
        {
            _velocity.y += GravityValue * Time.deltaTime;
        }

        // Apply jump force if triggered by PlayerInputReader
        if (_inputReader.IsJumping && _controller.isGrounded)
        {
            _velocity.y = JumpForce;
        }

        // Combine horizontal and vertical velocities
        Vector3 movement = _currentMoveVelocity * Time.deltaTime;
        movement.y += _velocity.y * Time.deltaTime;


        _controller.Move(movement);

    }

    private void RotateTowardsCursor()
    {
        if (_mainCamera == null)
        {
            _mainCamera = FindAnyObjectByType<Camera>();
            Debug.LogError("Main camera not found. Unable to rotate towards cursor.");
            return;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.Log($"Raycast from cursor: Origin = {ray.origin}, Direction = {ray.direction}");

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, GroundLayer))
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance);
            Vector3 direction = hit.point - transform.position;
            Debug.Log($"Raycast hit: {hit.point}, Direction to cursor: {direction}");

            direction.y = 0; // Ignore vertical component

            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.1f);
                Debug.Log($"Rotating towards cursor: Target Rotation = {targetRotation}, Current Rotation = {transform.rotation}");
            }
            else
            {
                Debug.Log("Direction to cursor too small to rotate.");
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * 10000f);
            Debug.LogWarning("Cursor raycast did not hit any valid object.");
        }
    }
}
