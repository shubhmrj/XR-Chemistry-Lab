using UnityEngine;

namespace ManoMotion.Gizmos
{
    /// <summary>
    /// Marker to be left at a position for a specified duration.
    /// </summary>
    public class TriggerMarker : MonoBehaviour
    {
        [SerializeField] float displayTime = 1f;

        float timeDisplayed = 0f;

        private void Update()
        {
            timeDisplayed += Time.deltaTime;

            if (displayTime != 0 && timeDisplayed > displayTime)
            {
                gameObject.SetActive(false);
            }
        }

        public void Activate(Vector3 position)
        {
            timeDisplayed = 0;
            transform.position = position;
            gameObject.SetActive(true);
            transform.SetParent(null);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }
    }
}