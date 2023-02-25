using UnityEngine;

namespace Simulation
{
    public class BuoyantObject : MonoBehaviour, IBuoyantObject
    {
        [Tooltip("How much drag should be added to the rigidbody.")]
        [SerializeField] private float dragAdd;
        [Tooltip("How much angular drag should be added to the rigidbody.")]
        [SerializeField] private float angularDragAdd;
        
        
        [Tooltip("Does the object float on the surface of the water.")]
        [SerializeField] private bool floatingObject;

        private Rigidbody _rb;

        protected virtual void Awake()
        {
            _rb = GetComponentInParent<Rigidbody>();
        }

        public virtual void EnableBuoyancy()
        {
            _rb.drag += dragAdd;
            _rb.angularDrag += angularDragAdd;
            if (floatingObject)
            {
                _rb.useGravity = false;
            }
        }

        public virtual void DisableBuoyancy()
        {
            _rb.drag -= dragAdd;
            _rb.angularDrag -= angularDragAdd;
            if (floatingObject)
            {
                _rb.useGravity = true;
            }
        }
    }
}