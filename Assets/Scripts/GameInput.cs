using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractPerformed;
    public event EventHandler OnInteractCanceled;
    public event EventHandler OnOrderPerformed;

    public bool HoldingInteract = false;

    private PlayerInputActions _playerInputActions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Player.Enable();

        _playerInputActions.Player.Interact.performed += Interact_performed;
        _playerInputActions.Player.Interact.canceled += Interact_canceled;
        _playerInputActions.Player.Order.performed += Order_performed;
    }

    private void OnDestroy()
    {
        _playerInputActions.Player.Interact.performed -= Interact_performed;
        _playerInputActions.Player.Interact.canceled -= Interact_canceled;
        _playerInputActions.Player.Order.performed -= Order_performed;
    }

    // --------- Public Methods --------- //

    public Vector2 GetCameraMovementVectorNormalized()
    {
        return _playerInputActions.Player.CameraMovement.ReadValue<Vector2>().normalized;
    }

    public float GetCameraRotationValue()
    {
        return _playerInputActions.Player.CameraRotation.ReadValue<float>();
    }

    public float GetCameraZoomValue()
    {
        return _playerInputActions.Player.CameraZoom.ReadValue<float>() / 120;
    }

    // --------- Private Methods --------- //

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractPerformed?.Invoke(this, EventArgs.Empty);
        HoldingInteract = true;
    }

    private void Interact_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractCanceled?.Invoke(this, EventArgs.Empty);
        HoldingInteract = false;
    }

    private void Order_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnOrderPerformed?.Invoke(this, EventArgs.Empty);
    }
}
