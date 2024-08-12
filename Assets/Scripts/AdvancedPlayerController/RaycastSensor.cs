using UnityEngine;

namespace Ghost.AdvancedPlayerController
{
    public class RaycastSensor
    {
        public float CastLength = 1f;
        public LayerMask LayerMask = 255;
        
        private Vector3 _origin = Vector3.zero;
        private Transform _transform;

        private CastDirection _castDirection;

        private RaycastHit _hitInfo;

        public RaycastSensor(Transform playerTransform)
        {
            _transform = playerTransform;
        }

        public void Cast()
        {
            Vector3 worldOrigin = _transform.TransformPoint(_origin);
            Vector3 worldDirection = GetCastDirection();

            // `QueryTriggerInteraction.Ignore` is added as we don't want to unnecessarily raycast toward trigger colliders
            Physics.Raycast(worldOrigin, worldDirection, out _hitInfo, CastLength, LayerMask, QueryTriggerInteraction.Ignore);
        }

        public bool HasDetectedHit()
        {
            return _hitInfo.collider != null;
        }
        
        public float GetDistance()
        {
            return _hitInfo.distance;
        }
        
        public Vector3 GetNormal()
        {
            return _hitInfo.normal;
        }
        
        public Vector3 GetPosition()
        {
            return _hitInfo.point;
        }
        
        public Collider GetCollider()
        {
            return _hitInfo.collider;
        }
        
        public Transform GetTransform()
        {
            return _hitInfo.transform;
        }
        
        public void SetCastDirection(CastDirection castDirection)
        {
            _castDirection = castDirection;
        }

        public void SetCastOrigin(Vector3 position)
        {
            _origin = _transform.InverseTransformPoint(position);
        }
        
        private Vector3 GetCastDirection()
        {
            return _castDirection switch
            {
                CastDirection.Forward => _transform.forward,
                CastDirection.Right => _transform.right,
                CastDirection.Up => _transform.up,
                CastDirection.Backward => -_transform.forward,
                CastDirection.Left => -_transform.right,
                CastDirection.Down => -_transform.up,
                _ => Vector3.one
            };
        }
    }

    public enum CastDirection
    {
        Forward,
        Right,
        Up,
        Backward,
        Left,
        Down
    }
}
