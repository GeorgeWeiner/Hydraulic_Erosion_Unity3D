using UnityEngine;

namespace Player
{
    public class ViewBob : MonoBehaviour
    {
        [SerializeField] private bool enable = true;

        
        [Header("Intensity")]
        [SerializeField] private float minAmplitude = 0.15f;
        [SerializeField] private float minFrequency = 15f;
        
        [SerializeField] private float maxAmplitude = 0.3f;
        [SerializeField] private float maxFrequency = 25f;

        [SerializeField] private float resetSpeed = 1f;
        [SerializeField] private float smoothingStrength;

        [SerializeField] private AnimationCurve intensityCurve;

        [Header("Cameras")]
        [SerializeField] private Transform cam;
        [SerializeField] private Transform cameraHolder;
        
        private float _amplitude;
        private float _frequency;
        private float _smoothedIntensity;
        private float _speed;
        private float _maxSpeed;
        private const float ToggleSpeed = 3f;
        
        private Vector3 _startPos;
        private Vector3 _smoothedPos;
        
        private PlayerController _controller;
        
        private Rigidbody _rb;
        

        private void Awake()
        {
            _controller = GetComponentInParent<PlayerController>();
            _rb = GetComponentInParent<Rigidbody>();
            _startPos = cam.localPosition;
            
        }

        private void Update()
        {
            if (!enable) return;
            
            CheckMotion();
            //ResetPosition();
            ControlIntensity();
            cam.LookAt(FocusTarget());
        }

        private void CheckMotion()
        {
            Vector3 velocity = _rb.velocity;
            _speed = new Vector3(velocity.x, 0, velocity.z).magnitude;

            //if (_speed < ToggleSpeed) return;
            if (!_controller.IsGrounded()) return;
            
            PlayMotion(FootStepMotion());
        }
        
        private void PlayMotion(Vector3 motion)
        {
            cam.localPosition += motion; 
        }

        private void ResetPosition()
        {
            if (cam.localPosition == _startPos) return;

            cam.localPosition = Vector3.MoveTowards(cam.localPosition, _startPos, resetSpeed);
        }

        private Vector3 FootStepMotion()
        {
            Vector3 pos = Vector3.zero;
            pos.y += Mathf.Sin(Time.time * _frequency) * _amplitude;
            pos.x += Mathf.Cos(Time.time * _frequency / 2) * _amplitude * 2;
            return pos;
        }

        private Vector3 FocusTarget()
        {
            Vector3 position = transform.position;
            var pos = new Vector3(position.x, position.y + cameraHolder.localPosition.y, position.z);
            pos += cameraHolder.forward * 50f;
            return pos;
        }

        private void ControlIntensity()
        {
            float intensity = Mathf.InverseLerp(_controller.MinSpeed, _controller.MaxSpeed, _controller.MovementSpeed());
            _smoothedIntensity = Mathf.MoveTowards(_smoothedIntensity, intensity, 1 / smoothingStrength);

            _frequency = Mathf.Lerp(0f, maxFrequency, intensityCurve.Evaluate(_smoothedIntensity));
            _amplitude = Mathf.Lerp(0f, maxAmplitude, intensityCurve.Evaluate(_smoothedIntensity));
        }
    }
}
