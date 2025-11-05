using UnityEngine;

namespace ManoMotion.TryOn
{
    public class WristTryOn : MonoBehaviour
    {
        [SerializeField] LeftOrRightHand leftRightHand;
        [SerializeField] Vector3 rotationOffset;
        [Tooltip("Watch needs to be scaled so the width is the same as a (1, 1, 1) cube.")]
        [SerializeField] float scaleMultiplier = 1f;

        void Update()
        {
            Hide();

            if (ManoMotionManager.Instance.TryGetHandInfo(leftRightHand, out HandInfo handInfo, out int handIndex))
            {
                if (handInfo.gestureInfo.manoClass != ManoClass.NO_HAND)
                {
                    Show(handInfo, handIndex);
                }
            } 
        }

        private void Show(HandInfo handInfo, int handIndex)
        {
            WristInfo wristInfo = handInfo.trackingInfo.wristInfo;

            // Gets the position between the 2 finger points from the finger gizmo.
            Transform wristJoint = SkeletonManager.Instance.GetJoint(handIndex, 0).transform;
            float distance = Vector3.Distance(Camera.main.transform.position, wristJoint.position);
            Vector3 left = ManoUtils.Instance.CalculateNewPositionWithDepth(wristInfo.leftPoint, distance);
            Vector3 right = ManoUtils.Instance.CalculateNewPositionWithDepth(wristInfo.rightPoint, distance);
            Vector3 watchPlacement = ManoUtils.GetCenter(left, right);

            // Place the watch at the wrist placement position.
            transform.position = watchPlacement;
            transform.rotation = wristJoint.rotation * Quaternion.Euler(rotationOffset);

            //transform.rotation = wristJoint.transform.rotation * Quaternion.Euler(rotationOffset);

            // Scale the ring with the width from the 2 finger points and multiplyed by a scaleModifier.
            float wristWidth = Vector3.Distance(left, right);
            transform.localScale = Vector3.one * wristWidth * scaleMultiplier;
        }

        /// <summary>
        /// Enabled the outline image and move the ring to -Vector3.one so its not visable.
        /// </summary>
        private void Hide()
        {
            transform.position = -Vector3.one;
        }

        // Get the normal to a triangle from the three corner points, a, b and c.
        Vector3 GetNormalTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            // Find vectors corresponding to two of the sides of the triangle.
            Vector3 side1 = b - a;
            Vector3 side2 = c - a;

            // Cross the vectors to get a perpendicular vector, then normalize it.
            return Vector3.Cross(side1, side2).normalized;
        }
    }
}