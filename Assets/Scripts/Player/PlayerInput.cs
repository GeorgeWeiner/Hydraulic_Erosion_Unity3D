using UnityEngine;

namespace Player
{
    public class PlayerInput : MonoBehaviour
    {
        public static PlayerInput Instance { get; private set; }
        private static KeybindManager _keybindManager;

#if ENABLE_LEGACY_INPUT_MANAGER
        public static float InputX() => Input.GetAxisRaw("Horizontal");
        public static float InputY() => Input.GetAxisRaw("Vertical");
        public static float MouseInputX() => Input.GetAxisRaw("Mouse Y");
        public static float MouseInputY() => Input.GetAxisRaw("Mouse X");
        public static bool Jump() => Input.GetKeyDown(_keybindManager.jump);
        public static bool Shoot() => Input.GetMouseButton(0);
        public static bool Reload() => Input.GetKeyDown(_keybindManager.reload);
        public static bool Interact() => Input.GetKeyDown(_keybindManager.interact);
        public static bool Sprint() => Input.GetKey(_keybindManager.sprint);
        public static bool Crouch() => Input.GetKey(_keybindManager.crouch);
        public static bool Flashlight() => Input.GetKeyDown(_keybindManager.flashlight);
#endif        
        
        public static Vector3 playerForward;

        private void Awake()
        {
            _keybindManager = FindObjectOfType<KeybindManager>();
            
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            } 
            else 
            {
                Instance = this;
            }
        }
    }
}