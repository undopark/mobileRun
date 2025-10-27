using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Mouse Follow Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Rotation Limits")]
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 20f;
    [SerializeField] private float maxZoom = 90f;
    [SerializeField] private float defaultZoom = 60f;

    private Camera cam;
    private float rotationX = 0f;
    private float rotationY = 0f;
    private float currentZoom;
    private bool isPaused = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        // Hide and lock cursor by default
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize zoom
        currentZoom = defaultZoom;
        if (cam != null)
        {
            cam.fieldOfView = currentZoom;
        }

        // Initialize rotation based on current camera rotation
        Vector3 currentRotation = transform.eulerAngles;
        rotationY = currentRotation.y;
        rotationX = currentRotation.x;
        if (rotationX > 180f) rotationX -= 360f;
    }

    void Update()
    {
        HandleAltKeyToggle();

        if (!isPaused)
        {
            HandleMouseMovement();
        }

        HandleZoom();
    }

    void HandleAltKeyToggle()
    {
        // Toggle pause when Alt key is pressed
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                // Show and unlock cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // Hide and lock cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void HandleMouseMovement()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Calculate rotation
        rotationY += mouseX;
        rotationX -= mouseY;

        // Clamp vertical rotation
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        // Apply rotation smoothly
        Quaternion targetRotation = Quaternion.Euler(rotationX, rotationY, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
    }

    void HandleZoom()
    {
        // Get scroll wheel input
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0f)
        {
            // Adjust zoom (negative because scrolling up should zoom in)
            currentZoom -= scrollInput * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            // Apply zoom smoothly
            if (cam != null)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, currentZoom, smoothSpeed * Time.deltaTime);
            }
        }
    }
}
