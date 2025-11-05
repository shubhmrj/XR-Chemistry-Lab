using UnityEngine;
using UnityEngine.Events;

namespace ManoMotion.Gizmos
{
    public class EdgeWarnings : MonoBehaviour
    {
        [SerializeField] RectTransform top, bottom, left, right;
        [SerializeField] UnityEvent<bool> OnWarningActive;

        private void LateUpdate()
        {
            bool topTriggered = false;
            bool bottomTriggered = false;
            bool leftTriggered = false;
            bool rightTriggered = false;

            HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;
            for (int i = 0; i < handInfos.Length; i++)
            {
                topTriggered |= handInfos[i].trackingInfo.skeleton.confidence == 1 && handInfos[i].warning == Warning.WARNING_APPROACHING_UPPER_EDGE;
                bottomTriggered |= handInfos[i].trackingInfo.skeleton.confidence == 1 && handInfos[i].warning == Warning.WARNING_APPROACHING_LOWER_EDGE;
                leftTriggered |= handInfos[i].trackingInfo.skeleton.confidence == 1 && handInfos[i].warning == Warning.WARNING_APPROACHING_LEFT_EDGE;
                rightTriggered |= handInfos[i].trackingInfo.skeleton.confidence == 1 && handInfos[i].warning == Warning.WARNING_APPROACHING_RIGHT_EDGE;
            }

            SetWarning(top, topTriggered);
            SetWarning(bottom, bottomTriggered);
            SetWarning(left, leftTriggered);
            SetWarning(right, rightTriggered);

            OnWarningActive?.Invoke(topTriggered || bottomTriggered || leftTriggered || rightTriggered);
        }

        private void SetWarning(RectTransform warning, bool warningTriggered)
        {
            warning.gameObject.SetActive(warningTriggered);
        }
    }
}