using System.Collections.Generic;
using UnityEngine;

namespace ManoMotion.TryOn
{
    /// <summary>
    /// Handles the visualization of the finger width and position.
    /// </summary>
    public class FingerInfoGizmo : MonoBehaviour
    {
        [SerializeField] LeftOrRightHand leftRightHand;
        [SerializeField] GameObject leftFingerPoint3D;
        [SerializeField] GameObject rightFingerPoint3D;
        [SerializeField] GameObject tryOnLine;

        private OneEuroFilterSetting positionFilterSetting = new OneEuroFilterSetting(120, 0.0001f, 500f, 1f);
        private OneEuroFilter<Vector3> leftFilter, rightFilter;

        int[] SettingToSkeletonJoint = { 0, 2, 5, 9, 13, 17 };

        private void Awake()
        {
            leftFilter = new OneEuroFilter<Vector3>(positionFilterSetting);
            rightFilter = new OneEuroFilter<Vector3>(positionFilterSetting);
        }

        private void Update()
        {
            bool foundHand = ManoMotionManager.Instance.TryGetHandInfo(leftRightHand, out HandInfo handInfo, out int handIndex);
            if (foundHand)
                ShowFingerInformation(handInfo, handIndex);
            ActivateFingerGizmos(foundHand);
        }

        /// <summary>
        /// If SDK should run finger information ShowFingerInformation will calculate the normalized values to fit the hands position.
        /// if no hand is detected the left, right sphere and the tryOnLine will be disabled
        /// </summary>
        public void ShowFingerInformation(HandInfo handInfo, int handIndex)
        {
            FingerInfo fingerInfo = handInfo.trackingInfo.fingerInfo;

            float distance = Vector3.Distance(Camera.main.transform.position, GetSkeletonJointPosition(handIndex));
            leftFingerPoint3D.transform.position = leftFilter.Filter(ManoUtils.Instance.CalculateNewPositionWithDepth(fingerInfo.leftPoint, distance));
            rightFingerPoint3D.transform.position = rightFilter.Filter(ManoUtils.Instance.CalculateNewPositionWithDepth(fingerInfo.rightPoint, distance));
        }

        private Vector3 GetSkeletonJointPosition(int handIndex)
        {
            int fingerJoint = SettingToSkeletonJoint[ManoMotionManager.Instance.ManomotionSession.enabledFeatures.fingerInfo];
            return SkeletonManager.Instance.GetJoints(handIndex)[fingerJoint].transform.position;
        }

        private void ActivateFingerGizmos(bool status)
        {
            leftFingerPoint3D.SetActive(status);
            rightFingerPoint3D.SetActive(status);
            tryOnLine.SetActive(status);
        }
    }
}