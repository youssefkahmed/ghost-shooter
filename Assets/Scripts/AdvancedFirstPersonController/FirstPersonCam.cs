using UnityEngine;

namespace Ghost.AdvancedFirstPersonController
{
    public class FirstPersonCam : MonoBehaviour
    {
        [SerializeField] private Transform orientation;
        
        [SerializeField] private float xSensitivity;
        [SerializeField] private float ySensitivity;

        private float _xRotation;
        private float _yRotation;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            float mouseX = Input.GetAxisRaw("Mouse X");
            float mouseY = Input.GetAxisRaw("Mouse Y");

            _yRotation += mouseX;
            _xRotation = Mathf.Clamp(_xRotation - mouseY, -90, 90);

            transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, _yRotation, 0);
        }
    }
}
