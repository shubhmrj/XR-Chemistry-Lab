using UnityEngine;
using UnityEngine.Events;

namespace ManoMotion.Demos
{
    /// <summary>
    /// Recenters the GameObject when enabled.
    /// Mostly used in "scenes" so that the GameObjects appears in front of the camera.
    /// </summary>
    public class Recenterer : MonoBehaviour
    {
        [SerializeField] Vector3 position;
        [SerializeField] UnityEvent OnRecenter;

        private void Awake()
        {
            // To prevent issues where scene is sometimes in front and sometimes behind.
            if (position.z == 0)
                position.z = 0.01f;
        }

        private void OnEnable()
        {
            Recenter();
        }

        public void Recenter()
        {
            Transform camera = Camera.main.transform;

            Vector3 right = camera.right;
            right.y = 0;
            right = right.normalized * position.x;

            Vector3 up = Vector3.up * position.y;

            Vector3 forward = camera.forward;
            forward.y = 0;
            forward = forward.normalized * position.z;

            Vector3 newPosition = camera.position + right + up + forward;

            transform.position = newPosition;
            transform.LookAt(transform.position + forward);

            OnRecenter.Invoke();
        }
    }
}