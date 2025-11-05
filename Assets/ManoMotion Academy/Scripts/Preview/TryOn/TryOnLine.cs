using UnityEngine;

namespace ManoMotion.TryOn
{
    public class TryOnLine : MonoBehaviour
    {
        [SerializeField] LeftOrRightHand leftRightHand;
        [SerializeField] GameObject leftPoint;
        [SerializeField] GameObject rightPoint;
        [SerializeField] LineRenderer tryOnLineRenderer;

        void LateUpdate()
        {
            if (ManoMotionManager.Instance.TryGetHandInfo(leftRightHand, out HandInfo handInfo) && handInfo.gestureInfo.manoClass != ManoClass.NO_HAND)
            {
                DrawOutLine();
            }
            else
            {
                tryOnLineRenderer.SetPosition(0, -Vector3.one);
                tryOnLineRenderer.SetPosition(1, -Vector3.one);
            }
        }

        /// <summary>
        /// Draws a line between the 2 points.
        /// </summary>
        private void DrawOutLine()
        {
            tryOnLineRenderer.SetPosition(0, leftPoint.transform.position);
            tryOnLineRenderer.SetPosition(1, rightPoint.transform.position);
        }
    }
}