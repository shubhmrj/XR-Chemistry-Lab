using UnityEngine;

namespace ManoMotion.Visualization
{
    /// <summary>
    /// Handles the visualization for the hand contour.
    /// </summary>
    public class HandContour : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] int handIndex;
        [SerializeField] LineRenderer contourLineRenderer;
        [SerializeField] bool showContour;

        /// <summary>
        /// If no linerenderer is set this will get the Linerenderer from the GameObject
        /// </summary>
        private void Awake()
        {
            if (contourLineRenderer == null)
            {
                contourLineRenderer = GetComponent<LineRenderer>();
            }
        }

        private void Update()
        {
            contourLineRenderer.enabled = showContour;
            if (showContour)
                ShowContour(ManoMotionManager.Instance.HandInfos[handIndex].trackingInfo);
        }

        /// <summary>
        /// This will calculate the new ContourPoints and set the positions of the LineRenderer.
        /// </summary>
        public void ShowContour(TrackingInfo trackingInfo)
        {
            int count = trackingInfo.numberOfContourPoints;
            Vector3[] newContourPoints = new Vector3[count];

            float contourDepthPosition = GetDepth();

            if (ManoMotionManager.Instance.ManomotionSession.enabledFeatures.contour != 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Vector3 pos = trackingInfo.contourPoints[i];
                    newContourPoints[i] = ManoUtils.Instance.CalculateNewPositionWithDepth(pos, contourDepthPosition);
                    Debug.Log($"From {pos} to {newContourPoints[i]}");
                }

                contourLineRenderer.positionCount = count;
                contourLineRenderer.SetPositions(newContourPoints);
            }
        }

        private float GetDepth()
        {
            GameObject wrist = SkeletonManager.Instance.GetJoint(handIndex, 0);
            return Vector3.Distance(Camera.main.transform.position, wrist.transform.position);
        }

        private void OnEnable()
        {
            ManoMotionManager.OnContourToggle += SetContourActive;
        }

        private void OnDisable()
        {
            ManoMotionManager.OnContourToggle -= SetContourActive;
        }

        private void SetContourActive(bool value)
        {
            showContour = value;
        }
    }
}