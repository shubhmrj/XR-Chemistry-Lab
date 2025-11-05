using UnityEngine;

namespace ManoMotion.Tools
{
    /// <summary>
    /// Functionality to open a webpage. Used with a Button.
    /// </summary>
    public class WebsiteOpener : MonoBehaviour
    {
        [SerializeField] string url;

        public void OpenWebsite()
        {
            Application.OpenURL(url);
        }
    }
}