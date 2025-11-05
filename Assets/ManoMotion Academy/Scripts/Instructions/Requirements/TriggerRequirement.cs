using UnityEngine;

namespace ManoMotion.Instructions
{
    /// <summary>
    /// ScriptableObject instruction requirement for gesture triggers.
    /// </summary>
    [CreateAssetMenu(fileName = "New InteractionRequirement", menuName = "ManoMotion/Instructions/TriggerRequirement")]
    public class TriggerRequirement : InstructionRequirement
    {
        [SerializeField] ManoGestureTrigger triggerGesture;

        public override bool IsFulfilled()
        {
            for (int i = 0; i <= ManoMotionManager.Instance.ManomotionSession.enabledFeatures.twoHands; i++)
            {
                HandInfo handInfo = ManoMotionManager.Instance.HandInfos[i];
                if (handInfo.gestureInfo.manoGestureTrigger.Equals(triggerGesture))
                {
                    return true;
                }
            }
            return false;
        }
    }
}