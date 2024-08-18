using UnityEngine;
using UnityUtils;

namespace Ghost.AdvancedPlayerController
{
    public class TurnTowardController : MonoBehaviour
    {
        [SerializeField] private PlayerAdvancedController playerController;
        [SerializeField] private float turnSpeed = 50f;

        private Transform _transform;
        private float _currentYRotation;
        private const float FALL_OFF_ANGLE = 90f;

        private void Start()
        {
            _transform = transform;

            _currentYRotation = _transform.localEulerAngles.y;
        }

        private void LateUpdate()
        {
            Vector3 velocity = Vector3.ProjectOnPlane(playerController.GetMovementVelocity(), _transform.parent.up);
            if (velocity.magnitude < 0.001f)
            {
                return;
            }

            float angleDifference = VectorMath.GetAngle(_transform.forward, velocity.normalized, _transform.parent.up);
            
            float step = Mathf.Sign(angleDifference) *
                         Mathf.InverseLerp(0f, FALL_OFF_ANGLE, Mathf.Abs(angleDifference)) *
                         turnSpeed * Time.deltaTime;

            _currentYRotation += Mathf.Abs(step) > Mathf.Abs(angleDifference) ? angleDifference : step;
            _transform.localRotation = Quaternion.Euler(0f, _currentYRotation, 0f);
        }
    }
}
