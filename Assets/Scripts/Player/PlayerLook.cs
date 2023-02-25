using UnityEngine;

namespace Player
{
    public class PlayerLook : MonoBehaviour
    {
        public float sensX, sensY;
        public Transform cam;

        [SerializeField] private Transform camAnchor;
        
        private float _yRotation, _xRotation;
        private const float Multiplier = 100f;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            cam.rotation = camAnchor.rotation;
        }

        private void Update()
        {
            LookAround();
            UpdatePosition();
        }


        private void UpdatePosition()
        {
            transform.position = camAnchor.transform.position;
        }

        private void LookAround()
        {
            _xRotation -= PlayerInput.MouseInputX() * sensY * Multiplier * Time.deltaTime;
            _yRotation += PlayerInput.MouseInputY() * sensX * Multiplier * Time.deltaTime;

            _xRotation = Mathf.Clamp(_xRotation, -85f, 85f);
            cam.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);

            PlayerInput.playerForward = cam.eulerAngles;
        }
    }
}