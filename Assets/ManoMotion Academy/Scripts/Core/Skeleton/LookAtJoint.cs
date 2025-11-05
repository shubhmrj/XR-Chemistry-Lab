using UnityEngine;

namespace ManoMotion
{
    public class LookAtJoint : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] bool fingerTip = false;

        public void UpdateRotation(Quaternion rotation, HandSide handside)
        {
            transform.rotation = rotation;
            Vector3 forward = transform.forward;

            // Update up direction to properly look at the next joint.
            if (handside == HandSide.Backside)
                transform.up = Up; 
            else
                transform.rotation = Quaternion.LookRotation(forward, Up);
        }

        private Vector3 Up
        {
            get
            {
                if (fingerTip)
                    return transform.position - target.position;
                else
                    return target.position - transform.position;
            }
        }
    }
}