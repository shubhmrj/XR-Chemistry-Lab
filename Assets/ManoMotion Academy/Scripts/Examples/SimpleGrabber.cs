using UnityEngine;
using System.Collections.Generic;

namespace ManoMotion
{
    /// <summary>
    /// Example of how to make a script to grab objects.
    /// Can change between grabbing with open/closed hand or pinch.
    /// </summary>
    public class SimpleGrabber : MonoBehaviour
    {
        enum GrabType
        {
            GRAB,
            PINCH
        }

        [SerializeField, Range(0, 1)] int handIndex;
        [SerializeField] GrabType grabType;
        [SerializeField] float castRadius = 0.05f;

        GrabType lastGrabType = (GrabType)(-1);
        ManoGestureTrigger grabTrigger = ManoGestureTrigger.GRAB_GESTURE;
        ManoGestureTrigger releaseTrigger = ManoGestureTrigger.RELEASE_GESTURE;
        int[] fingerJoints;

        HandInfo handInfo;
        Transform grabbedObject;
        bool isGrabbing;

        private void Update()
        {
            UpdateGrabType();

            handInfo = ManoMotionManager.Instance.HandInfos[handIndex];
            ManoGestureTrigger trigger = handInfo.gestureInfo.manoGestureTrigger;

            if (trigger == grabTrigger)
                GrabObject();
            else if (trigger == releaseTrigger)
                DropObject();

            if (isGrabbing)
                MoveObject();
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

        void GrabObject()
        {
            Vector3 position = GetCenterPoint();
            Collider[] cols = Physics.OverlapSphere(position, castRadius);

            if (cols.Length > 0)
            {
                // Grab the first object
                grabbedObject = cols[0].transform;
                isGrabbing = true;
            }
        }

        void MoveObject()
        {
            grabbedObject.position = GetCenterPoint();
        }

        void DropObject()
        {
            grabbedObject = null;
            isGrabbing = false;
        }

        Vector3 GetCenterPoint()
        {
            Vector3 total = new Vector3();
            List<GameObject> joints = SkeletonManager.Instance.GetJoints(handIndex);

            for (int i = 0; i < fingerJoints.Length; i++)
                total += joints[fingerJoints[i]].transform.position;

            return total / fingerJoints.Length;
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