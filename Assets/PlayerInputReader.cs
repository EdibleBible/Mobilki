using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    private PlayerInputActions playerActions;

    public Vector3 MoveInput { get; private set; } = Vector3.zero;
    public bool IsJumping { get; private set; }

    public event System.Action OnInteractPerformed;
    public event System.Action OnChargeStarted;
    public event System.Action OnChargeCanceled;

    private void Awake()
    {
        if (playerActions == null)
            playerActions = new PlayerInputActions();

        // Register movement events
        playerActions.Player.Move.performed += OnMovePerformed;
        playerActions.Player.Move.canceled += OnMoveCanceled;

        // Register jump events
        playerActions.Player.Jump.performed += OnJumpPerformed;
        playerActions.Player.Jump.canceled += OnJumpCanceled;

        // Register interact events
        playerActions.Player.Interact.performed += OnInteractPerform;

        // Register charge events
        playerActions.Player.Charge.started += OnChargeStartedInternal;
        playerActions.Player.Charge.canceled += OnChargeCanceledInternal;
    }

    private void OnEnable()
    {
        playerActions?.Enable();
    }

    private void OnDisable()
    {
        playerActions?.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        MoveInput = new Vector3(input.x, 0, input.y);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        MoveInput = Vector3.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        IsJumping = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        IsJumping = false;
    }

    private void OnInteractPerform(InputAction.CallbackContext context)
    {
        OnInteractPerformed?.Invoke();
    }

    private void OnChargeStartedInternal(InputAction.CallbackContext context)
    {
        OnChargeStarted?.Invoke(); // Emituje zdarzenie rozpoczęcia ładowania
    }

    private void OnChargeCanceledInternal(InputAction.CallbackContext context)
    {
        OnChargeCanceled?.Invoke(); // Emituje zdarzenie zakończenia ładowania
    }
}
