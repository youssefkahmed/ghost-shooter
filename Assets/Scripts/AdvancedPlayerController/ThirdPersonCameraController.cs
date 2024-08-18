using Ghost.Utils;
using UnityEngine;

namespace Ghost.AdvancedPlayerController
{
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [SerializeField] private InputReader inputReader;
        
        [SerializeField, Range(0f, 90f)] private float upperVerticalLimit = 35f;
        [SerializeField, Range(0f, 90f)] private float lowerVerticalLimit = 35f;

        [SerializeField] private bool smoothCameraRotation;
        [SerializeField, Range(1f, 50f)] private float cameraSmoothingFactor = 25f;
        [SerializeField] private float cameraSpeed = 50f;

        private Transform _transform;
        private Camera _camera;
        
        private float _currentXAngle;
        private float _currentYAngle;

        private void Awake()
        {
            _transform = transform;
            _camera = GetComponentInChildren<Camera>();

            _currentXAngle = _transform.localRotation.eulerAngles.x;
            _currentYAngle = _transform.localRotation.eulerAngles.y;
        }

        private void Update()
        {
            RotateCamera(inputReader.LookDirection.x, -inputReader.LookDirection.y);
        }

        public Vector3 GetUpDirection()
        {
            return _transform.up;
        }
        
        public Vector3 GetFacingDirection()
        {
            return _transform.forward;
        }
        
        private void RotateCamera(float horizontalInput, float verticalInput)
        {
            if (smoothCameraRotation)
            {
                horizontalInput = Mathf.Lerp(0, horizontalInput, cameraSmoothingFactor * Time.deltaTime);
                verticalInput = Mathf.Lerp(0, verticalInput, cameraSmoothingFactor * Time.deltaTime);
            }

            _currentXAngle += verticalInput * cameraSpeed * Time.deltaTime;
            _currentYAngle += horizontalInput * cameraSpeed * Time.deltaTime;

            _currentXAngle = Mathf.Clamp(_currentXAngle, -upperVerticalLimit, lowerVerticalLimit);

            _transform.localRotation = Quaternion.Euler(_currentXAngle, _currentYAngle, 0);
        }
    }
}
