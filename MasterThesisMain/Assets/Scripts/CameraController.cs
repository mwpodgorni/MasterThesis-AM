using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f; // Speed of camera movement
    public float lookSpeed = 2f;  // Sensitivity of mouse look
    public float rotationSmoothness = 5f;

    private Vector3 rotation = Vector3.zero;
    private bool _move = false;

    void Start()
    {
        rotation = transform.eulerAngles;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            _move = !_move;
            Cursor.lockState = _move ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !_move;
        }
        if (!_move) return;
        // Camera movement with WSAD keys
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // Camera rotation with mouse
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        rotation.y += mouseX;
        rotation.x -= mouseY;
        rotation.x = Mathf.Clamp(rotation.x, -90f, 90f);

        Quaternion targetRotation = Quaternion.Euler(rotation.x, rotation.y, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
