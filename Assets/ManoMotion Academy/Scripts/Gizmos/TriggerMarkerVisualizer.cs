using UnityEngine;

namespace ManoMotion.Gizmos
{
    /// <summary>
    /// Activates a marker when a specified gesture is triggered.
    /// </summary>
    public class TriggerMarkerVisualizer : MonoBehaviour
    {
        [SerializeField] LeftOrRightHand handLeftRight;
        [SerializeField] ManoGestureTrigger gesture;
        [SerializeField] TriggerMarker marker;

        private void Update()
        {
            if (ManoMotionManager.Instance.TryGetHandInfo(handLeftRight, out HandInfo handInfo))
            {
                GestureInfo gestureInfo = handInfo.gestureInfo;

                if (gestureInfo.manoGestureTrigger.Equals(gesture))
                {
                    marker.Activate(transform.position);
                }
            }
        }
    }
}