using TMPro;
using UnityEngine;

namespace ManoMotion.Gizmos
{
    /// <summary>
    /// Displays which continuous gesture is being performed.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ContinuousGizmo : MonoBehaviour
    {
        [SerializeField] LeftOrRightHand handLeftRight;
        [SerializeField] Color openHandColor, closedHandColor, openPinchColor, closedPinchColor, pointerColor;
        TextMeshProUGUI text;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        void LateUpdate()
        {
            if (ManoMotionManager.Instance.TryGetHandInfo(handLeftRight, out HandInfo handInfo))
            {
                UpdateText(handInfo.gestureInfo.manoGestureContinuous);
                text.text += "\n" + handInfo.gestureInfo.handSide;
            }
        }

        private void UpdateText(ManoGestureContinuous gesture)
        {
            switch (gesture)
            {
                case ManoGestureContinuous.NO_GESTURE:
                    text.text = "";
                    break;
                case ManoGestureContinuous.OPEN_HAND_GESTURE:
                    text.text = "Open hand";
                    text.color = openHandColor;
                    break;
                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
                    text.text = "Closed hand";
                    text.color = closedHandColor;
                    break;
                case ManoGestureContinuous.OPEN_PINCH_GESTURE:
                    text.text = "Open pinch";
                    text.color = openPinchColor;
                    break;
                case ManoGestureContinuous.HOLD_GESTURE:
                    text.text = "Hold";
                    text.color = closedPinchColor;
                    break;
                case ManoGestureContinuous.POINTER_GESTURE:
                    text.text = "Pointer";
                    text.color = pointerColor;
                    break;
                default:
                    break;
            }
        }
    }
}