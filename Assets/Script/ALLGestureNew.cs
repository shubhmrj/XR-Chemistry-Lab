using UnityEngine;
using System.Collections.Generic;

namespace ManoMotion
{
    public class AllGestureNew : MonoBehaviour
    {
        enum GrabType
        {
            GRAB,
            PINCH
        }

        [SerializeField, Range(0, 1)] private int handIndex = 0;
        [SerializeField] private GrabType grabType = GrabType.GRAB;
        [SerializeField] private float castRadius = 0.05f;
        [SerializeField] private LayerMask grabbableLayer;

        private GrabType lastGrabType = (GrabType)(-1);
        private ManoGestureTrigger grabTrigger = ManoGestureTrigger.GRAB_GESTURE;
        private ManoGestureTrigger releaseTrigger = ManoGestureTrigger.RELEASE_GESTURE;
        private int[] fingerJoints;

        private HandInfo handInfo;
        private Transform grabbedObject;
        private Rigidbody grabbedRigidbody;
        private bool isGrabbing;

        private void Update()
        {
            UpdateGrabType();

            handInfo = ManoMotionManager.Instance.HandInfos[handIndex];
            // if (handInfo == null) return;  // ❗ safer null check instead of Equals

            var trigger = handInfo.gestureInfo.manoGestureTrigger;

            if (trigger == grabTrigger)
                TryGrabObject();
            else if (trigger == releaseTrigger)
                ReleaseObject();

            if (isGrabbing && grabbedObject != null)
                MoveGrabbedObject();
        }

        private void UpdateGrabType()
        {
            if (grabType == lastGrabType)
                return;

            switch (grabType)
            {
                case GrabType.GRAB:
                default:
                    grabTrigger = ManoGestureTrigger.GRAB_GESTURE;
                    releaseTrigger = ManoGestureTrigger.RELEASE_GESTURE;
                    fingerJoints = new int[] { 9 };
                    break;

                case GrabType.PINCH:
                    grabTrigger = ManoGestureTrigger.PICK;
                    releaseTrigger = ManoGestureTrigger.DROP;
                    fingerJoints = new int[] { 4, 8 };
                    break;
            }
            lastGrabType = grabType;
        }

        private void TryGrabObject()
        {
            if (isGrabbing) return;

            Vector3 centerPoint = GetCenterPoint();
            Collider[] colliders = Physics.OverlapSphere(centerPoint, castRadius, grabbableLayer);

            if (colliders.Length > 0)
            {
                grabbedObject = colliders[0].transform;
                grabbedRigidbody = grabbedObject.GetComponent<Rigidbody>();

                if (grabbedRigidbody != null)
                    grabbedRigidbody.isKinematic = true;

                isGrabbing = true;
            }
        }

        private void MoveGrabbedObject()
        {
            Vector3 targetPosition = GetCenterPoint();
            grabbedObject.position = Vector3.Lerp(grabbedObject.position, targetPosition, Time.deltaTime * 20f);  // 🚀 Faster, snappier grab
        }

        private void ReleaseObject()
        {
            if (!isGrabbing) return;

            if (grabbedRigidbody != null)
                grabbedRigidbody.isKinematic = false;

            grabbedObject = null;
            grabbedRigidbody = null;
            isGrabbing = false;
        }

        private Vector3 GetCenterPoint()
        {
            Vector3 totalPosition = Vector3.zero;
            List<GameObject> joints = SkeletonManager.Instance.GetJoints(handIndex);

            if (joints == null || joints.Count == 0)
                return totalPosition;

            foreach (int jointIndex in fingerJoints)
            {
                if (jointIndex >= 0 && jointIndex < joints.Count)
                    totalPosition += joints[jointIndex].transform.position;
            }

            return totalPosition / fingerJoints.Length;
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                UnityEngine.Gizmos.DrawWireSphere(GetCenterPoint(), castRadius);
            }
        }
    }
}
