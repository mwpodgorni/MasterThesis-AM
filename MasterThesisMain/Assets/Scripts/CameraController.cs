using UnityEngine;

public class AxonometricCameraController : MonoBehaviour
{
    public float lookSpeed = 2f;
    public float zoomSpeed = 2f;
    public float edgeScrollSpeed = 10f;
    public float edgeThreshold = 100f; // in pixels
    public float minDistance = 5f;
    public float maxDistance = 20f;

    private Vector3 pivot;
    private float distance = 10f;
    private Vector2 rotation = new Vector2(30f, 45f);

    void Start()
    {
        // Set pivot based on initial position and direction.
        pivot = transform.position + transform.forward * distance;
    }

    void Update()
    {
        // Zoom with mouse scroll.
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        distance -= scrollInput * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Rotate camera when right mouse button is held.
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;
            rotation.y += mouseX;
            rotation.x = Mathf.Clamp(rotation.x - mouseY, -90f, 90f);
        }
        Quaternion camRotation = Quaternion.Euler(rotation.x, rotation.y, 0);

        Vector3 moveDir = Vector3.zero;
        Vector3 right = camRotation * Vector3.right;
        Vector3 forward = camRotation * Vector3.forward;
        right.y = 0; forward.y = 0;
        right.Normalize(); forward.Normalize();

        if (Input.mousePosition.x <= edgeThreshold)
            moveDir -= right;
        if (Input.mousePosition.x >= Screen.width - edgeThreshold)
            moveDir += right;
        if (Input.mousePosition.y <= edgeThreshold)
            moveDir -= forward;
        if (Input.mousePosition.y >= Screen.height - edgeThreshold)
            moveDir += forward;

        pivot += moveDir * edgeScrollSpeed * Time.deltaTime;

        // Position the camera relative to the pivot.
        Vector3 offset = camRotation * new Vector3(0, 0, -distance);
        transform.position = pivot + offset;
        transform.LookAt(pivot);
    }
}
