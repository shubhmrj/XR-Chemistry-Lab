using TMPro;
using UnityEngine;

namespace ManoMotion.Gizmos
{
    /// <summary>
    /// Component to quickly display values for debugging.
    /// </summary>
    public class DebugText : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI debugText;
        [SerializeField] int hand;

        string message = "";

        private void Update()
        {
            HandInfo handInfo = ManoMotionManager.Instance.HandInfos[hand];
            GestureInfo gestureInfo = handInfo.gestureInfo;
            TrackingInfo trackingInfo = handInfo.trackingInfo;

            message = "";
            AddToMessage($"{ManoMotionManager.Instance.Width}x{ManoMotionManager.Instance.Height}");
            AddToMessage(gestureInfo.leftRightHand.ToString());
            AddToMessage(ManoMotionManager.Instance.ManomotionSession.orientation.ToString());
            AddToMessage(Camera.main.transform.rotation.ToString());

            AddToMessage($"Depth: {trackingInfo.depthEstimation}");
            AddToMessage($"Confidence: {trackingInfo.skeleton.confidence}");

            AddToMessage($"ManoClass: {gestureInfo.manoClass}");
            AddToMessage($"Continuous: {gestureInfo.manoGestureContinuous}");
            AddToMessage($"Trigger: {gestureInfo.manoGestureTrigger}");

            debugText.text = message;
        }

        private void AddToMessage(string text)
        {
            message += $"{text}\n";
        }
    }
}