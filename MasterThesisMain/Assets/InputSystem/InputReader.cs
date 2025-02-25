using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "InputSystem/Input Reader")]
public class InputReader : ScriptableObject, Inputs.IGameplayActions, Inputs.IUIActions
{
    Inputs _input;
    public InputAccess Access { get; private set; }

    public Vector2 Movement => _input.Gameplay.Movement.ReadValue<Vector2>();
    public Vector2 Position => _input.Gameplay.Position.ReadValue<Vector2>();
    public Vector2 PositionDelta => _input.Gameplay.PositionDelta.ReadValue<Vector2>();
    public float Scroll => _input.Gameplay.Scroll.ReadValue<float>();
    public bool LeftClicking => _input.Gameplay.LeftClick.ReadValue<float>() > 0;
    public bool RightClicking => _input.Gameplay.RightClick.ReadValue<float>() > 0;

    public event UnityAction LeftClickDown = delegate { };
    public event UnityAction LeftClickUp = delegate { };

    public event UnityAction RightClickDown = delegate { };
    public event UnityAction RightClickUp = delegate { };

    public event UnityAction Escape = delegate { };

    void OnEnable()
    {
        _input ??= new Inputs();
        _input.Gameplay.SetCallbacks(this);
        _input.UI.SetCallbacks(this);
        _input.Enable();

        Access = new(this);
    }

    #region Input Actions
    public void EnableGameplayActions() => _input.Gameplay.Enable();
    public void EnableUIActions() => _input.UI.Enable();
    public void DisableGameplayActions() => _input.Gameplay.Disable();
    public void DisableUIActions() => _input.UI.Disable();

    public void OnMovement(InputAction.CallbackContext context)
    {
    }

    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            LeftClickDown.Invoke();
        }
        else if (context.canceled)
        {
            LeftClickUp.Invoke();
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RightClickDown.Invoke();
        }
        else if (context.canceled)
        {
            RightClickUp.Invoke();
        }
    }

    public void OnPosition(InputAction.CallbackContext context)
    {
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Escape.Invoke();
        }
    }

    public void OnUnpause(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Escape.Invoke();
        }
    }

    public void OnScroll(InputAction.CallbackContext context)
    {

    }

    public void OnPositionDelta(InputAction.CallbackContext context)
    {
        
    }
    #endregion
}

public enum InputMode { UI, Game }

public class InputAccess
{
    readonly InputReader _input;

    public InputAccess(InputReader input) => _input = input;

    public void Mode(InputMode mode)
    {
        if (mode == InputMode.UI)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        if (mode == InputMode.Game)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
