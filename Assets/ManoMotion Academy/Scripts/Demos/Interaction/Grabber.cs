using UnityEngine;

namespace ManoMotion.Demos
{
    /// <summary>
    /// Manages interactions with grabbable objects.
    /// </summary>
    public class Grabber : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] int handIndex;
        [SerializeField]
        ManoGestureTrigger grabTriggerGesture = ManoGestureTrigger.GRAB_GESTURE,
                           releaseGestureTrigger = ManoGestureTrigger.RELEASE_GESTURE,
                           clickGestureTrigger = ManoGestureTrigger.CLICK,
                           pickGestureTrigger = ManoGestureTrigger.PICK,
                           dropGestureTrigger = ManoGestureTrigger.DROP;
        [SerializeField] float castRadius = 0.1f;
        [SerializeField] LayerMask grabLayers;

        [SerializeField] float pickDragStartDistance;

        Grabbable hoveredGrabbable;
        Grabbable grabbedGrabbable;

        Grabbable pickedGrabbable;
        bool pickStarted;
        Vector3 pickStartedPosition;

        ManoGestureTrigger grabType;

        const int GrabJointIndex = 9;

        public Vector3 GrabPosition => GetWorldPosition(GrabJointIndex);
        public Vector3 PinchPosition => ManoUtils.GetCenter(GetWorldPosition(4), GetWorldPosition(8));

        private void LateUpdate()
        {
            // Tries to grab and move a Rigidbody
            if (TryGetHandInfo(out HandInfo handInfo))
            {
                // Stop hover current object
                HoverStop();

                if (handInfo.warning != Warning.NO_WARNING)
                    return;

                ManoGestureTrigger trigger = handInfo.gestureInfo.manoGestureTrigger;

                if (pickStarted)
                {
                    PickUpdate(handInfo, PinchPosition, trigger);
                    return;
                }

                if (grabbedGrabbable)
                {
                    Vector3 position = grabType == grabTriggerGesture ? GrabPosition : PinchPosition;
                    UpdateGrabbed(position, SkeletonManager.Instance.GetHandRotation(handIndex));

                    ManoGestureTrigger releaseTrigger = grabType == grabTriggerGesture ? releaseGestureTrigger : dropGestureTrigger;
                    if (trigger.Equals(releaseTrigger))
                    {
                        Release();
                    }
                    return;
                }

                // Try to grab or pinch a grabbable
                if (!TryGrab(GrabPosition, trigger))
                {
                    Vector3 pinchCenter = ManoUtils.GetCenter(GetWorldPosition(4), GetWorldPosition(8));
                    TryClickOrPick(pinchCenter, trigger);
                }
            }
            else
            {
                HoverStop();
            }
        }

        /// <summary>
        /// Returns true and gives back the hand info of the left/right hand specified.
        /// </summary>
        private bool TryGetHandInfo(out HandInfo handInfo)
        {
            handInfo = ManoMotionManager.Instance.HandInfos[handIndex];
            return !handInfo.gestureInfo.manoClass.Equals(ManoClass.NO_HAND);
        }

        private Vector3 GetWorldPosition(int jointIndex)
        {
            return SkeletonManager.Instance.GetJoint(handIndex, jointIndex).transform.position;
        }

        private bool TryHover(Vector3 position, out Grabbable grabbable)
        {
            grabbable = null;

            Collider[] cols = Physics.OverlapSphere(position, castRadius);
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i].TryGetComponent(out grabbable) && grabbable.CanBeGrabbed)
                {
                    return true;
                }
            }
            return false;
        }

        private bool TryGrab(Vector3 position, ManoGestureTrigger trigger)
        {
            // Start hover grabbable if found
            if (TryHover(position, out Grabbable grabbable))
            {
                // Grab if gesture is triggered
                if (trigger.Equals(grabTriggerGesture))
                {
                    Grab(grabbable, grabTriggerGesture);
                }
                else
                {
                    HoverStart(grabbable);
                }
            }

            return grabbedGrabbable;
        }

        private bool TryClickOrPick(Vector3 position, ManoGestureTrigger trigger)
        {
            // Start hover grabbable if found
            if (TryHover(position, out Grabbable grabbable))
            {
                // Perform click / pick / hover
                if (trigger.Equals(clickGestureTrigger))
                {
                    Click(grabbable);
                }
                else if (trigger.Equals(pickGestureTrigger))
                {
                    Pick(grabbable, position);
                    return true;
                }
                else
                {
                    HoverStart(grabbable);
                }
            }
            return false;
        }

        /// <summary>
        /// Moves the grabbed Rigidbody to the hand with the same distance as when it was grabbed
        /// </summary>
        private void UpdateGrabbed(Vector3 position, Quaternion rotation)
        {
            grabbedGrabbable.Move(position);
            grabbedGrabbable.Rotate(rotation);
        }

        private void HoverStart(Grabbable grabbable)
        {
            HoverStop();
            hoveredGrabbable = grabbable;
            hoveredGrabbable.HoverStart(this);
        }

        private void HoverStop()
        {
            if (hoveredGrabbable)
            {
                hoveredGrabbable.HoverStop(this);
                hoveredGrabbable = null;
            }
        }

        public void Grab(Grabbable grabbable, ManoGestureTrigger gestureTrigger)
        {
            HoverStop();
            grabbedGrabbable = grabbable;
            grabbable.Grab(this);
            grabType = gestureTrigger;
        }

        private void Release()
        {
            HoverStop();
            if (grabbedGrabbable)
            {
                grabbedGrabbable.Release(this);
            }
            grabbedGrabbable = null;
            pickStarted = false;
        }

        private void Click(Grabbable grabbable)
        {
            grabbable.Click(this);
        }

        private void Pick(Grabbable grabbable, Vector3 position)
        {
            pickedGrabbable = grabbable;
            pickStarted = true;
            pickStartedPosition = position;
        }

        private void PickUpdate(HandInfo handInfo, Vector3 position, ManoGestureTrigger trigger)
        {
            if (trigger.Equals(dropGestureTrigger))
            {
                Release();
                return;
            }

            if (Vector3.Distance(pickStartedPosition, position) > pickDragStartDistance)
            {
                DragStart(pickedGrabbable);
            }
        }

        /// <summary>
        /// Called when a Grabbable has been picked and then hand has moved a certain distance.
        /// </summary>
        private void DragStart(Grabbable grabbable)
        {
            grabbable.DragStart(this);
            pickedGrabbable = null;
            pickStarted = false;
        }
    }
}