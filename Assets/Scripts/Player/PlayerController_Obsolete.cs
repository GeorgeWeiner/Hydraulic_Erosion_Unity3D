using UnityEngine;
using UnityEngine.AI;

namespace Player
{
    public class PlayerController_Obsolete : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Camera _cam;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _cam = Camera.main;
        }
    
        private void Update()
        {
            BasicMovement();
        }

        private void BasicMovement()
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit);
        
            Debug.DrawRay(ray.origin, ray.direction, Color.green);

            if (!Input.GetMouseButton(0)) return;
            _agent.SetDestination(hit.point);
        }
    }
}
