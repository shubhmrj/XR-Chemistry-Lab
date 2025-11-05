using UnityEngine;

namespace ManoMotion.TryOn
{
    public class FingerTryOn : MonoBehaviour
    {
        [SerializeField] LeftOrRightHand leftRightHand;
        [SerializeField] Vector3 rotationOffset;
        [Tooltip("Ring needs to be scaled so the width is the same as a (1, 1, 1) cube.")]
        [SerializeField] float scaleMultiplier = 1f;

        [Space()]
        [Tooltip("When false will use the FingerInfo points to calculate position. When true will calculate position based on the skeleton joint positions.")]
        [SerializeField] bool customPosition = false;
        [Tooltip("At what percentage between the base joint and next joint to position at.")]
        [SerializeField, Range(0, 1)] float customBlendValue = 0.5f;

        MeshRenderer[] meshes;
        int handIndex;
        int fingerIndex;

        int[] SettingToSkeletonJoint = { 0, 2, 5, 9, 13, 17 };

        private void Awake()
        {
            meshes = transform.GetComponentsInChildren<MeshRenderer>();
        }

        void Update()
        {
            fingerIndex = ManoMotionManager.Instance.ManomotionSession.enabledFeatures.fingerInfo;
            bool showing = false;

            if (fingerIndex != 0 && ManoMotionManager.Instance.TryGetHandInfo(leftRightHand, out HandInfo handInfo, out handIndex))
            {
                // While open hand gesture is performed the ring should show.
                if (handInfo.gestureInfo.manoClass == ManoClass.GRAB_GESTURE)
                {
                    //fingerInfoGizmo.ShowFingerInformation(handInfo);
                    ShowRing(handInfo);
                    showing = true;
                }
            }

            if (!showing)
            {
                SetActive(false);
            }
        }

        private void ShowRing(HandInfo handInfo)
        {
            SetActive(true);

            FingerInfo fingerInfo = handInfo.trackingInfo.fingerInfo;
            fingerIndex = SettingToSkeletonJoint[fingerIndex];
            Transform fingerJoint = GetFingerJoint(handIndex, fingerIndex);

            // Calculate the center point
            float distance = Vector3.Distance(Camera.main.transform.position, fingerJoint.position);
            Vector3 left = ManoUtils.Instance.CalculateNewPositionWithDepth(fingerInfo.leftPoint, distance);
            Vector3 right = ManoUtils.Instance.CalculateNewPositionWithDepth(fingerInfo.rightPoint, distance);

            if (customPosition)
            {
                Transform nextJoint = GetFingerJoint(handIndex, fingerIndex + 1);
                transform.position = Vector3.Lerp(fingerJoint.position, nextJoint.position, customBlendValue);
            }
            else
                transform.position = (left + right) / 2;

            // Get the joint rotation, add rotation offset since not all models might be made facing the same way.
            transform.rotation = fingerJoint.rotation * Quaternion.Euler(rotationOffset);

            // Update the scale to match with different distances.
            float fingerWidth = Vector3.Distance(left, right);
            transform.localScale = Vector3.one * fingerWidth * scaleMultiplier;
        }

        private void SetActive(bool active)
        {
            foreach (MeshRenderer mesh in meshes)
            {
                mesh.enabled = active;
            }
        }

        private Transform GetFingerJoint(int handIndex, int fingerJoint)
        {
            return SkeletonManager.Instance.GetJoint(handIndex, fingerJoint).transform;
        }

        /// <summary>
        /// Toggles the finger that should use the ring and also updated the UI text.
        /// </summary>
        public void ToggleFingerForRing()
        {
            fingerIndex = (fingerIndex + 1) % 6;
            ManoMotionManager.Instance.ToggleFingerInfoFinger(fingerIndex);
        }
    }
}
