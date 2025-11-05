using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace ManoMotion.Demos
{
    public class HandMouseController : MonoBehaviour
    {
        // Cursors to display where pinch is aiming.
        [SerializeField] RectTransform[] cursors;

        private void Update()
        {
            HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;
            bool hasHandCursor = false;

            for (int i = 0; i < handInfos.Length; i++)
            {
                if (ManoMotionManager.Instance.TryGetHandInfo((LeftOrRightHand)i, out HandInfo handInfo))
                {
                    // Make sure the hand is pinching.
                    if (handInfo.gestureInfo.manoGestureContinuous != ManoGestureContinuous.OPEN_PINCH_GESTURE)
                        continue;

                    Vector3 screenPosition = GetCursorScreenPosition(i);
                    cursors[i].position = screenPosition;

                    // Check the gesture trigger.
                    bool clicked = handInfos[i].gestureInfo.manoGestureTrigger.Equals(ManoGestureTrigger.CLICK);

                    HandleInteractionUI(screenPosition, clicked);
                    hasHandCursor = true;
                }
            }

            // No hand was found, stop hovering any previously hovered object.
            if (!hasHandCursor)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        // Returns a screen position between the thumb tip and index finger tip.
        private Vector3 GetCursorScreenPosition(int handIndex)
        {
            // Get the world space positions from the SkeletonManager that already applies smoothing.
            Vector3 pos1 = SkeletonManager.Instance.GetJoint(handIndex, 4).transform.position;
            Vector3 pos2 = SkeletonManager.Instance.GetJoint(handIndex, 8).transform.position;
            Vector3 middle = (pos1 + pos2) / 2;
            Vector3 position = Camera.main.WorldToScreenPoint(middle);
            return position;
        }

        private bool HandleInteractionUI(Vector3 screenPosition, bool click)
        {
            // Use the EventSystem to find UI objects
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current) { position = screenPosition };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            // Look for the first Button component.
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.TryGetComponent(out Button button))
                {
                    EventSystem.current.SetSelectedGameObject(button.gameObject);
                    if (click)
                    {
                        button.onClick.Invoke();
                    }
                    return true;
                }
            }

            // If no Button was found then stop current selection.
            EventSystem.current.SetSelectedGameObject(null);
            return false;
        }
    }
}