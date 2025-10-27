using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float smoothing = 5f;
    
    [Header("Camera Limits")]
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // Private variables
    private float currentX = 0f;
    private float currentY = 0f;
    private float targetX = 0f;
    private float targetY = 0f;
    private bool isCameraMode = true;
    private bool lastCameraMode = true;
    
    // Input System
    private Vector2 mouseDelta;
    private float scrollInput;

    void Start()
    {
        // 타겟이 설정되지 않은 경우 자동으로 찾기
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                if (showDebugInfo)
                {
                    Debug.Log("ThirdPersonCamera: Player 태그를 가진 객체를 자동으로 찾았습니다.");
                }
            }
            else
            {
                Debug.LogError("ThirdPersonCamera: 타겟이 설정되지 않았고 'Player' 태그를 가진 객체도 찾을 수 없습니다.");
                return;
            }
        }

        // 초기 카메라 위치 설정
        Vector3 angles = transform.eulerAngles;
        currentX = targetX = angles.y;
        currentY = targetY = angles.x;
        
        // 마우스 커서 숨기기
        SetCursorState(true);
    }

    void Update()
    {
        if (target == null) return;

        HandleInput();
        UpdateCameraMode();
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        // 마우스 입력 받기
        if (Mouse.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue();
            scrollInput = Mouse.current.scroll.ReadValue().y;
        }

        // Alt 키를 누르고 있는 동안만 카메라 모드 비활성화
        bool altPressed = Keyboard.current != null && Keyboard.current.leftAltKey.isPressed;
        isCameraMode = !altPressed;
        
        // 카메라 모드가 변경되었을 때만 커서 상태 업데이트
        if (isCameraMode != lastCameraMode)
        {
            SetCursorState(isCameraMode);
            lastCameraMode = isCameraMode;
        }
    }

    void UpdateCameraMode()
    {
        if (isCameraMode)
        {
            // 카메라 회전 업데이트
            targetX += mouseDelta.x * mouseSensitivity;
            targetY -= mouseDelta.y * mouseSensitivity;
            
            // 수직 각도 제한
            targetY = Mathf.Clamp(targetY, minVerticalAngle, maxVerticalAngle);
        }
        
        // 줌 처리 (카메라 모드와 관계없이 항상 작동)
        if (Mathf.Abs(scrollInput) > 0.1f)
        {
            distance -= scrollInput * zoomSpeed * 0.1f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            
            if (showDebugInfo)
            {
                Debug.Log($"줌 거리: {distance:F2}");
            }
        }
    }

    void UpdateCameraPosition()
    {
        // 부드러운 회전 적용
        currentX = Mathf.LerpAngle(currentX, targetX, smoothing * Time.deltaTime);
        currentY = Mathf.LerpAngle(currentY, targetY, smoothing * Time.deltaTime);

        // 카메라 회전 계산
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        
        // 카메라 위치 계산
        Vector3 targetPosition = target.position + Vector3.up * height;
        Vector3 desiredPosition = targetPosition - rotation * Vector3.forward * distance;
        
        // 거리를 정확히 유지하면서 위치 설정
        transform.position = desiredPosition;
        
        // 타겟을 바라보기
        transform.LookAt(targetPosition);
    }

    void SetCursorState(bool hideCursor)
    {
        if (hideCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"마우스 커서: {(hideCursor ? "숨김" : "보임")}");
        }
    }

    // Public methods for external control
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetCameraMode(bool cameraMode)
    {
        isCameraMode = cameraMode;
        SetCursorState(isCameraMode);
    }

    public void SetDistance(float newDistance)
    {
        distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }

    // Inspector에서 호출 가능한 메서드들
    [ContextMenu("Reset Camera Position")]
    public void ResetCameraPosition()
    {
        currentX = targetX = 0f;
        currentY = targetY = 0f;
        distance = 5f;
        
        if (showDebugInfo)
        {
            Debug.Log("카메라 위치가 리셋되었습니다.");
        }
    }

    [ContextMenu("Toggle Camera Mode")]
    public void ToggleCameraMode()
    {
        isCameraMode = !isCameraMode;
        SetCursorState(isCameraMode);
        
        if (showDebugInfo)
        {
            Debug.Log($"카메라 모드: {(isCameraMode ? "활성" : "비활성")}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // 타겟 위치 표시
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position + Vector3.up * height, 0.5f);
            
            // 카메라 거리 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(target.position + Vector3.up * height, transform.position);
        }
    }
}
