using UnityEngine;
using Fusion;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Reference")]
    [SerializeField] private NetworkCharacterController playerController;
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

    [Networked] private NetworkButtons PreviousButtons { get; set; }

    private void Awake()
    {
        if (HasInputAuthority)
        {
            mainCamera = Camera.main; // Lokalny gracz używa swojej kamery
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Rotate Toward
        Debug.Log($"Rotate Toward");
        RotateTowardsCursor();

        if (GetInput(out NetInput input))
        {
            // Log input direction and buttons pressed
            Debug.Log($"Input received. Direction: {input.Direction}, Buttons: {input.Buttons}");

            Vector3 worldDirection = new Vector3(input.Direction.x, 0f, input.Direction.y);

            // Log world direction
            Debug.Log($"World Direction: {worldDirection}");


            // Log if jump button was pressed
            bool jumpPressed = input.Buttons.WasPressed(PreviousButtons, InputButton.Jump);
            Debug.Log($"Jump pressed: {jumpPressed}");

            ApplyMovement(worldDirection, jumpPressed);

            // Log previous buttons state
            Debug.Log($"Previous Buttons: {PreviousButtons}");

            PreviousButtons = input.Buttons;
        }
        else
        {
            // Log if no input is received
            Debug.Log("No input received.");
        }
    }


    /*    private void Update()
        {
            if (!HasInputAuthority) return;

            ApplyMovement();
        }

        private void LateUpdate()
        {
            if (!HasInputAuthority) return;

            RotateTowardsCursor();
        }*/

    private void ApplyMovement(Vector3 worldDirection, bool isJump)
    {
        // Log the input world direction and whether jump is pressed
        Debug.Log($"World Direction: {worldDirection}, Is Jump: {isJump}");

        // Smooth acceleration and deceleration for horizontal movement
        if (worldDirection.magnitude > 0)
        {
            currentMoveVelocity = Vector3.Lerp(currentMoveVelocity, worldDirection * moveSpeed, moveAcceleration);
            Debug.Log($"Current Move Velocity (accelerating): {currentMoveVelocity}");
        }
        else
        {
            currentMoveVelocity = Vector3.Lerp(currentMoveVelocity, Vector3.zero, moveBreak);
            Debug.Log($"Current Move Velocity (breaking): {currentMoveVelocity}");
        }

        // Handle vertical movement (gravity and jump)
        if (IsGrounded() && verticalVelocity < 0)
        {
            verticalVelocity = -1f;
            Debug.Log("Player is grounded, setting vertical velocity to -1.");
        }

        if (isJump && IsGrounded())
        {
            verticalVelocity += jumpAcceleration;
            Debug.Log($"Jumping! Vertical Velocity after jump: {verticalVelocity}");
        }
        else
        {
            verticalVelocity += gravityValue * Time.deltaTime;
            Debug.Log($"Gravity applied. Vertical Velocity: {verticalVelocity}");
        }

        // Combine horizontal and vertical movement
        Vector3 movement = currentMoveVelocity * Time.deltaTime;
        movement.y = verticalVelocity * Time.deltaTime;

        // Log movement vector before applying
        Debug.Log($"Movement vector: {movement}");

        playerController.Move(movement);
    }


    private void RotateTowardsCursor()
    {
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
        return playerController.Grounded;
    }
}
