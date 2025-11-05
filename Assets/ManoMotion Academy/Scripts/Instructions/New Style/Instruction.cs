using UnityEngine;

namespace ManoMotion.Instructions.New
{
    public enum GestureAnimation
    {
        None,
        Grab,
        Release,
        Click,
        Pick,
        Drop
    }

    /// <summary>
    /// Component that keeps track of an instruction requirement.
    /// </summary>
    public class Instruction : MonoBehaviour
    {
        /// <summary>
        /// Requirement to fulfill in order to complete the instruction.
        /// </summary>
        [SerializeField] InstructionRequirement requirement;

        /// <summary>
        /// Reference to a GameObject to be activated when the requirement is met.
        /// </summary>
        [SerializeField] GameObject finishedImage;

        /// <summary>
        /// Option to select a gesture animation to be played.
        /// </summary>
        [SerializeField] GestureAnimation gestureAnimation;

        Animator animator;
        bool isTracking = false;
        bool isCompleted = false;

        public bool IsCompleted => isCompleted;

        /// <summary>
        /// Starts tracking the requirement.
        /// </summary>
        public void StartInstruction()
        {
            animator = GetComponent<Animator>();
            isTracking = true;
            isCompleted = false;
            finishedImage.SetActive(false);
            requirement.Start();
            if (!gestureAnimation.Equals(GestureAnimation.None))
            {
                animator.Play(gestureAnimation.ToString());
            }
        }

        /// <summary>
        /// Stops tracking the requirement.
        /// Marks the instruction as completed.
        /// </summary>
        public void StopInstruction()
        {
            isTracking = false;
            isCompleted = true;
            requirement.Stop();
        }

        private void Update()
        {
            if (isTracking && requirement.IsFulfilled())
            {
                StopInstruction();
                finishedImage.SetActive(true);
            }
        }
        
        public void ResetInstruction()
        {
            finishedImage.SetActive(false);
        }
    }
}