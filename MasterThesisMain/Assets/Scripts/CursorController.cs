using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    [SerializeField] InputReader _inputs;

    public IClickable selectedObject;

    private void OnEnable()
    {
        _inputs.LeftClickUp += SelectedLeftClick;
        _inputs.RightClickUp += SelectedRightClick;
    }

    private void OnDisable()
    {
        _inputs.LeftClickUp -= SelectedLeftClick;   
        _inputs.RightClickUp -= SelectedRightClick;
    }

    void SelectObject()
    {
        var ray = Camera.main.ScreenPointToRay(_inputs.Position);
        RaycastHit hit;

        Physics.Raycast(ray, out hit);
        
        if (hit.collider.gameObject.TryGetComponent<IClickable>(out IClickable clickable))
        {
            selectedObject = clickable;
        }
    }

    void SelectedLeftClick()
    {
        SelectObject();
        if(selectedObject != null) selectedObject.OnLeftClick();
    }

    void SelectedRightClick()
    {
        SelectObject();
        if(selectedObject != null) selectedObject.OnRightClick();
    }
}
