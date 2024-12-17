using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPun
{
    [Header("Reference")]
    [SerializeField] private CharacterController playerController;
    [SerializeField] private Camera mainCamera; // Dodano referencję do kamery
    [SerializeField] private PlayerInputReader inputReader;

    // Movement settings
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveAcceleration = 0.1f;
    [SerializeField] private float moveBreak = 0.1f;

    // Jump and gravity settings
    [Header("Jump and Gravity Settings")]
    [SerializeField] private float jumpAcceleration = 0.2f;
    [SerializeField] private float gravityValue = -9.81f;

    // Cursor settings
    [Header("Cursor Settings")]
    [SerializeField] private LayerMask groundLayer; // Warstwa gruntu, na którą patrzymy
    private Vector3 currentMoveVelocity;
    private float verticalVelocity;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            // Nie wykonuj ruchu dla obiektów innych graczy
            return;
        }
        ApplyMovement();
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine)
        {
            // Nie wykonuj ruchu dla obiektów innych graczy
            return;
        }
        RotateTowardsCursor(); 
    }

    private void ApplyMovement()
    {
        // Smooth acceleration and deceleration for horizontal movement
        if (inputReader.MoveInput.magnitude > 0)
        {
            currentMoveVelocity = Vector3.Lerp(currentMoveVelocity, inputReader.MoveInput * moveSpeed, moveAcceleration);
        }
        else
        {
            currentMoveVelocity = Vector3.Lerp(currentMoveVelocity, Vector3.zero, moveBreak);
        }

        // Handle vertical movement (gravity and jump)
        if (IsGrounded() && verticalVelocity < 0)
        {
            verticalVelocity = -1f;
        }

        if (inputReader.IsJumping && IsGrounded())
        {
            verticalVelocity += jumpAcceleration;
        }
        else
        {
            verticalVelocity += gravityValue * Time.deltaTime;
        }

        // Combine horizontal and vertical movement
        Vector3 movement = currentMoveVelocity * Time.deltaTime;
        movement.y = verticalVelocity * Time.deltaTime;

        playerController.Move(movement);
    }

    private void RotateTowardsCursor()
    {
        if(!IsGrounded())
        if (mainCamera == null) return;

        // Rzutuj promień od kursora w przestrzeni ekranu do świata gry
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // Wyznacz kierunek od gracza do punktu kursora
            Vector3 direction = hit.point - transform.position;
            direction.y = 0; // Ignoruj komponent osi Y (gracz patrzy tylko w poziomie)

            // Obróć gracza w kierunku kursora
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.1f); // Gładka rotacja
            }
        }
    }

    private bool IsGrounded()
    {
        return playerController.isGrounded;
    }
}
