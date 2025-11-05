using UnityEngine;

namespace ManoMotion.Instructions
{
    /// <summary>
    /// ScriptableObject instruction requirement for continuous gestures.
    /// </summary>
    [CreateAssetMenu(fileName = "New InteractionRequirement", menuName = "ManoMotion/Instructions/ContinuousRequirement")]
    public class ContinuousRequirement : InstructionRequirement
    {
        [SerializeField] ManoGestureContinuous continuousGesture;
        [SerializeField] float requiredTimeSpent;

        float timeSpent = 0;

        public override bool IsFulfilled()
        {
            bool performedGesture = false;

            for (int i = 0; i <= ManoMotionManager.Instance.ManomotionSession.enabledFeatures.twoHands; i++)
            {
                HandInfo handInfo = ManoMotionManager.Instance.HandInfos[i];
                if (handInfo.gestureInfo.manoGestureContinuous.Equals(continuousGesture))
                {
                    performedGesture = true;
                    timeSpent += Time.deltaTime;
                    break;
                }
            }

            if (!performedGesture)
            {
                timeSpent = 0;
            }

            return timeSpent >= requiredTimeSpent;
        }

        public override void Start()
        {
            timeSpent = 0;
        }

        public override void Stop()
        {
            timeSpent = 0;
        }
    }
}