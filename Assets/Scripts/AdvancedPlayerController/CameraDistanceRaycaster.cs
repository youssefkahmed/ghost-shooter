using UnityEngine;

namespace Ghost.AdvancedPlayerController
{
    public class CameraDistanceRaycaster : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform cameraTargetTransform;
        [SerializeField] private LayerMask layerMask = Physics.AllLayers;

        [SerializeField] private float minimumDistanceFromObstacles = 0.1f;
        [SerializeField] private float smoothingFactor = 25f;

        private Transform _transform;
        private float _currentDistance;

        private void Awake()
        {
            _transform = transform;

            layerMask &= ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
            _currentDistance = (cameraTargetTransform.position - _transform.position).magnitude;
        }

        private void LateUpdate()
        {
            Vector3 castDirection = cameraTargetTransform.position - _transform.position;

            float distance = GetCameraDistance(castDirection);

            _currentDistance = Mathf.Lerp(_currentDistance, distance, smoothingFactor * Time.deltaTime);
            cameraTransform.position = _transform.position + castDirection * _currentDistance;
        }

        private float GetCameraDistance(Vector3 castDirection)
        {
            // We add a small buffer to make sure the camera doesn't get too close to an obstacle
            float distance = castDirection.magnitude + minimumDistanceFromObstacles;
            var ray = new Ray(_transform.position, castDirection);
            const float sphereRadius = 0.5f;
            
            if (Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, distance, layerMask, QueryTriggerInteraction.Ignore))
            {
                // We don't want negative values
                return Mathf.Max(0f, hit.distance - minimumDistanceFromObstacles);
            }
            
            return castDirection.magnitude;
        }
    }
}
