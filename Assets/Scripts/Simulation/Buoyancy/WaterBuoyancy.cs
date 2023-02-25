using UnityEngine;

namespace Simulation
{
    public class WaterBuoyancy : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IBuoyantObject buoyantObject))
            {
                buoyantObject.EnableBuoyancy();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IBuoyantObject buoyantObject))
            {
                buoyantObject.DisableBuoyancy();
            } 
        }
    }
}