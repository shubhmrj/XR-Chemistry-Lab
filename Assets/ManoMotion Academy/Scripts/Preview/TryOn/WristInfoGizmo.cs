using UnityEngine;

namespace ManoMotion.TryOn
{
    /// <summary>
    /// Handles the visualization of the wrist width and position.
    /// </summary>
    public class WristInfoGizmo : MonoBehaviour
    {
        [SerializeField] LeftOrRightHand leftRightHand;
        [SerializeField] GameObject leftWrist3D;
        [SerializeField] GameObject rightWrist3D;
        [SerializeField] GameObject tryOnLine;

        private OneEuroFilterSetting positionFilterSetting = new OneEuroFilterSetting(120, 0.0001f, 500f, 1f);
        private OneEuroFilter<Vector3> leftFilter, rightFilter;

        private void Awake()
        {
            leftFilter = new OneEuroFilter<Vector3>(positionFilterSetting);
            rightFilter = new OneEuroFilter<Vector3>(positionFilterSetting);
        }

        private void Update()
        {
            bool foundHand = ManoMotionManager.Instance.TryGetHandInfo(leftRightHand, out HandInfo handInfo, out int handIndex);
            if (foundHand)
                ShowWristInformation(handInfo, handIndex);
            ActivateWristGizmos(foundHand);
        }

        /// <summary>
        /// If SDK should run wrist information ShowWristInformation will calculate the normalized values to fit the hands position.
        /// if no hand is detected the left, right sphere and the tryOnLine will be disabled
        /// </summary>
        public void ShowWristInformation(HandInfo handInfo, int handIndex)
        {
            WristInfo wristInfo = handInfo.trackingInfo.wristInfo;

            float distance = Vector3.Distance(Camera.main.transform.position, GetSkeletonJointPosition(handIndex));
            leftWrist3D.transform.position = leftFilter.Filter(ManoUtils.Instance.CalculateNewPositionWithDepth(wristInfo.leftPoint, distance));
            rightWrist3D.transform.position = rightFilter.Filter(ManoUtils.Instance.CalculateNewPositionWithDepth(wristInfo.rightPoint, distance));
        }

        private Vector3 GetSkeletonJointPosition(int handIndex)
        {
            return SkeletonManager.Instance.GetJoints(handIndex)[0].transform.position;
        }

        private void ActivateWristGizmos(bool status)
        {
            rightWrist3D.SetActive(status);
            leftWrist3D.SetActive(status);
            tryOnLine.SetActive(status);
        }
    }
}