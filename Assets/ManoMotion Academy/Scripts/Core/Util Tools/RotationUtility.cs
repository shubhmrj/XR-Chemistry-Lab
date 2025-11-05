using UnityEngine;

namespace ManoMotion
{
    /// <summary>
    /// Helper class to convert joint rotations for Unity.
    /// </summary>
    public static class RotationUtility
    {
        static Quaternion[] HandRotationOffsets = new Quaternion[]
        { 
            Quaternion.Euler(new Vector3(0, 0, 180)), 
            Quaternion.Euler(new Vector3(0, 180, 180))
        };

        /// <summary>
        /// Get corrected hand rotation for the given hand.
        /// </summary>
        public static Quaternion GetHandRotation(this HandInfo handInfo)
        {
            if (handInfo.trackingInfo.worldSkeleton.jointNormalizedRotations == null)
                return Quaternion.identity;

            // Get the rotation of the wrist joint on the world skeleton.
            Quaternion wristRotation = handInfo.trackingInfo.worldSkeleton.jointNormalizedRotations[0];
            LeftOrRightHand hand = handInfo.gestureInfo.leftRightHand;

            if (hand != LeftOrRightHand.LEFT_HAND && hand != LeftOrRightHand.RIGHT_HAND)
                return Quaternion.identity;

            // Get all necessary rotations.
            Quaternion cameraRotation = Camera.main.transform.rotation;
            Quaternion correctedRotation = CorrectHandRotation(wristRotation);
            Quaternion handRotationOffset = HandRotationOffsets[(int)handInfo.gestureInfo.leftRightHand];

            // Multiply rotations together to get the final rotation.
            return cameraRotation * correctedRotation * handRotationOffset;
        }

        /// <summary>
        /// Returns rotation value to add to the hand rotation
        /// </summary>
        /// <param name="handInfo"></param>
        /// <param name="jointIndex"></param>
        /// <returns></returns>
        public static Quaternion GetFingerJointRotation(this HandInfo handInfo, int jointIndex)
        {
            Quaternion[] rotations = handInfo.trackingInfo.worldSkeleton.jointNormalizedRotations;
            if (rotations == null || rotations[jointIndex].IsNaN())
                return Quaternion.identity;

            return CorrectFingerJointRotation(rotations[jointIndex], handInfo.gestureInfo.leftRightHand);
        }

        public static bool IsNaN(this Quaternion q)
        {
            return float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);
        }

        /// <summary>
        /// Corrects the direction of rotation axes depending on which hand it is.
        /// </summary>
        static Quaternion CorrectHandRotation(Quaternion q)
        {
            return new Quaternion(-q.x, q.y, -q.z, q.w);
        }

        static Quaternion CorrectFingerJointRotation(Quaternion q, LeftOrRightHand hand)
        {
            return hand switch
            {
                LeftOrRightHand.LEFT_HAND => new Quaternion(q.x, q.y, -q.z, q.w),
                LeftOrRightHand.RIGHT_HAND => new Quaternion(q.x, q.y, -q.z, -q.w),
                _ => Quaternion.identity
            };
        }
    }
}