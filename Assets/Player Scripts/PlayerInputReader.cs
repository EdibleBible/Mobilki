using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : NetworkBehaviour
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
        {
            playerActions = new PlayerInputActions();
        }

        playerActions.Player.Move.performed += OnMovePerformed;
        playerActions.Player.Move.canceled += OnMoveCanceled;
        playerActions.Player.Jump.performed += OnJumpPerformed;
        playerActions.Player.Jump.canceled += OnJumpCanceled;
        playerActions.Player.Interact.performed += OnInteractPerform;
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
        if (!IsLocalPlayer()) return;

        Vector2 input = context.ReadValue<Vector2>();
        MoveInput = new Vector3(input.x, 0, input.y);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer()) return;

        MoveInput = Vector3.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer()) return;

        IsJumping = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer()) return;

        IsJumping = false;
    }

    private void OnInteractPerform(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer()) return;

        OnInteractPerformed?.Invoke();
    }

    private void OnChargeStartedInternal(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer()) return;

        OnChargeStarted?.Invoke();
    }

    private void OnChargeCanceledInternal(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer()) return;

        OnChargeCanceled?.Invoke();
    }

    private bool IsLocalPlayer()
    {
        if (NetworkController.runnerInstance != null && NetworkController.runnerInstance.LocalPlayer == Object.InputAuthority)
        {
            return true;
        }

        return false;
    }
}
