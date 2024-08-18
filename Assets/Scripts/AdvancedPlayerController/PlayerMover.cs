using UnityEngine;

namespace Ghost.AdvancedPlayerController
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMover : MonoBehaviour
    {
        [Header("Collider Settings:")]
        [Range(0f, 1f)] [SerializeField] private float stepHeightRatio = 0.1f;
        [SerializeField] private float colliderHeight = 2f;
        [SerializeField] private float colliderThickness = 1f;
        [SerializeField] private Vector3 colliderOffset = Vector3.zero;

        [Header("Sensor Settings:")]
        [SerializeField] private bool isInDebugMode;
        
        private Rigidbody _rb;
        private Transform _transform;
        private CapsuleCollider _collider;
        private RaycastSensor _sensor;
        
        private bool _isGrounded;
        private float _baseSensorRange;
        private Vector3 _currentGroundAdjustmentVelocity; // Velocity to adjust player position to maintain ground contact
        private int _currentLayer;

        private bool _isUsingExtendedSensorRange = true; // Use extended range for smoother ground transitions

        private void Awake()
        {
            Setup();
            RecalculateColliderDimensions();
        }

        private void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                RecalculateColliderDimensions();
            }
        }
        
        public void CheckForGround()
        {
            if (_currentLayer != gameObject.layer)
            {
                RecalculateSensorLayerMask();
            }
            
            _currentGroundAdjustmentVelocity = Vector3.zero;
            
            _sensor.CastLength = _isUsingExtendedSensorRange 
                ? _baseSensorRange + colliderHeight * _transform.localScale.x * stepHeightRatio
                : _baseSensorRange;
            _sensor.Cast();
            
            _isGrounded = _sensor.HasDetectedHit();
            if (!_isGrounded)
            {
                return;
            }
            
            float distance = _sensor.GetDistance();
            float upperLimit = colliderHeight * _transform.localScale.x * (1f - stepHeightRatio) * 0.5f;
            float middle = upperLimit + colliderHeight * _transform.localScale.x * stepHeightRatio;
            float distanceToGo = middle - distance;
            
            // Velocity needed to move player to correct position over the course of a single frame
            _currentGroundAdjustmentVelocity = _transform.up * (distanceToGo / Time.fixedDeltaTime);
        }
        
        public bool IsGrounded()
        {
            return _isGrounded;
        }
        
        public Vector3 GetGroundNormal()
        {
            return _sensor.GetNormal();
        }
        
        // NOTE: Older versions of Unity use rb.velocity instead
        public void SetVelocity(Vector3 velocity)
        {
            #if UNITY_6000_0_OR_NEWER
                _rb.linearVelocity = velocity + _currentGroundAdjustmentVelocity;
            #else
                _rb.velocity = velocity + _currentGroundAdjustmentVelocity;
            #endif
        }
        
        public void SetExtendSensorRange(bool isExtended)
        {
            _isUsingExtendedSensorRange = isExtended;
        }
        
        private void Setup()
        {
            _transform = transform;
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            
            _rb.freezeRotation = true;
            _rb.useGravity = false;
        }

        private void RecalculateColliderDimensions()
        {
            if (_collider == null)
            {
                Setup();
            }
            
            _collider.height = colliderHeight * (1f - stepHeightRatio);
            _collider.radius = colliderThickness / 2f;
            _collider.center = colliderOffset * colliderHeight + new Vector3(0f, stepHeightRatio * _collider.height / 2f, 0f);

            // Making sure collider radius doesn't exceed half of its height; to maintain valid capsule shape
            if (_collider.height / 2f < _collider.radius)
            {
                _collider.radius = _collider.height / 2f;
            }
            
            // Collider shape changed; we need to recalibrate the RaycastSensor
            RecalibrateSensor();
        }

        private void RecalibrateSensor()
        {
            _sensor ??= new RaycastSensor(_transform);
            
            _sensor.SetCastOrigin(_collider.bounds.center);
            _sensor.SetCastDirection(CastDirection.Down);
            RecalculateSensorLayerMask();
            
            // Small factor added to prevent clipping issues when the _sensor range is calculated
            const float safetyDistanceFactor = 0.001f;
            
            float length = colliderHeight * (1f - stepHeightRatio) * 0.5f + colliderHeight * stepHeightRatio;
            _baseSensorRange = length * (1f + safetyDistanceFactor) * _transform.localScale.x;
            _sensor.CastLength = length * _transform.localScale.x;
        }

        private void RecalculateSensorLayerMask()
        {
            int objectLayer = gameObject.layer;
            int layerMask = Physics.AllLayers; // Constant, equals -1

            // 32 for 32 bits of the LayerMask
            for (var i = 0; i < 32; i++)
            {
                if (Physics.GetIgnoreLayerCollision(objectLayer, i))
                {
                    // Turning off ignored layers from the LayerMask
                    layerMask &= ~(1 << i);
                }
            }
            
            int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            layerMask &= ~(1 << ignoreRaycastLayer);
            
            _sensor.LayerMask = layerMask;
            _currentLayer = objectLayer;
        }
    }
}
