using UnityEngine;

namespace ManoMotion.UI
{
    /// <summary>
    /// For world space UI
    /// </summary>
    public class LookAtCamera : MonoBehaviour
    {
        void Update()
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}