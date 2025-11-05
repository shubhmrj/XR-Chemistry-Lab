using UnityEngine.UI;
using UnityEngine;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace ManoMotion
{
	/// <summary>
	/// Gives information about the license being used.
	/// </summary>
	public class ManoEvents : MonoBehaviour
	{
		[SerializeField] Animator statusAnimator;
        [SerializeField] Text statusText;
		[SerializeField] Animator lowPowerAnimator;
		[SerializeField] Text lowPowerText;

		void Update()
		{
            HandleManomotionMessages();
			CheckForMode();
        }

        /// <summary>
        /// Interprets the message from the Manomotion Manager class and assigns a string message to be displayed.
        /// </summary>
        void HandleManomotionMessages()
        {
            switch (ManoMotionManager.Instance.LicenseStatus.licenseAnswer)
            {
                case LicenseAnswer.LICENSE_OK:
					break;
                case LicenseAnswer.LICENSE_INTERNET_REQUIRED:
                    DisplayLogMessage("Internet Required");
                    break;
                default:
                    DisplayLogMessage("Contact Support");
                    break;
            }
        }

        /// <summary>
        /// Displays Log messages from the Manomotion Flags 
        /// </summary>
        /// <param name="message">Requires the string message to be displayed</param>
        void DisplayLogMessage(string message)
        {
            statusAnimator.Play("SlideIn");
            statusText.text = message;
        }

		bool showingMode = false;

		void CheckForMode()
		{
            // Check for iOS low power mode
#if UNITY_IOS
			if (Device.lowPowerModeEnabled && !showingMode)
            {
                DisplayLowPowerToast("LowPowerMode Enabled");
                showingMode = true;
            }
            if (!Device.lowPowerModeEnabled)
            {
                showingMode = false;
            }
#endif

            // Check for Android power save mode
#if UNITY_ANDROID
            if (ManoMotionManager.Instance.ManomotionSession.flag == Flags.ANDROID_SAVE_BATTERY_ON && !showingMode)
            {
                DisplayLowPowerToast("PowerSaveMode Enabled");
                showingMode = true;
            }
            if (ManoMotionManager.Instance.ManomotionSession.flag != Flags.ANDROID_SAVE_BATTERY_ON)
            {
                showingMode = false;
            }
#endif
        }

        /// <summary>
        /// Displays the message about LowPowerMode enabled 3 times.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public void DisplayLowPowerToast(string message)
		{
			lowPowerAnimator.Play("SlideInLow");
            lowPowerText.text = message;
		}
	}
}