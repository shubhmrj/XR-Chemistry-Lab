using UnityEngine;

namespace ManoMotion
{
    /// <summary>
    /// Toggles the background when grabbing.
    /// </summary>
    public class BackgroundToggle : MonoBehaviour
    {
        [SerializeField] ManoGestureTrigger toggleBackgroundTrigger = ManoGestureTrigger.GRAB_GESTURE;
        [SerializeField] MeshRenderer background;
        [SerializeField] Material space, black;

        bool isSpaceBackground = true;

        private void LateUpdate()
        {
            for (int i = 0; i <= ManoMotionManager.Instance.ManomotionSession.enabledFeatures.twoHands; i++)
            {
                if (ManoMotionManager.Instance.TryGetHandInfo((LeftOrRightHand)i, out HandInfo handInfo))
                {
                    bool toggle = handInfo.gestureInfo.manoGestureTrigger.Equals(toggleBackgroundTrigger);
                    if (toggle)
                    {
                        ToggleBackground();
                    }
                }
            }
        }

        private void ToggleBackground()
        {
            isSpaceBackground = !isSpaceBackground;
            background.material = isSpaceBackground ? space : black;
        }
    }
}