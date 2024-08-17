using UnityEngine;

namespace Ghost.AdvancedFirstPersonController
{
    public class MoveCamera : MonoBehaviour
    {
        [SerializeField] private Transform cameraPosition;

        private void Update()
        {
            transform.position = cameraPosition.position;
        }
    }
}
