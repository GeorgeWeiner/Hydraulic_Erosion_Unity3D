using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class LooseCondition : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("NPC"))
            {
                SceneManager.LoadScene("OutdoorsScene");
            }
        }
    }
}