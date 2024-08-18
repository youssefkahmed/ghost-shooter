using UnityEngine;

namespace Ghost.AdvancedPlayerController 
{
    public class CeilingDetector : MonoBehaviour
    {
        [SerializeField] private float ceilingAngleLimit = 10f;
        [SerializeField] private bool isInDebugMode;

        private bool _ceilingWasHit;
        private const float DEBUG_DRAW_DURATION = 2.0f;

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        private void OnCollisionEnter(Collision collision)
        {
            CheckFirstContact(collision);
        }
        
        private void OnCollisionStay(Collision collision)
        { 
            CheckFirstContact(collision);
        }

        public bool HitCeiling()
        {
            return _ceilingWasHit;
        }
        
        public void Reset()
        {
            _ceilingWasHit = false;
        }
        
        private void CheckFirstContact(Collision collision)
        {
            if (collision.contacts.Length == 0)
            {
                return;
            }

            float angle = Vector3.Angle(-_transform.up, collision.contacts[0].normal);
            if (angle < ceilingAngleLimit)
            {
                _ceilingWasHit = true;
            }

            if (isInDebugMode)
            {
                Debug.DrawRay(collision.contacts[0].point, collision.contacts[0].normal, Color.red, DEBUG_DRAW_DURATION);
            }
        }
    }
}