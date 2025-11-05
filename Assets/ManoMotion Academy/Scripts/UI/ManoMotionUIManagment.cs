using UnityEngine;
using UnityEngine.UI;

namespace ManoMotion.UI
{
    /// <summary>
    /// Handles the UI, FPS, version, licence etc.
    /// </summary>
    public class ManoMotionUIManagment : MonoBehaviour
    {
        [SerializeField] Text FPSValueText;
        [SerializeField] Text processingTimeValueText;
        [SerializeField] Text versionText;

        private void Start()
        {
            HandleManoMotionManagerInitialized();
        }

        private void OnEnable()
        {
            ManoMotionManager.OnManoMotionFrameProcessed += DisplayInformationAfterManoMotionProcessFrame;
            ManoMotionManager.OnManoMotionLicenseInitialized += HandleManoMotionManagerInitialized;
        }

        private void OnDisable()
        {
            ManoMotionManager.OnManoMotionFrameProcessed -= DisplayInformationAfterManoMotionProcessFrame;
            ManoMotionManager.OnManoMotionLicenseInitialized -= HandleManoMotionManagerInitialized;
        }

        /// <summary>
        /// Displays information from the ManoMotion Manager after the frame has been processed.
        /// </summary>
        void DisplayInformationAfterManoMotionProcessFrame()
        {
            UpdateFPSText();
            UpdateProcessingTime();
        }

        /// <summary>
        /// Toggles the visibility of a Gameobject.
        /// </summary>
        /// <param name="givenObject">Requires a Gameobject</param>
        public void ToggleUIElement(GameObject givenObject)
        {
            givenObject.SetActive(!givenObject.activeInHierarchy);
        }

        /// <summary>
        /// Updates the text field with the calculated Frames Per Second value.
        /// </summary>
        public void UpdateFPSText()
        {
            FPSValueText.text = ManoMotionManager.Instance.Fps.ToString();
        }

        /// <summary>
        /// Updates the text field with the calculated processing time value.
        /// </summary>
        public void UpdateProcessingTime()
        {
            processingTimeValueText.text = ManoMotionManager.Instance.ProcessingTime.ToString() + " ms";
        }

        /// <summary>
        /// Shows the current version of the SDK.
        /// </summary>
        public void HandleManoMotionManagerInitialized()
        {
            versionText.text = "Version PRO ";
            float versionFull = ManoMotionManager.Instance.Version;
            string prefix = "Version PRO ";

            string versionString = versionFull.ToString();

            if (versionString.Length == 4)
            {
                versionString = versionString.Insert(versionString.Length - 1, ".");
            }

            else if (versionString.Length == 5)
            {
                versionString = versionString.Insert(versionString.Length - 2, ".");
                versionString = versionString.Insert(versionString.Length - 1, ".");
            }

            versionText.text = prefix += versionString;
        }
    }
}