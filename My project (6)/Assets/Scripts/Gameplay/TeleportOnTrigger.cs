using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Gameplay
{
    public class TeleportOnTrigger : MonoBehaviour
    {
        [SerializeField] private string sceneToLoad = "SampleScene";
        [SerializeField] private string playerRootName = "Player";

        private void OnTriggerEnter(Collider other)
        {
            Transform t = other.transform;
            while (t != null)
            {
                if (t.name == playerRootName)
                {
                    LoadScene();
                    return;
                }
                t = t.parent;
            }
        }

        private void LoadScene()
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
