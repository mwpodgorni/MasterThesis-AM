using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraDragController : MonoBehaviour
{
    [Header("Camera Properties")]
    public ProjectionMode projectionMode = ProjectionMode.Perspective;

    [SerializeField] float _lookSpeed = 2f;
    [SerializeField] float _zoomSpeed = 2f;

    [Header("Zoom Properties")]
    [Tooltip("Only relevant for Orthographic Projection Mode")]
    [SerializeField] float _maxViewSize = 20f;
    [Tooltip("Only relevant for Orthographic Projection Mode")]
    [SerializeField] float _minViewSize = 5f;

    [Tooltip("Only relevant for Perspective Projection Mode")]
    [SerializeField] float _maxDistance = 20f;
    [Tooltip("Only relevant for Perspective Projection Mode")]
    [SerializeField] float _minDistance = 5f;

    [Header("Camera Angles")]
    [SerializeField] float _maxYaw = 90f;
    [SerializeField] float _minYaw = -90f;

    [Header("Camera Holder")]
    [SerializeField] Transform _cameraHolder;

    [Header("Input Reader")]
    [SerializeField] InputReader _input;

    Vector2 _targetRotation = new Vector2(30f, 45f);
    Vector3 _targetPosition = new Vector3(0, 0, 0);

    Vector3 _dragOrigin;
    Vector3 _dragDifference;

    bool _dragMovement;
    bool _dragRotation;

    float _currentZoomValue = 0;
    void OnEnable()
    {
        _input.DragStart += EnableDragMovement;
        _input.DragEnd += DisableDragMovement;
        _input.RotateStart += EnableDragRotation;
        _input.RotateEnd += DisableDragRotation;
    }

    void OnDisable()
    {
        _input.DragStart -= EnableDragMovement;
        _input.DragEnd -= DisableDragMovement;
        _input.RotateStart -= EnableDragRotation;
        _input.RotateEnd -= DisableDragRotation;
    }

    void Start()
    {
        transform.position = _targetPosition;
        transform.rotation = Quaternion.Euler(_targetRotation.x, _targetRotation.y, 0);

        switch (projectionMode)
        {
            case ProjectionMode.Perspective:
                Camera.main.orthographic = false;
                _currentZoomValue = _maxDistance;
                _cameraHolder.localPosition = new Vector3(0, 0, -_currentZoomValue);
                break;
            case ProjectionMode.Orthographic:
                Camera.main.orthographic = true;
                _currentZoomValue = _maxViewSize;
                _cameraHolder.localPosition = new Vector3(0, 0, -100);
                break;
        }
    }

    void LateUpdate()
    {
        Zoom();
        if (_dragMovement) DragMovement();
        if (_dragRotation) DragRotation();
    }

    void Zoom()
    {
        _currentZoomValue -= _input.Scroll * _zoomSpeed;

        switch (projectionMode)
        {
            case ProjectionMode.Perspective:
                _currentZoomValue = Mathf.Clamp(_currentZoomValue, _minDistance, _maxDistance);
                _cameraHolder.localPosition = new Vector3(0, 0, -_currentZoomValue);
                break;
            case ProjectionMode.Orthographic:
                _currentZoomValue = Mathf.Clamp(_currentZoomValue, _minViewSize, _maxViewSize);
                Camera.main.orthographicSize = _currentZoomValue;
                break;
        }

    }

    void DragMovement()
    {
        _dragDifference = GetMousePosition() - transform.position;
        transform.position = _dragOrigin - _dragDifference;
    }

    void DragRotation()
    {
        float mouseX = _input.PositionDelta.x * _lookSpeed;
        float mouseY = _input.PositionDelta.y * _lookSpeed;
        _targetRotation.y += mouseX;
        _targetRotation.x = Mathf.Clamp(_targetRotation.x - mouseY, _minYaw, _maxYaw);
        transform.rotation = Quaternion.Euler(_targetRotation.x, _targetRotation.y, 0);
    }

    void EnableDragMovement()
    {
        if (_dragRotation) return;
        _dragOrigin = GetMousePosition();
        _dragMovement = true;
    }

    void DisableDragMovement()
    {
        _dragMovement = false;
    }

    void EnableDragRotation()
    {
        if (_dragMovement) return;
        _dragRotation = true;
    }

    void DisableDragRotation()
    {
        _dragRotation = false;
    }

    Vector3 GetMousePosition()
    {
        switch (projectionMode)
        {
            case ProjectionMode.Perspective:
                Vector3 pos = _input.Position;
                pos.z = _currentZoomValue;
                return Camera.main.ScreenToWorldPoint(pos);
            case ProjectionMode.Orthographic:
                return Camera.main.ScreenToWorldPoint(_input.Position);
        }

        return new Vector3();
    }

    public enum ProjectionMode
    {
        Perspective,
        Orthographic
    }
}
