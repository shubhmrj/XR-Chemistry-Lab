using UnityEngine;

namespace ManoMotion.Tools
{
    /// <summary>
    /// Add component to a GameObject to close a Windows application with Escape.
    /// </summary>
    public class Escape : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}