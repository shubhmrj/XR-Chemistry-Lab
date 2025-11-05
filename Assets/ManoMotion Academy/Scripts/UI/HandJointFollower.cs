using UnityEngine;

namespace ManoMotion.UI
{
    /// <summary>
    /// For world space UI to follow a joint and look at the camera.
    /// </summary>
    public class HandJointFollower : MonoBehaviour
    {
        [SerializeField] LeftOrRightHand handLeftRight;
        [SerializeField] bool needCorrectGesture = false, canDeactivate = true;
        [SerializeField] ManoClass gesture;
        [SerializeField, Range(0, 20)] int targetJointIndex = 0;
        [Tooltip("Offset the panel in world space in relation to the main camera")]
        [SerializeField] Vector3 positionOffset;
        [SerializeField] Transform follower;

        Transform cameraTransform;

        private void Awake()
        {
            cameraTransform = Camera.main.transform;
        }

        void Update()
        {
            bool receivedHandInfo = ManoMotionManager.Instance.TryGetHandInfo(handLeftRight, out HandInfo hand, out int handIndex);
            ManoClass manoClass = hand.gestureInfo.manoClass;
            bool foundHand = receivedHandInfo && !manoClass.Equals(ManoClass.NO_HAND);
            bool correctGesture = !needCorrectGesture || manoClass.Equals(gesture);

            // Deactivate if the specified hand can not be found
            if (!foundHand || !correctGesture)
            {
                if (canDeactivate)
                    follower.gameObject.SetActive(false);
                return;
            }

            follower.gameObject.SetActive(true);

            // Get the position of the joint in world space (in relation to the main camera)
            GameObject target = SkeletonManager.Instance.GetJoint(handIndex, targetJointIndex);
            Vector3 jointPosition = target.transform.position;
            Vector3 position = jointPosition + cameraTransform.TransformDirection(positionOffset);
            follower.position = position;
            follower.rotation = cameraTransform.rotation;
        }
    }
}