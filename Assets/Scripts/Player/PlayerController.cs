using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 150f;
        [SerializeField] private float sprintSpeed = 230f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float distanceToGround = .5f;
        [SerializeField] private float maxDistanceGroundInfo = 5f;
        [SerializeField] private float gravityAcceleration = 50f;
        
        [SerializeField] private LayerMask groundLayer;

        [Header("Crouching")] 
        [SerializeField] private Vector3 crouchScale = new(1.5f, 1f, 1.5f);
        [SerializeField] private float crouchMovementSpeed = 60f;
        [SerializeField] private float crouchStateSpeed = 6f;
        [SerializeField] private float maxHeadToCeilingDistance = 1f;


        private Vector3 _normalScale;
        private float _crouchScaleDifference;
        private Rigidbody _rb;
        private CapsuleCollider _col;
        private PlayerLook _playerLook;
        private float gravityStrength;

        public float MaxSpeed => sprintSpeed;
        public float MinSpeed => walkSpeed;
        public float GravityAcceleration
        {
            get => gravityAcceleration;
            set => gravityAcceleration = value;
        }

        public float GravityStrength
        {
            get => gravityStrength;
            set => gravityStrength = value;
        }

        public bool IsSwimming;


        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponentInChildren<CapsuleCollider>();
            _playerLook = GetComponentInChildren<PlayerLook>();
            _normalScale = _col.transform.localScale;
        }
        private void Update()
        {
            Jump();
            Crouch();
        }

        private void FixedUpdate()
        {
            if (!IsSwimming)
                MovePlayer();
            else
            {
                MovePlayerSwimming();
            }
            GravitationalAcceleration();
            Gravity();
        }

        private void MovePlayer()
        {
            Vector3 playerLookDirection = _playerLook.transform.forward * PlayerInput.InputY() + _playerLook.transform.right * PlayerInput.InputX();
            var directionHorizontal = new Vector3(playerLookDirection.x, 0f, playerLookDirection.z);
            Vector3 projectOnPlane = Vector3.ProjectOnPlane(directionHorizontal, GroundInfo().normal);

            _rb.AddForce(projectOnPlane.normalized * (MovementSpeed() * Time.fixedDeltaTime * 100f), ForceMode.Acceleration);
            _rb.AddForce(-projectOnPlane.normalized * (MovementSpeed() * 0.75f * Time.fixedDeltaTime * 100f), ForceMode.Acceleration);
        }

        private void MovePlayerSwimming()
        {
            Vector3 playerLookDirection = _playerLook.transform.forward * PlayerInput.InputY() + _playerLook.transform.right * PlayerInput.InputX();

            _rb.AddForce(playerLookDirection.normalized * (MovementSpeed() * Time.fixedDeltaTime * 100f), ForceMode.Acceleration);
            _rb.AddForce(-playerLookDirection.normalized * (MovementSpeed() * 0.75f * Time.fixedDeltaTime * 100f), ForceMode.Acceleration);
        }

        private void Jump()
        {
            if (IsGrounded() && PlayerInput.Jump() && CanStandUp())
            {
                Vector3 velocity = _rb.velocity;
                velocity = new Vector3(velocity.x, 0f, velocity.z);
                
                _rb.velocity = velocity;
                _rb.AddForce(0f, jumpForce, 0f, ForceMode.Impulse);
            }
        }

        private void Gravity()
        {
            if (!IsGrounded() && !IsSwimming)
            {
                _rb.AddForce(0f, -gravityStrength, 0f, ForceMode.Acceleration);
            }
        }

        public float MovementSpeed()
        {
            if (PlayerInput.Crouch() || !CanStandUp()) return crouchMovementSpeed;
            return PlayerInput.Sprint() ? sprintSpeed : walkSpeed;
        }

        public bool IsGrounded()
        {
            Bounds bounds = _col.bounds;
            var capsuleBottom = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

            return Physics.CheckCapsule(bounds.center, capsuleBottom, distanceToGround, groundLayer,
                QueryTriggerInteraction.Ignore);
        }

        private RaycastHit GroundInfo()
        {
            Transform myTransform = transform;
            Physics.Raycast(myTransform.position, -myTransform.up, out RaycastHit hit, maxDistanceGroundInfo,
                groundLayer, QueryTriggerInteraction.Ignore);
            
            return hit;
        }

        private void GravitationalAcceleration()
        {
            if (!IsGrounded() && !IsSwimming)
            {
                gravityStrength += gravityAcceleration * Time.fixedDeltaTime;
            }
            else
            {
                gravityStrength = 0f;
            }
        }

        private void Crouch()
        {
            Vector3 localScale;

            if (!PlayerInput.Crouch() && CanStandUp())
            {
                localScale = _normalScale;
                distanceToGround = .5f;
            }
            else
            {
                localScale = crouchScale;
                distanceToGround = .25f;
            }
            
            _col.transform.localScale = Vector3.MoveTowards(_col.transform.localScale, localScale, crouchStateSpeed * Time.deltaTime);
        }

        private bool CanStandUp()
        {
            Transform myTransform = transform;
            return !Physics.Raycast(myTransform.position, myTransform.up, maxHeadToCeilingDistance);
        }
    }
}
