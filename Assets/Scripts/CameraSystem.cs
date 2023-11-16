using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    [Space]
    [SerializeField] private bool _edgeScrollingEnabled = true;
    [SerializeField, Range(0, 5)] private float _sensivity = 2f;

    private float _moveSpeed = 10f;
    private float _rotationSpeed = 100f;
    private float _edgeScrollSize = 50f;
    private float _maxZoom = 60f;
    private float _minZoom = 20f;
    private float _targetFieldOfView = 60f;

    private void Update()
    {
        HandleCameraMovement();
        HandleCameraRotation();
        HandleCameraZoom();
    }

    private void HandleCameraMovement()
    {
        Vector3 inputDir = GameInput.Instance.GetCameraMovementVectorNormalized();
        if (_edgeScrollingEnabled)
        {
            HandleCameraMovementEdgeScrolling(out inputDir, inputDir);
        }

        Vector3 moveDir = transform.forward * inputDir.y + transform.right * inputDir.x;

        transform.position += moveDir * _moveSpeed * Time.deltaTime * _sensivity;
    }

    private void HandleCameraMovementEdgeScrolling(out Vector3 inputDir, Vector3 currentInputDir)
    {
        inputDir = currentInputDir;

        if (Input.mousePosition.x < _edgeScrollSize && Input.mousePosition.x > 0)
        {
            inputDir.x = -1f;
        }
        if (Input.mousePosition.y < _edgeScrollSize && Input.mousePosition.y > 0)
        {
            inputDir.y = -1f;
        }
        if (Input.mousePosition.x > Screen.width - _edgeScrollSize && Input.mousePosition.x < Screen.width + _edgeScrollSize)
        {
            inputDir.x = 1f;
        }
        if (Input.mousePosition.y > Screen.height - _edgeScrollSize && Input.mousePosition.y < Screen.height + _edgeScrollSize)
        {
            inputDir.y = 1f;
        }
    }

    private void HandleCameraRotation()
    {
        float rotation = GameInput.Instance.GetCameraRotationValue() * _sensivity;
        transform.eulerAngles += new Vector3(0, rotation * _rotationSpeed * Time.deltaTime, 0);
    }

    private void HandleCameraZoom()
    {
        _targetFieldOfView += GameInput.Instance.GetCameraZoomValue() * _sensivity;
        _targetFieldOfView = Mathf.Clamp(_targetFieldOfView, _minZoom, _maxZoom);

        _virtualCamera.m_Lens.FieldOfView = _targetFieldOfView;
    }
}
